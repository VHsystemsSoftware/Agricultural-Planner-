using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace VHS.Data.Core.Repositories;

public interface IJobRepository : IRepository<Job>
{
	//Task<Job?> GetCurrentJobForTray(Guid trayId, bool setInProgress = false);
	Task<Job?> GetCurrentJobForDate(Guid jobLocationTypeId, DateOnly date, bool setInProgress = false);
	Task<Job?> GetCurrentJobForTray(Guid jobLocationTypeId, Guid trayId);

	Task<Job?> SetJobInProgressAsync(Guid jobID);
	Task<Job?> SetJobCompletedAsync(Guid jobID);
	Task<int> GetNextJobOrderOnDay(Guid jobLocationTypeId, DateOnly scheduledDate);

	Task<Job> GetTransplantJobForBatch(Guid batchId);
}

public class JobRepository : Repository<Job>, IJobRepository
{
	private readonly VHSCoreDBContext _context;

	public JobRepository(VHSCoreDBContext context) : base(context)
	{
		_context = context;
	}

	public override async Task<IEnumerable<Job>> GetAllAsync(Expression<Func<Job, bool>>? filter = null, params string[] includeProperties)
	{
		IQueryable<Job> query = _context.Jobs
			.Include(x => x.JobTrays);

		if (filter != null)
		{
			query = query.Where(filter);
		}

		return await query.ToListAsync();
	}

	public override async Task<Job?> GetByIdAsync(object id)
	{
		return await _context.Jobs
			.Include(x => x.JobTrays)
			.FirstOrDefaultAsync(j => j.Id == (Guid)id);
	}

	public async Task<Job?> GetCurrentJobForDate(Guid jobLocationTypeId, DateOnly date, bool setInProgress = false)
	{
		var job = await _context.Jobs
			.Include(x => x.JobTrays)
			.Include(x => x.Batch.GrowPlan)
			.Include(x => x.JobTrays)
			.Where(x => x.ScheduledDate <= date)
			.Where(x => x.JobLocationTypeId == jobLocationTypeId)
			.Where(x => x.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
				.OrderBy(x => x.ScheduledDate).ThenBy(x => x.OrderOnDay)
			.FirstOrDefaultAsync();

		if (job != null && setInProgress)
			await SetJobInProgressAsync(job.Id);

		return job;
	}

	public async Task<Job?> SetJobInProgressAsync(Guid jobID)
	{
		return await SetJobStatus(jobID, GlobalConstants.JOBSTATUS_INPROGRESS);
	}

	public async Task<Job?> SetJobCompletedAsync(Guid jobID)
	{
		return await SetJobStatus(jobID, GlobalConstants.JOBSTATUS_COMPLETED);
	}

	private async Task<Job?> SetJobStatus(Guid jobID, Guid statusId)
	{
		var job = await _context.Jobs.FindAsync(jobID);
		if (job != null)
		{
			job.StatusId = statusId;
			if (statusId == GlobalConstants.JOBSTATUS_INPROGRESS)
			{
				job.ScheduledDate = DateOnly.FromDateTime(DateTime.Now);
			}

			if (statusId == GlobalConstants.JOBSTATUS_COMPLETED)
			{
				//check for batch/batchplan status finished
				var batch = await _context.Batches
				  .Include(b => b.Jobs)
				  .FirstOrDefaultAsync(b => b.Id == job.BatchId);
				if (batch != null)
				{
					var futureJobsOutstanding = batch.Jobs
					  .Where(x => x.Id != jobID)
					  .Where(x => x.StatusId != GlobalConstants.JOBSTATUS_COMPLETED).Any();
					if (!futureJobsOutstanding)
					{
						batch.StatusId = GlobalConstants.BATCHSTATUS_COMPLETED;

						var batchPlan = await _context.BatchPlans
						  .Include(bp => bp.Batches)
						  .FirstOrDefaultAsync(bp => bp.Id == batch.GrowPlanId);
						if (batchPlan != null)
						{
							var futureBatchOutstanding = batchPlan.Batches
							  .Where(x => x.Id != batch.Id)
							  .Where(x => x.StatusId != GlobalConstants.BATCHSTATUS_COMPLETED).Any();
							if (!futureBatchOutstanding)
							{
								batchPlan.StatusId = GlobalConstants.BATCHPLANSTATUS_FINISHED;
							}
						}
					}
				}
			}

			await _context.SaveChangesAsync();
		}
		return job;
	}

	public async Task<Job?> GetCurrentJobForTray(Guid jobLocationTypeId, Guid trayId)
	{
		var job = await _context.JobTrays
			.Include(jt => jt.Job)
			.Include(jt => jt.Job.JobTrays)
			.Include(jt => jt.Job.Batch.GrowPlan)
			.Include(jt => jt.Tray)
			.Include(jt => jt.DestinationLayer)
			.Include(jt => jt.Recipe)
				.Where(jt => jt.TrayId == trayId)
				.Where(jt => jt.Job.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
				.Where(jt => jt.Job.JobLocationTypeId == jobLocationTypeId)
			.Select(jt => jt.Job)
			.FirstOrDefaultAsync();

		return job;
	}

	public async Task<int> GetNextJobOrderOnDay(Guid jobLocationTypeId, DateOnly scheduledDate)
	{
		var jobsOnDay = await _context.Jobs
			.Where(j => j.ScheduledDate == scheduledDate && j.JobLocationTypeId == jobLocationTypeId)
			.Select(x => new
			{
				x.Id,
				x.OrderOnDay
			})
			.ToListAsync();

		if (jobsOnDay.Any())
			return jobsOnDay.Max(x => x.OrderOnDay) + 1;

		else
			return 1;
	}

	public async Task<Job> GetTransplantJobForBatch(Guid batchId)
	{
		var jobs = await _context.Jobs
				.Where(jt => jt.BatchId == batchId)
				.Where(jt => jt.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
				.Where(jt => jt.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT)
			.Select(jt => jt)
			.SingleOrDefaultAsync();

		return jobs;
	}
}
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
	Task<int> GetNextJobOrderOnDay(DateTime scheduledDate);
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
		var dateOnlyValue = date.ToDateTime(TimeOnly.MinValue).Date;
		var job = await _context.Jobs
			.Include(x => x.JobTrays)
			.Include(x => x.Batch.BatchPlan)
			.Include(x => x.JobTrays)
			.Where(x => x.ScheduledDate.Date == dateOnlyValue)
			.Where(x => x.JobLocationTypeId == jobLocationTypeId)
			.Where(x => x.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
			.OrderBy(x => x.OrderOnDay)
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

			if (statusId == GlobalConstants.JOBSTATUS_COMPLETED)
			{
				//check for batch/batchplan status finished
				var batch = await _context.Batches.FindAsync(job.BatchId);
				if (batch != null)
				{
					var futureJobsOutstanding = batch.Jobs
						.Where(x => x.Id != jobID)
						.Where(x => x.StatusId != GlobalConstants.BATCHSTATUS_COMPLETED).Any();
					if (!futureJobsOutstanding)
					{
						batch.StatusId = GlobalConstants.BATCHSTATUS_COMPLETED;

						var batchPlan = await _context.BatchPlans.FindAsync(batch.BatchPlanId);
						var futureBatchOutstanding = batchPlan.Batches
							.Where(x => x.Id != batch.Id)
							.Where(x => x.StatusId != GlobalConstants.BATCHPLANSTATUS_FINISHED).Any();
						if (!futureBatchOutstanding)
						{
							batchPlan.StatusId = GlobalConstants.BATCHPLANSTATUS_FINISHED;
						}
					}
				}
			}
		}
		return job;
	}

	public async Task<Job?> GetCurrentJobForTray(Guid jobLocationTypeId, Guid trayId)
	{
		var job = await _context.JobTrays
			.Include(jt => jt.Job)
			.Include(x => x.Job.JobTrays)
			.Include(x => x.Job.Batch.BatchPlan)
			.Include(jt => jt.Tray)
			.Include(jt => jt.DestinationLayer)
			.Include(jt => jt.Recipe)
				.Where(jt => jt.TrayId == trayId)
				.Where(jt => jt.Job.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
				.Where(x => x.Job.JobLocationTypeId == jobLocationTypeId)
			.Select(jt => jt.Job)
			.FirstOrDefaultAsync();

		return job;
	}

	public async Task<int> GetNextJobOrderOnDay(DateTime scheduledDate)
	{
		var jobsOnDay = await _context.Jobs
			.Where(j => j.ScheduledDate.Date == scheduledDate.Date)
			.Select(x => new
			{
				x.OrderOnDay
			})
			.ToListAsync();

		if (jobsOnDay.Any())
			return jobsOnDay.Max(x => x.OrderOnDay) + 1;

		else
			return 1;
	}
}
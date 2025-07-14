using Azure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace VHS.Data.Core.Repositories;

public interface IJobTrayRepository : IRepository<JobTray>
{
	Task<JobTray?> GetByJobAndTrayIdAsync(Guid jobId, Guid trayId);
	Task<JobTray?> GetByJobAndTagAsync(Guid jobId, int tag);
	Task<JobTray?> GetByTagAsync(string trayTag);
	Task<JobTray?> GetNextJobTrayAndUpdate(Guid jobId, Guid trayId);
}

public class JobTrayRepository : Repository<JobTray>, IJobTrayRepository
{
	private readonly VHSCoreDBContext _context;

	public JobTrayRepository(VHSCoreDBContext context) : base(context)
	{
		_context = context;
	}

	public override async Task<IEnumerable<JobTray>> GetAllAsync(Expression<Func<JobTray, bool>>? filter = null, params string[] includeProperties)
	{
		IQueryable<JobTray> query = _context.JobTrays;

		if (filter != null)
		{
			query = query.Where(filter);
		}

		return await query.ToListAsync();
	}

	public override async Task<JobTray?> GetByIdAsync(object id)
	{
		return await _context.JobTrays
			.Include(x => x.Tray)
			.Include(x => x.DestinationLayer.Rack.Floor.Farm)
			.FirstOrDefaultAsync(j => j.Id == (Guid)id);
	}

	public async Task<JobTray?> GetByJobAndTagAsync(Guid jobId, int tag)
	{
		return await _context.JobTrays
			.Include(x => x.DestinationLayer)
			.FirstOrDefaultAsync(j => j.JobId == jobId && j.Tray.Tag == tag.ToString());
	}

	public async Task<JobTray?> GetByJobAndTrayIdAsync(Guid jobId, Guid trayId)
	{
		return await _context.JobTrays
			.Include(x => x.DestinationLayer)
			.SingleOrDefaultAsync(j => j.JobId == jobId && j.TrayId == trayId);
	}

	public async Task<JobTray?> GetByTagAsync(string trayTag)
	{
		return await _context.JobTrays
				.Include(x => x.DestinationLayer.Rack.Floor.Farm)
				.Include(x => x.Job).Include(x => x.Tray)
			.Where(x => x.Tray.Tag == trayTag)
			.Where(x => x.Job.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
			.OrderBy(x => x.Job.ScheduledDate).ThenBy(x=>x.OrderInJob)
			.FirstOrDefaultAsync();
	}

	public async Task<JobTray?> GetNextJobTrayAndUpdate(Guid jobId, Guid trayId)
	{
		var jobTray = await _context.JobTrays
				.Include(x => x.Job)
				.Include(x => x.Tray)
				.Include(x => x.Job.Batch.BatchPlan)
				.Include(x => x.DestinationLayer.Rack.Floor.Farm)
				.Include(x => x.Recipe)
			.Where(x => x.Job.Id == jobId)
			.Where(x => x.TrayId == null)
			.OrderBy(x => x.OrderInJob)
			.FirstOrDefaultAsync();

		if (jobTray != null && jobTray.TrayId==null)
		{
			jobTray.TrayId = trayId;

			var children = await _context.JobTrays.Where(x => x.ParentJobTrayId == jobTray.Id).ToListAsync();
			foreach (var child in children)
			{
				child.TrayId = trayId;
			}			
		}
		return jobTray;

	}

}

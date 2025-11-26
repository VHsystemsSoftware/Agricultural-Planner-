using Azure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace VHS.Data.Core.Repositories;

public interface IJobTrayRepository : IRepository<JobTray>
{
	JobTray? GetByJobAndTrayIdAsync(Guid jobId, Guid trayId);
	JobTray? GetNextJobTrayAndUpdate(Guid jobId, Guid trayId);
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
				.Include(x => x.Job)
				.Include(x => x.Tray)
				.Include(x => x.Job.Batch.GrowPlan)
				.Include(x => x.DestinationLayer.Rack.Floor.Farm)
				.Include(x => x.Recipe)
			.FirstOrDefaultAsync(j => j.Id == (Guid)id);
	}


	public JobTray? GetByJobAndTrayIdAsync(Guid jobId, Guid trayId)
	{
		return _context.JobTrays
				.Include(x => x.Job)
				.Include(x => x.Tray)
				.Include(x => x.Job.Batch.GrowPlan)
				.Include(x => x.DestinationLayer.Rack.Floor.Farm)
				.Include(x => x.Recipe)
			.SingleOrDefault(j => j.JobId == jobId && j.TrayId == trayId);
	}

	public JobTray? GetNextJobTrayAndUpdate(Guid jobId, Guid trayId)
	{
		var jobTray = _context.JobTrays
				.Include(x => x.Job)
				.Include(x => x.Tray)
				.Include(x => x.Job.Batch.GrowPlan)
				.Include(x => x.DestinationLayer.Rack.Floor.Farm)
				.Include(x => x.Recipe)
			.Where(x => x.Job.Id == jobId)
			.Where(x => x.TrayId == null)
			.OrderBy(x => x.OrderInJob)
			.FirstOrDefault();

		if (jobTray != null && jobTray.TrayId == null)
		{
			jobTray.TrayId = trayId;

			var children = _context.JobTrays.Where(x => x.ParentJobTrayId == jobTray.Id).ToList();
			foreach (var child in children)
			{
				child.TrayId = trayId;
			}
		}
		return jobTray;

	}

}

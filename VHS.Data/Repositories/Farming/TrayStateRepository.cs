using Microsoft.EntityFrameworkCore;

namespace VHS.Data.Core.Repositories;

public interface ITrayStateRepository : IRepository<TrayState>
{
	Task<List<TrayState>> GetCurrentStates();
	Task CreateNewTrayState(Guid? batchId, Guid trayId);
	Task<List<Tray>> GetFreeTrays();
}

public class TrayStateRepository : Repository<TrayState>, ITrayStateRepository
{
	private readonly VHSCoreDBContext _context;

	public TrayStateRepository(VHSCoreDBContext context) : base(context)
	{
		_context = context;
	}


	public async Task<List<TrayState>> GetCurrentStates()
	{
		return await _context.TrayStates
			.Where(x => x.FinishedDateTime == null)
			.ToListAsync();

	}
	public async Task<List<Tray>> GetFreeTrays()
	{
		return await _context.TrayStates
			.GroupBy(ts => ts.Tray)
			.Where(g => g.All(ts => ts.FinishedDateTime != null))
			.Select(g => g.Key)
			.ToListAsync();
	}
	public async Task CreateNewTrayState(Guid? batchId, Guid trayId)
	{
		var newTrayState = new TrayState()
		{
			TrayId = trayId,
			BatchId = batchId,
			FinishedDateTime = null,
			AddedDateTime = DateTime.UtcNow,
		};
		await _context.TrayStates.AddAsync(newTrayState);
		await _context.SaveChangesAsync();
	}

}

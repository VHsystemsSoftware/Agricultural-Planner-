using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VHS.Data.Core.Repositories;

public interface ITrayRepository : IRepository<Tray>
{
	Task<Guid> FindAndCreateTrayAsync(Guid farmId, uint trayTag, Guid? batchId = null, bool resetState = false);
}

public class TrayRepository : Repository<Tray>, ITrayRepository
{
	private readonly ILogger<TrayRepository> _logger;
	private readonly VHSCoreDBContext _context;
	private readonly ITrayStateRepository _trayStateRepository;

	public TrayRepository(VHSCoreDBContext context, ITrayStateRepository trayStateRepository, ILogger<TrayRepository> logger) : base(context)
	{
		_context = context;
		_trayStateRepository = trayStateRepository;
		_logger = logger;
	}

	public async Task<Guid> FindAndCreateTrayAsync(Guid farmId, uint trayTag, Guid? batchId, bool resetState = false)
	{
		var tray = await _context.Trays
			.Where(x => x.Tag == trayTag.ToString())
			.SingleOrDefaultAsync();
		_logger.LogInformation("Searching for tray with tag {TrayTag} in farm {FarmId}", trayTag, farmId);

		if (tray != null)
		{
			//when there is existing state and we want to reset it
			if (tray.CurrentState != null && resetState)
			{
				var state = await _context.TrayStates
					.Where(x => x.Id == tray.CurrentState.Id).SingleAsync();
				state.FinishedDateTime = DateTime.UtcNow;
				_context.TrayStates.Update(state);

				await _trayStateRepository.CreateNewTrayState(batchId, tray.Id);
			}
			else if (tray.CurrentState == null)
			{
				await _trayStateRepository.CreateNewTrayState(batchId, tray.Id);
			}
			await _context.SaveChangesAsync();
		}
		else
		{
			tray = new Tray()
			{
				Id = Guid.NewGuid(),
				AddedDateTime = DateTime.UtcNow,
				FarmId = farmId,				
				StatusId = GlobalConstants.TRAYSTATUS_INUSE,
				TrayStates = new List<TrayState>(),
				Tag = trayTag.ToString()
			};

			var newTrayState = new TrayState()
			{
				TrayId = tray.Id,
				BatchId = batchId,
				FinishedDateTime = null,
				AddedDateTime = DateTime.UtcNow,
			};

			await _context.Trays.AddAsync(tray);
			await _context.TrayStates.AddAsync(newTrayState);
			await _context.SaveChangesAsync();
		}

		return tray.Id;
	}

}

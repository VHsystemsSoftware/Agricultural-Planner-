using Microsoft.EntityFrameworkCore;


using VHS.Data.Core.Models;
using VHS.Services.Farming.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface ITrayStateService
{
	Task AddTrayStateAudit(Guid trayStateId, Guid auditId);
	Task<TrayState> GetCurrentState(Guid trayId);

	Task<TrayState> ArrivedAtSeederEmpty(TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, Guid? rackTypeId, int? trayCountPerLayer);
	Task<TrayState> ArrivedForSeeding(TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, Guid? rackTypeId, int? trayCountPerLayer, Recipe recipe, Guid? growingDestinationLayerId, Guid? growingTransportLayerId);
	Task ArrivedPaternosterUp(Guid trayId, Guid opcAuditId);
	Task ArrivedHarvesting(Guid trayId, Guid opcAuditId);
	Task ArrivedHarvester(Guid trayId, Guid opcAuditId);
	Task ArrivedGrow(Guid trayId, Guid opcAuditId);
	Task PropagationToTransplant(Guid trayId, Guid opcAuditId);
	Task RegisterWeight(Guid trayId, Guid opcAuditId, float weight);
	Task ArrivedWashingAndFinish(Guid trayId, Guid opcAuditId);
	Task RegisterWillBePushedOutPreGrow(Guid byTrayId, TrayState tray);

	Task<TrayState?> GetOutputTrayPreGrow(Guid excludeTrayId, Guid rackId, Guid layerId);
	Task<TrayState?> GetTransportOutputTrayPreGrow(Guid excludeTrayId, Guid rackId);

	Task RegisterWillBePushedOutGrow(Guid byTrayId, TrayState tray);

	Task<TrayState?> GetOutputTrayGrow(Guid excludeTrayId, Guid rackId, Guid layerId);
	Task<TrayState?> GetTransportOutputTrayGrow(Guid excludeTrayId, Guid rackId);

	Task RemoveTransportLayerIdPreGrow(TrayState trayState);
	Task RemoveTransportLayerIdGrow(TrayState trayState);

	Task<IEnumerable<TrayStateDTO>> GetCurrentStates();
	Task<TrayStateDTO?> GetTrayStateByIdAsync(Guid id);
	Task<TrayStateDTO> UpdateTrayStateAsync(TrayStateDTO dto);

	Task MoveTraysOnLayerPreGrowTransport(Guid layerId);
	Task MoveTraysOnLayerPreGrow(Guid layerId);
	Task MoveTraysOnLayerGrowTransport(Guid layerI);
	Task MoveTraysOnLayerGrow(Guid layerId);
}

public static class TrayStateDTOSelect
{
	public static IQueryable<TrayStateDTO> MapTrayStateToDTO(this IQueryable<TrayState> data)
	{
		var method = System.Reflection.MethodBase.GetCurrentMethod();
		return data.TagWith(method.Name)
		.Select(x => new TrayStateDTO
		{
			Id = x.Id,
			TrayId = x.TrayId,
			BatchId = x.BatchId,
			PreGrowLayerId = x.PreGrowLayerId,
			GrowLayerId = x.GrowLayerId,
			RecipeId = x.RecipeId,
			TrayTag = x.Tray.Tag,
			GrowLayerName = x.GrowLayer != null ? $"{x.GrowLayer.Rack.Floor.Name}-{x.GrowLayer.Rack.Name}-{x.GrowLayer.Number}" : string.Empty,
			PreGrowLayerName = x.PreGrowLayer != null ? $"{x.PreGrowLayer.Rack.Floor.Name}-{x.PreGrowLayer.Rack.Name}-{x.PreGrowLayer.Number}" : string.Empty,
			PreGrowFinishedDate = x.PreGrowFinishedDate,
			GrowFinishedDate = x.GrowFinishedDate,
			ArrivedAtSeeder = x.ArrivedAtSeeder,
			ArrivedGrow = x.ArrivedGrow,
			ArrivedHarvest = x.ArrivedHarvest,
			ArrivedWashing = x.ArrivedWashing,
			ArrivedPaternosterUp = x.ArrivedPaternosterUp,
			EmptyReason = x.EmptyReason,
			EmptyToTransplant = x.EmptyToTransplant,
			FinishedDateTime = x.FinishedDateTime,
			HarvestedWeightKG = x.HarvestedWeightKG,
			SeedDate = x.SeedDate,
			PropagationToTransplant = x.PropagationToTransplant,
			TransportToHarvest = x.TransportToHarvest,
			TransportToPaternosterUp = x.TransportToPaternosterUp,
			TransportToWashing = x.TransportToWashing,
			WeightRegistered = x.WeightRegistered,
			GrowOrderOnLayer = x.GrowOrderOnLayer,
			PreGrowOrderOnLayer = x.PreGrowOrderOnLayer,
			WillBePushedOutGrow = x.WillBePushedOutGrow,
			WillBePushedOutPreGrow = x.WillBePushedOutPreGrow,
			Recipe = x.RecipeId.HasValue ? new RecipeDTO
			{
				Id = x.Recipe.Id,
				Name = x.Recipe.Name
			} : null
		});
	}
}

public class TrayStateService : ITrayStateService
{
	private readonly IUnitOfWorkCore _unitOfWork;

	public TrayStateService(IUnitOfWorkCore unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<TrayState> GetCurrentState(Guid trayId)
	{
		return await _unitOfWork.TrayState
				.Query(x => x.TrayId == trayId && x.FinishedDateTime == null)
				.SingleAsync();
	}

	public Task AddTrayStateAudit(Guid trayStateId, Guid auditId)
	{
		var audit = new TrayStateAudit
		{
			Id = Guid.NewGuid(),
			TrayStateId = trayStateId,
			OPCAuditId = auditId,
			AddedDateTime = DateTime.UtcNow
		};
		return _unitOfWork.TrayStateAudit.AddAsync(audit);
	}

	public async Task<TrayState> ArrivedAtSeederEmpty(TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, Guid? rackTypeId, int? trayCountPerLayer)
	{	
		var now = DateTime.UtcNow;
		var today = DateOnly.FromDateTime(now.Date);

		trayCurrentState.ArrivedAtSeeder = now;

		trayCurrentState.RecipeId = null;
		trayCurrentState.SeedDate = null;

		if (JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOWASHER)
		{
			trayCurrentState.TransportToWashing = now; // the tray is transported to the washing machine
			trayCurrentState.EmptyReason = GlobalConstants.TRAYSTATE_EMPTYREASON_TOWASHING;
		}
		else if (JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT)
		{
			trayCurrentState.EmptyToTransplant = now; // the tray is transported to the transplanting machine
			trayCurrentState.EmptyReason = GlobalConstants.TRAYSTATE_EMPTYREASON_TOTRANSPLANT;
		}
		else if (JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TORACK || destinationLayerId != null)
		{
			if (rackTypeId == GlobalConstants.RACKTYPE_GERMINATION || rackTypeId == GlobalConstants.RACKTYPE_PROPAGATION)
			{
				trayCurrentState.PreGrowLayerId = destinationLayerId; // the layer where the tray will be placed after seeding
				trayCurrentState.PreGrowFinishedDate = null;
				trayCurrentState.PreGrowOrderOnLayer = trayCountPerLayer;
				trayCurrentState.PreGrowTransportLayerId = transportLayerId;
				trayCurrentState.GrowLayerId = null;
			}
			else
			{
				trayCurrentState.GrowLayerId = destinationLayerId; // the layer where the tray will be placed after seeding
				trayCurrentState.GrowTransportLayerId = transportLayerId;
				trayCurrentState.PreGrowLayerId = null;
			}
		}
		return trayCurrentState;

	}

	public async Task<TrayState> ArrivedForSeeding(TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, 
		Guid? rackTypeId, int? trayCountPerLayer, 
		Recipe recipe, Guid? growingDestinationLayerId, Guid? growingTransportLayerId)
	{
		var now = DateTime.UtcNow;
		var today = DateOnly.FromDateTime(now.Date);

		trayCurrentState.ArrivedAtSeeder = now;

		trayCurrentState.RecipeId = recipe.Id;
		trayCurrentState.SeedDate = today;

		trayCurrentState.PreGrowLayerId = destinationLayerId; //the layer where the tray will be placed after seeding
		trayCurrentState.PreGrowFinishedDate = today.AddDays(recipe.PreGrowDays);
		trayCurrentState.PreGrowOrderOnLayer = trayCountPerLayer;
		trayCurrentState.PreGrowTransportLayerId = transportLayerId;

		if (growingDestinationLayerId.HasValue)
		{
			trayCurrentState.GrowLayerId = growingDestinationLayerId;
			trayCurrentState.GrowFinishedDate = today.AddDays(recipe.PreGrowDays + recipe.GrowDays);
			trayCurrentState.GrowTransportLayerId = growingTransportLayerId;
		}

		return trayCurrentState;
	}

	public async Task ArrivedWashingAndFinish(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.ArrivedWashing = DateTime.UtcNow;
		trayCurrentState.FinishedDateTime = DateTime.UtcNow;
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}

	public async Task ArrivedPaternosterUp(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.ArrivedPaternosterUp = DateTime.UtcNow;
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}

	public async Task ArrivedHarvesting(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.TransportToHarvest = DateTime.UtcNow;// next stop is harvest machine
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}
	public async Task ArrivedHarvester(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.ArrivedHarvest = DateTime.UtcNow;
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}
	public async Task ArrivedGrow(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.ArrivedGrow = DateTime.UtcNow;
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}

	public async Task PropagationToTransplant(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.PropagationToTransplant = DateTime.UtcNow;
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}

	public async Task RegisterWeight(Guid trayId, Guid opcAuditId, float weight)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		trayCurrentState.WeightRegistered = DateTime.UtcNow;
		trayCurrentState.HarvestedWeightKG = weight;
		await AddTrayStateAudit(trayCurrentState.Id, opcAuditId);
	}
	public async Task RegisterWillBePushedOutPreGrow(Guid byTrayId, TrayState trayState)
	{
		trayState.PreGrowPushedOutByTrayId = byTrayId;
		trayState.WillBePushedOutPreGrow = DateTime.UtcNow;
	}
	public async Task RegisterWillBePushedOutGrow(Guid byTrayId, TrayState trayState)
	{
		trayState.GrowPushedOutByTrayId = byTrayId;
		trayState.WillBePushedOutGrow = DateTime.UtcNow;
	}

	public async Task<TrayState?> GetOutputTrayPreGrow(Guid excludeTrayId, Guid rackId, Guid layerId)
	{
		var tray = await _unitOfWork.TrayState
			.Query(x =>
				x.PreGrowLayer.RackId == rackId
				&& x.PreGrowLayerId == layerId
				&& x.FinishedDateTime == null
				&& x.PreGrowTransportLayerId == null
				&& x.TrayId != excludeTrayId
				&& x.PreGrowOrderOnLayer == 1)
			.SingleOrDefaultAsync();

		return tray;
	}

	public async Task<TrayState?> GetOutputTrayGrow(Guid excludeTrayId, Guid rackId, Guid layerId)
	{
		var tray = await _unitOfWork.TrayState
			.Query(x =>
				x.GrowLayer.RackId == rackId
				&& x.GrowLayerId == layerId
				&& x.FinishedDateTime == null
				&& x.GrowTransportLayerId == null
				&& x.TrayId != excludeTrayId
				&& x.GrowOrderOnLayer == 1)
			.SingleOrDefaultAsync();

		return tray;
	}

	public async Task<IEnumerable<TrayStateDTO>> GetCurrentStates()
	{
		return await _unitOfWork.TrayState
			.Query(x => x.FinishedDateTime == null)
			.MapTrayStateToDTO()
			.AsNoTracking()
			.OrderBy(p => p.TrayTag)
			.ToListAsync();
	}

	public async Task<TrayState?> GetTransportOutputTrayPreGrow(Guid excludeTrayId, Guid rackId)
	{
		var tray = await _unitOfWork.TrayState
			.Query(x =>
				x.PreGrowLayer.RackId == rackId
				&& x.PreGrowTransportLayerId != null
				&& x.FinishedDateTime == null
				&& x.TrayId != excludeTrayId
				&& x.PreGrowOrderOnLayer == 1)
			.SingleOrDefaultAsync();

		return tray;
	}

	public async Task<TrayState?> GetTransportOutputTrayGrow(Guid excludeTrayId, Guid rackId)
	{
		var tray = await _unitOfWork.TrayState
			.Query(x =>
				x.GrowLayer.RackId == rackId
				&& x.GrowTransportLayerId != null
				&& x.FinishedDateTime == null
				&& x.TrayId != excludeTrayId
				&& x.GrowOrderOnLayer == 1)
			.SingleOrDefaultAsync();

		return tray;
	}

	public async Task RemoveTransportLayerIdPreGrow(TrayState trayState)
	{
		trayState.PreGrowTransportLayerId = null;
	}

	public async Task RemoveTransportLayerIdGrow(TrayState trayState)
	{
		trayState.GrowTransportLayerId = null;
	}

	public async Task<TrayStateDTO?> GetTrayStateByIdAsync(Guid id)
	{
		return await _unitOfWork.TrayState
			.Query(x => x.Id == id)
			.MapTrayStateToDTO()
			.AsNoTracking()
			.FirstOrDefaultAsync();
	}

	public async Task<TrayStateDTO> UpdateTrayStateAsync(TrayStateDTO dto)
	{
		var tray = await _unitOfWork.TrayState.GetByIdAsync(dto.Id);
		if (tray == null)
			throw new Exception("TrayState not found");

		tray.TrayId = dto.TrayId;
		tray.BatchId = dto.BatchId;
		tray.PreGrowLayerId = dto.PreGrowLayerId;
		tray.GrowLayerId = dto.GrowLayerId;
		tray.RecipeId = dto.RecipeId;

		tray.Tray.Tag = dto.TrayTag;
		tray.EmptyReason = dto.EmptyReason;
		tray.HarvestedWeightKG = dto.HarvestedWeightKG;
		tray.WeightRegistered = dto.WeightRegistered;

		tray.SeedDate = dto.SeedDate;
		tray.PreGrowFinishedDate = dto.PreGrowFinishedDate;
		tray.GrowFinishedDate = dto.GrowFinishedDate;

		tray.ArrivedAtSeeder = dto.ArrivedAtSeeder;
		tray.ArrivedGrow = dto.ArrivedGrow;
		tray.TransportToHarvest = dto.TransportToHarvest;
		tray.ArrivedHarvest = dto.ArrivedHarvest;

		tray.EmptyToTransplant = dto.EmptyToTransplant;
		tray.PropagationToTransplant = dto.PropagationToTransplant;
		tray.TransportToWashing = dto.TransportToWashing;
		tray.ArrivedWashing = dto.ArrivedWashing;

		tray.TransportToPaternosterUp = dto.TransportToPaternosterUp;
		tray.ArrivedPaternosterUp = dto.ArrivedPaternosterUp;

		tray.WillBePushedOutPreGrow = dto.WillBePushedOutPreGrow;
		tray.WillBePushedOutGrow = dto.WillBePushedOutGrow;

		tray.FinishedDateTime = dto.FinishedDateTime;

		_unitOfWork.TrayState.Update(tray);
		await _unitOfWork.SaveChangesAsync();

		return await _unitOfWork.TrayState
			.Query(x => x.Id == tray.Id)
			.MapTrayStateToDTO()
			.AsNoTracking()
			.FirstAsync();
	}

	public async Task MoveTraysOnLayerPreGrowTransport(Guid layerId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.PreGrowTransportLayerId == layerId
				&& x.PreGrowTransportLayerId != null
				&& x.PreGrowOrderOnLayer > 0)
			.ToListAsync();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.PreGrowOrderOnLayer))
		{
			tray.PreGrowOrderOnLayer -= 1; //move one place up
		}
	}

	public async Task MoveTraysOnLayerPreGrow(Guid layerId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.PreGrowLayerId == layerId
				&& x.PreGrowTransportLayerId == null
				&& x.PreGrowOrderOnLayer > 0)
			.ToListAsync();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.PreGrowOrderOnLayer))
		{
			tray.PreGrowOrderOnLayer -= 1; //move one place up
		}
	}

	public async Task MoveTraysOnLayerGrowTransport(Guid layerId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.GrowTransportLayerId == layerId
			&& x.GrowTransportLayerId != null
			&& x.GrowOrderOnLayer > 0)
			.ToListAsync();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.GrowOrderOnLayer))
		{
			tray.GrowOrderOnLayer -= 1; //move one place up
		}
	}

	public async Task MoveTraysOnLayerGrow(Guid layerId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.GrowLayerId == layerId
			&& x.GrowTransportLayerId == null
			&& x.GrowOrderOnLayer > 0)
			.ToListAsync();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.GrowOrderOnLayer))
		{
			tray.GrowOrderOnLayer -= 1; //move one place up
		}
	}


}

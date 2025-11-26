using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHS.Data.Core.Models;
using VHS.Services.Audit;
using VHS.Services.Audit.DTO;
using VHS.Services.Farming.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface ITrayStateService
{
	Task<TrayState> GetCurrentState(Guid trayId);
	Task<TrayState> ArrivedAtSeeder(DateTime date, Guid auditId, JobTray trayJobInfo);
	Task<TrayState> ArrivedAtSeederEmpty(DateTime date, TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, Guid? rackTypeId, int? trayCountPerLayer);
	Task<TrayState> ArrivedForSeeding(DateTime date, TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, Guid? rackTypeId, int? trayCountPerLayer, Recipe recipe, Guid? growingDestinationLayerId, Guid? growingTransportLayerId);
	Task<TrayState> ArrivedForTransplant(DateTime date, TrayState trayCurrentState, Recipe recipe, Guid? growingDestinationLayerId, Guid? growingTransportLayerId);
	
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
    Task<IEnumerable<TrayStateDTO>> GetCurrentStates(Guid batchId);

    Task<TrayStateDTO?> GetTrayStateByIdAsync(Guid id);
	Task<TrayStateDTO> UpdateTrayStateAsync(TrayStateDTO dto, string userId);

	Task MoveTraysOnLayerPreGrowTransport(Guid layerId, Guid? batchId);
	Task MoveTraysOnLayerPreGrow(Guid layerId, Guid? batchId);
	Task MoveTraysOnLayerGrowTransport(Guid layerId, Guid? batchId);
	Task MoveTraysOnLayerGrow(Guid layerId, Guid? batchId);

	Task<bool> CheckDoubleSeedTray(string trayTag, Guid batchId);

	Task<IEnumerable<TrayStateDTO>> GetCurrentStatesForJob(Guid jobId);
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
			PreGrowLayerNumber = x.PreGrowLayerId.HasValue ? x.PreGrowLayer.Number : null,
			GrowLayerNumber = x.GrowLayerId.HasValue ? x.GrowLayer.Number : null,
			GrowLayerName = x.GrowLayerId.HasValue ? $"{x.GrowLayer.Rack.Floor.Name}-{x.GrowLayer.Rack.Name}-{x.GrowLayer.Number}" : string.Empty,
			PreGrowLayerName = x.PreGrowLayerId.HasValue ? $"{x.PreGrowLayer.Rack.Floor.Name}-{x.PreGrowLayer.Rack.Name}-{x.PreGrowLayer.Number}" : string.Empty,
			GrowTransportLayerName = x.GrowTransportLayer != null ? $"{x.GrowTransportLayer.Rack.Floor.Name}-{x.GrowTransportLayer.Rack.Name}-{x.GrowTransportLayer.Number}" : string.Empty,
			PreGrowTransportLayerName = x.PreGrowTransportLayer != null ? $"{x.PreGrowTransportLayer.Rack.Floor.Name}-{x.PreGrowTransportLayer.Rack.Name}-{x.PreGrowTransportLayer.Number}" : string.Empty,
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
			AddedDateTime = x.AddedDateTime,
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
	private readonly IAuditLogService _auditLogService;

    public TrayStateService(IUnitOfWorkCore unitOfWork, IAuditLogService auditLogService)
	{
		_unitOfWork = unitOfWork;
		_auditLogService = auditLogService;
    }

	public async Task<TrayState> GetCurrentState(Guid trayId)
	{
		return await _unitOfWork.TrayState
				.Query(x => x.TrayId == trayId && x.FinishedDateTime == null)
				.Include(x=>x.Recipe.Product)
				.SingleAsync();
	}

	public async Task<TrayState> ArrivedAtSeeder(DateTime date, Guid auditId, JobTray trayJobInfo)
	{
		var trayCurrentState = await GetCurrentState(trayJobInfo.TrayId.Value);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		if (!trayJobInfo.RecipeId.HasValue)
		{
			trayCurrentState = await ArrivedAtSeederEmpty(
				date,
				trayCurrentState,
				trayJobInfo.Job.JobTypeId,
				trayJobInfo.DestinationLayerId,
				trayJobInfo.TransportLayerId,
				trayJobInfo.DestinationLayer?.Rack?.TypeId,
				trayJobInfo.DestinationLayer?.Rack?.TrayCountPerLayer);
		}
		else
		{	
			if (trayJobInfo.Job.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT)
			{
				var growingJobTray = await _unitOfWork.JobTray.Query(x => x.TrayId == trayJobInfo.TrayId
					&& !x.ParentJobTrayId.HasValue
					&& x.DestinationLayer.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING).SingleOrDefaultAsync();

				trayCurrentState = await ArrivedForTransplant(
					date,
					trayCurrentState,
					trayJobInfo.Recipe,
					growingJobTray?.DestinationLayerId,
					growingJobTray?.TransportLayerId);
			}
			else
			{
				var growingJobTray = await _unitOfWork.JobTray.Query(x =>
					x.ParentJobTrayId == trayJobInfo.Id
					&& x.DestinationLayer.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING)
					.SingleOrDefaultAsync();

				trayCurrentState = await ArrivedForSeeding(
					date,
					trayCurrentState,
					trayJobInfo.Job.JobTypeId,
					trayJobInfo.DestinationLayerId,
					trayJobInfo.TransportLayerId,
					trayJobInfo.DestinationLayer?.Rack?.TypeId,
					trayJobInfo.DestinationLayer?.Rack?.TrayCountPerLayer,
					trayJobInfo.Recipe,
					growingJobTray?.DestinationLayerId,
					growingJobTray?.TransportLayerId);
			}
		}
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
		return trayCurrentState;
	}

	public async Task<TrayState> ArrivedAtSeederEmpty(DateTime date, TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId, Guid? rackTypeId, int? trayCountPerLayer)
	{
		trayCurrentState.ArrivedAtSeeder = date;

		trayCurrentState.RecipeId = null;
		trayCurrentState.SeedDate = null;

		if (JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOWASHER)
		{
			trayCurrentState.TransportToWashing = date; // the tray is transported to the washing machine
			trayCurrentState.EmptyReason = GlobalConstants.TRAYSTATE_EMPTYREASON_TOWASHING;
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

	public async Task<TrayState> ArrivedForTransplant(DateTime date, TrayState trayCurrentState, Recipe recipe, Guid? growingDestinationLayerId, Guid? growingTransportLayerId)
	{
		var today = DateOnly.FromDateTime(date.Date);

		trayCurrentState.ArrivedAtSeeder = date;

		trayCurrentState.RecipeId = recipe.Id;
		trayCurrentState.SeedDate = today;

		trayCurrentState.PreGrowLayerId = null;
		trayCurrentState.PreGrowFinishedDate = null;
		trayCurrentState.PreGrowOrderOnLayer = null;
		trayCurrentState.PreGrowTransportLayerId = null;

		trayCurrentState.GrowLayerId = growingDestinationLayerId;
		trayCurrentState.GrowFinishedDate = today.AddDays(recipe.PreGrowDays + recipe.GrowDays);
		trayCurrentState.GrowTransportLayerId = growingTransportLayerId;

		return trayCurrentState;
	}

	public async Task<TrayState> ArrivedForSeeding(DateTime date, TrayState trayCurrentState, Guid JobTypeId, Guid? destinationLayerId, Guid? transportLayerId,
		Guid? rackTypeId, int? trayCountPerLayer,
		Recipe recipe, Guid? growingDestinationLayerId, Guid? growingTransportLayerId)
    {
        var today = DateOnly.FromDateTime(date.Date);

        trayCurrentState.ArrivedAtSeeder = date;

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
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		trayCurrentState.ArrivedWashing = DateTime.UtcNow;
		trayCurrentState.FinishedDateTime = DateTime.UtcNow;
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task ArrivedPaternosterUp(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		trayCurrentState.ArrivedPaternosterUp = DateTime.UtcNow;
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task ArrivedHarvesting(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		trayCurrentState.TransportToHarvest = DateTime.UtcNow;// next stop is harvest machine
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task ArrivedHarvester(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		trayCurrentState.ArrivedHarvest = DateTime.UtcNow;
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task ArrivedGrow(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		trayCurrentState.ArrivedGrow = DateTime.UtcNow;
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task PropagationToTransplant(Guid trayId, Guid opcAuditId)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		trayCurrentState.PropagationToTransplant = DateTime.UtcNow;
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task RegisterWeight(Guid trayId, Guid opcAuditId, float weight)
	{
		var trayCurrentState = await GetCurrentState(trayId);
		var oldDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
		if (trayCurrentState.RecipeId.HasValue)
		{
			trayCurrentState.WeightRegistered = DateTime.UtcNow;
			trayCurrentState.HarvestedWeightKG = weight;
		}
		var result = await _unitOfWork.SaveChangesAsync();
		if (result > 0)
		{
			var newDto = await GetTrayStateByIdAsync(trayCurrentState.Id);
			await CreateAuditLogAsync("Modified", "SYSTEM", trayCurrentState.Id, oldDto, newDto);
		}
	}
	public async Task RegisterWillBePushedOutPreGrow(Guid byTrayId, TrayState trayState)
	{
		trayState.PreGrowPushedOutByTrayId = byTrayId;
		//trayState.PreGrowOrderOnLayer = null;
		trayState.WillBePushedOutPreGrow = DateTime.UtcNow;
	}
	public async Task RegisterWillBePushedOutGrow(Guid byTrayId, TrayState trayState)
	{
		trayState.GrowPushedOutByTrayId = byTrayId;
		//trayState.GrowOrderOnLayer = null;
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

    public async Task<IEnumerable<TrayStateDTO>> GetCurrentStates(Guid batchId)
    {
        return await _unitOfWork.TrayState
            .Query(x => x.BatchId == batchId)
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
	public async Task<TrayStateDTO> UpdateTrayStateAsync(TrayStateDTO dto, string userId)
	{
		var tray = await _unitOfWork.TrayState.GetByIdAsync(dto.Id);
		if (tray == null)
			throw new Exception("TrayState not found");

		var oldDto = await GetTrayStateByIdAsync(dto.Id);

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

		tray.GrowOrderOnLayer = dto.GrowOrderOnLayer;
		tray.PreGrowOrderOnLayer = dto.PreGrowOrderOnLayer;

		_unitOfWork.TrayState.Update(tray);
		var result = await _unitOfWork.SaveChangesAsync();

		TrayStateDTO newDto = oldDto;
		if (result > 0)
		{
			newDto = await GetTrayStateByIdAsync(dto.Id);
			await CreateAuditLogAsync("Modified", userId, tray.Id, oldDto, newDto);
		}

		return newDto;
	}
	public async Task MoveTraysOnLayerPreGrowTransport(Guid layerId, Guid? batchId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.PreGrowTransportLayerId == layerId
						&& x.PreGrowTransportLayerId != null
						&& x.PreGrowOrderOnLayer > 0
						&& x.FinishedDateTime == null)
			.ToListAsync();

		//var layer = await _unitOfWork.Layer.GetFirstOrDefaultAsync(x=>x.Id == layerId);
		//layer.BatchId = batchId;

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.PreGrowOrderOnLayer))
		{
			tray.PreGrowOrderOnLayer -= 1; //move one place up
		}
	}
	public async Task MoveTraysOnLayerPreGrow(Guid layerId, Guid? batchId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.PreGrowLayerId == layerId
						&& x.PreGrowTransportLayerId == null
						&& x.PreGrowOrderOnLayer > 0
						&& x.FinishedDateTime == null)
			.ToListAsync();

		//var layer = await _unitOfWork.Layer.GetFirstOrDefaultAsync(x => x.Id == layerId);
		//layer.BatchId = batchId;

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.PreGrowOrderOnLayer))
		{
			tray.PreGrowOrderOnLayer -= 1; //move one place down
		}
	}
	public async Task MoveTraysOnLayerGrowTransport(Guid layerId, Guid? batchId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.GrowTransportLayerId == layerId
					&& x.GrowTransportLayerId != null
					&& x.GrowOrderOnLayer > 0
					&& x.FinishedDateTime == null)
			.ToListAsync();

		//var layer = await _unitOfWork.Layer.GetFirstOrDefaultAsync(x => x.Id == layerId);
		//layer.BatchId = batchId;

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.GrowOrderOnLayer))
		{
			tray.GrowOrderOnLayer -= 1; //move one place down
		}
	}
	public async Task MoveTraysOnLayerGrow(Guid layerId, Guid? batchId)
	{
		var traysOnLayer = await _unitOfWork.TrayState
			.Query(x => x.GrowLayerId == layerId
					&& x.GrowTransportLayerId == null
					&& x.GrowOrderOnLayer > 0
					&& x.FinishedDateTime == null)
			.ToListAsync();

		//var layer = await _unitOfWork.Layer.GetFirstOrDefaultAsync(x => x.Id == layerId);
		//layer.BatchId = batchId;

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.GrowOrderOnLayer))
		{
			tray.GrowOrderOnLayer -= 1; //move one place down
		}
	}
	public async Task<bool> CheckDoubleSeedTray(string trayTag, Guid batchId)
	{
		var trayState = await _unitOfWork.TrayState
			.Query(x => x.Tray.Tag == trayTag
				&& x.BatchId== batchId
				&& x.FinishedDateTime == null
				&& x.ArrivedHarvest == null
				&& x.ArrivedPaternosterUp == null
				&& x.ArrivedAtSeeder != null)
			.OrderByDescending(x => x.AddedDateTime)
			.FirstOrDefaultAsync();

		return trayState != null;
	}

	private async Task CreateAuditLogAsync(string action, string userId, Guid entityId, TrayStateDTO? oldDto, TrayStateDTO? newDto)
	{
		var auditLog = new AuditLogDTO
		{
			UserId = string.IsNullOrEmpty(userId) ? "SYSTEM" : userId,
			EntityName = nameof(TrayState),
			Action = action,
			Timestamp = DateTime.UtcNow,
			KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
			OldValues = oldDto == null ? null : JsonSerializer.Serialize(oldDto),
			NewValues = newDto == null ? null : JsonSerializer.Serialize(newDto)
		};

		await _auditLogService.CreateAuditLogAsync(auditLog);
	}

	public async Task<IEnumerable<TrayStateDTO>> GetCurrentStatesForJob(Guid jobId)
	{
		var jobTrays = await _unitOfWork.JobTray
			.Query(j => j.JobId == jobId)
			.Select(t => t.TrayId)
			.ToListAsync();

		return await _unitOfWork.TrayState
			.Query(x => x.FinishedDateTime == null && jobTrays.Contains(x.TrayId))
			.MapTrayStateToDTO()
			.AsNoTracking()
			.OrderBy(p => p.TrayTag)
			.ToListAsync();
	}

}

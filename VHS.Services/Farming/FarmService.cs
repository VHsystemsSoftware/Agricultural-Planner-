using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using VHS.Data.Core.Mappings;
using VHS.Data.Core.Models;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.DTO;

namespace VHS.Services;

public interface IFarmService
{
	Task<IEnumerable<FarmDTO>> GetAllFarmsSimpleAsync();
	Task<IEnumerable<FarmDTO>> GetAllFarmsAsync();
	Task<FarmDTO?> GetFarmByIdAsync(Guid id);
	Task<IEnumerable<FarmTypeDTO>> GetAllFarmTypesAsync();
	Task<FarmDTO> CreateFarmAsync(FarmDTO farmDto);
	Task<FarmDTO> UpdateFarmAsync(FarmDTO farmDto);
	Task DeleteFarmAsync(Guid id);
	Task<List<LayerOccupancyDTO>> GetLayerOccupancyAsync(Guid farmId, DateTime asOf, Guid? rackId);
	Task<List<LayerOccupancyDTO>> GetRackOccupancyAsync(Guid farmId, Guid rackId);
}

public class FarmService : IFarmService
{
	private readonly IUnitOfWorkCore _unitOfWork;
	private readonly IRackService _rackService;
	private readonly ILayerService _layerService;
	private readonly ITrayStateRepository _trayStateRepository;
	private readonly IJobService _jobService;
	private readonly ITrayStateService _trayStateService;

	public FarmService(
		IUnitOfWorkCore unitOfWork,
		IRackService rackService,
		ILayerService layerService,
		ITrayStateRepository trayStateRepository,
		IJobService jobService,
		ITrayStateService trayStateService
		)
	{
		_unitOfWork = unitOfWork;
		_rackService = rackService;
		_layerService = layerService;
		_trayStateRepository = trayStateRepository;
		_jobService = jobService;
		_trayStateService = trayStateService;
	}

	private static FarmDTO MapFarmSimpleToDTO(Farm f) => new FarmDTO
	{
		Id = f.Id,
		Name = f.Name
	};

	private static FarmDTO MapFarmToDTO(Farm f) => new FarmDTO
	{
		Id = f.Id,
		Name = f.Name,
		Description = f.Description,
		FarmTypeId = f.FarmTypeId,
		FarmTypeName = f.FarmType?.Name ?? string.Empty,
		Floors = f.Floors?.Select(fl => new FloorDTO
		{
			Id = fl.Id,
			Name = fl.Name,
			Number = fl.Number,
			Enabled = fl.Enabled,
			Racks = fl.Racks?.Select(r => new RackDTO(r.Id, r.LayerCount)
			{
				Id = r.Id,
				Name = r.Name,
				Number = r.Number,
				TypeId = r.TypeId,
				TrayCountPerLayer = r.TrayCountPerLayer,
				Enabled = r.Enabled,
				TrayDepth = r.TrayCountPerLayer,
				Layers = r.Layers?.Select(l => new LayerDTO
				{
					Id = l.Id,
					Number = l.Number,
					RackId = l.RackId,
					Enabled = l.Enabled,
					TrayCountPerLayer = r.TrayCountPerLayer,
					Trays = r.TypeId == GlobalConstants.RACKTYPE_GROWING ? l.GrowTrayStates?.Select(t => new TrayStateDTO
					{
						Id = t.Id,
					}).ToList() ?? new List<TrayStateDTO>() : l.PreGrowTrayStates?.Select(t => new TrayStateDTO
					{
						Id = t.Id,
					}).ToList()
				}).ToList() ?? new List<LayerDTO>()
			}).ToList() ?? new List<RackDTO>()
		}).ToList() ?? new List<FloorDTO>()
	};

	public async Task<IEnumerable<FarmDTO>> GetAllFarmsAsync()
	{
		var farms = await _unitOfWork.Farm.GetAllAsync();
		return farms
			.OrderBy(f => f.Name)
			.Select(f => MapFarmToDTO(f));
	}

	public async Task<IEnumerable<FarmDTO>> GetAllFarmsSimpleAsync()
	{
		var farms = await _unitOfWork.Farm.GetAllAsync(includeProperties: ["Id,Name"]);
		return farms
			.OrderBy(f => f.Name)
			.Select(f => MapFarmSimpleToDTO(f));
	}

	public async Task<FarmDTO?> GetFarmByIdAsync(Guid id)
	{
		var farm = await _unitOfWork.Farm.GetByIdAsync(id);
		if (farm == null)
			return null;

		return MapFarmToDTO(farm);
	}

	public async Task<IEnumerable<FarmTypeDTO>> GetAllFarmTypesAsync()
	{
		var farmTypes = await _unitOfWork.FarmType.GetAllAsync();
		return farmTypes.Select(ft => new FarmTypeDTO
		{
			Id = ft.Id,
			Name = ft.Name,
			Description = ft.Description
		});
	}

	public async Task<FarmDTO> CreateFarmAsync(FarmDTO farmDto)
	{
		if (farmDto == null)
			throw new ArgumentNullException(nameof(farmDto));

		var farmId = farmDto.Id == Guid.Empty ? Guid.NewGuid() : farmDto.Id;

		var floors = new List<Floor>() {
			new Floor { Id = Guid.NewGuid(), Name = "SK1", Number = 1, FarmId = farmId, Enabled=true },
			new Floor { Id = Guid.NewGuid(), Name = "SK2", Number = 2, FarmId = farmId, Enabled=true },
			new Floor { Id = Guid.NewGuid(), Name = "SK3", Number = 3, FarmId = farmId, Enabled=true }
		};

		var farmEntity = new Farm
		{
			Id = farmId,
			Name = farmDto.Name,
			Description = farmDto.Description,
			FarmTypeId = farmDto.FarmTypeId,
			Floors = floors
		};

		try
		{
			await _unitOfWork.Farm.AddAsync(farmEntity);
			await _unitOfWork.SaveChangesAsync();

			await _rackService.InsertRacksForFarmAsync(farmEntity.Id, floors);
			await _layerService.InsertLayersForRacksAsync(farmEntity.Id);
		}
		catch (Exception ex)
		{
			throw;
		}

		return MapFarmToDTO(farmEntity);
	}

	public async Task<FarmDTO> UpdateFarmAsync(FarmDTO farmDto)
	{
		var existingFarm = await _unitOfWork.Farm.GetByIdAsync(farmDto.Id);
		if (existingFarm == null)
		{
			throw new Exception("Farm not found");
		}

		existingFarm.Name = farmDto.Name;
		existingFarm.Description = farmDto.Description;
		existingFarm.FarmTypeId = farmDto.FarmTypeId;

		_unitOfWork.Farm.Update(existingFarm);
		await _unitOfWork.SaveChangesAsync();

		var resultDto = MapFarmToDTO(existingFarm);
		resultDto.FarmTypeName = existingFarm.FarmType?.Name ?? string.Empty;

		return resultDto;
	}

	public async Task DeleteFarmAsync(Guid id)
	{
		var farm = await _unitOfWork.Farm.GetByIdAsync(id);
		if (farm == null)
		{
			throw new Exception("Farm not found");
		}

		farm.DeletedDateTime = DateTime.UtcNow;
		_unitOfWork.Farm.Update(farm);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task<List<LayerOccupancyDTO>> GetRackOccupancyAsync(Guid farmId, Guid rackId)
	{
		return await GetLayerOccupancyAsync(farmId, DateTime.Now, rackId);
	}

	public async Task<List<LayerOccupancyDTO>> GetLayerOccupancyAsync(Guid farmId, DateTime asOf, Guid? rackId = null)
	{
		var layers = (await _unitOfWork.Layer
			.GetAllAsync(
				l => l.Rack.Floor.FarmId == farmId &&
					 (!rackId.HasValue || l.RackId == rackId) &&
					 l.DeletedDateTime == null &&
					 l.Enabled &&
					 l.Rack.Floor.Enabled,
				includeProperties: new[] { "Rack.Floor" }))
			.Select(l => new
			{
				l.Id,
				l.RackId,
				l.Rack.FloorId,
				LayerName = $"{l.Rack.Name}-L{l.Number}",
				RackName = l.Rack.Name,
				FloorName = l.Rack.Floor.Name,
				Capacity = l.Rack.TrayCountPerLayer,
				FloorNumber = l.Rack.Floor.Number,
				RackNumber = l.Rack.Number,
				LayerNumber = l.Number,
				RackTypeId = l.Rack.TypeId,
				IsTransportLayer = l.Number == l.Rack.LayerCount
			})
			.ToList();

		List<TrayState> trayStates = await _trayStateRepository.GetCurrentStates();
		//List<Tray> freeTrays = await _trayStateRepository.GetFreeTrays();

		//for (int i = 0; i < (asOf - DateTime.UtcNow).Days + 1; i++)
		//{
		//	DateTime date = DateTime.UtcNow.AddDays(i);

		//	List<JobDTO> seedingJobs = (await _jobService.GetAllSeedingJobsAsync()).ToList();
		//	List<JobDTO> toSeedOnDay = seedingJobs.Where(j => j.ScheduledDate.Date == date.Date).ToList();
		//	Tray trayToUse = null;
		//	if (freeTrays.Any())
		//	{
		//		trayToUse = freeTrays.First();
		//		freeTrays.Remove(trayToUse);
		//	}
		//	else
		//	{
		//		bool foundNewNumber = false;
		//		Random rnd = new Random();
		//		string randomNumber = "0";
		//		while (!foundNewNumber)
		//		{
		//			randomNumber = rnd.Next(100000, 999999).ToString();
		//			if (!trayStates.Any(x => x.Tray.Tag == randomNumber)) foundNewNumber = true;
		//		}

		//		trayToUse = new Tray()
		//		{
		//			Id = Guid.NewGuid(),
		//			Tag = randomNumber,
		//			FarmId = farmId,
		//			StatusId = GlobalConstants.TRAYSTATUS_INUSE
		//		};
		//	}

		//	//start with a clean state
		//	trayToUse.TrayStates = new List<TrayState>();

		//	List<Layer> allLayers = (await _unitOfWork.Layer.GetAllAsync()).ToList();
		//	List<Recipe> allRecipes = (await _unitOfWork.Recipe.GetAllAsync()).ToList();

		//	if (toSeedOnDay.Any())
		//	{
		//		foreach (JobDTO job in toSeedOnDay.OrderBy(x => x.OrderOnDay))
		//		{
		//			foreach (JobTrayDTO trayJobInfo in job.JobTrays.OrderBy(x => x.OrderInJob))
		//			{
		//				TrayState currentState = new TrayState()
		//				{
		//					TrayId = trayToUse.Id,
		//					BatchId = job.BatchId,
		//					FinishedDateTime = null,
		//					AddedDateTime = DateTime.UtcNow,
		//				};
		//				trayToUse.TrayStates.Add(currentState);
		//				trayJobInfo.TrayId = trayToUse.Id;

		//				Layer pregrowLayer = allLayers.Single(x => x.Id == trayJobInfo.DestinationLayerId);
		//				int rackTrayCount = trayJobInfo.DestinationLayer.TrayCountPerLayer;

		//				if (!trayJobInfo.RecipeId.HasValue)
		//				{
		//					currentState = await _trayStateService.ArrivedAtSeederEmpty(currentState, job.JobTypeId,
		//						trayJobInfo.DestinationLayerId,
		//						trayJobInfo.TransportLayerId,
		//						trayJobInfo.DestinationLayer?.RackTypeId,
		//						trayJobInfo.DestinationLayer?.TrayCountPerLayer);
		//					currentState.PreGrowLayer = pregrowLayer;
		//					currentState.Tray = trayToUse;
		//				}
		//				else
		//				{
		//					var growingJobTray = await _unitOfWork.JobTray.Query(x =>
		//						x.ParentJobTrayId == trayJobInfo.Id
		//						&& x.DestinationLayer.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING).SingleOrDefaultAsync();

		//					var recipe = allRecipes.Single(x => x.Id == trayJobInfo.RecipeId.Value);

		//					currentState = await _trayStateService.ArrivedForSeeding(currentState, job.JobTypeId,
		//						trayJobInfo.DestinationLayerId,
		//						trayJobInfo.TransportLayerId,
		//						trayJobInfo.DestinationLayer?.RackTypeId,
		//						trayJobInfo.DestinationLayer?.TrayCountPerLayer,
		//						recipe,
		//						growingJobTray.DestinationLayerId,
		//						growingJobTray.TransportLayerId
		//						);
		//					currentState.Recipe = recipe;
		//					currentState.GrowLayer = growingJobTray.DestinationLayer;
		//					currentState.PreGrowLayer = pregrowLayer;
		//					currentState.Tray = trayToUse;
		//				}

		//				var transportTrayPreGrow = GetTransportOutputTrayPreGrow(trayStates,
		//							trayToUse.Id,
		//							trayJobInfo.DestinationLayer.RackId);
		//				if (transportTrayPreGrow != null)
		//				{
		//					MoveTraysOnLayerPreGrowTransport(trayStates, transportTrayPreGrow.PreGrowTransportLayerId.Value);
		//					MoveTraysOnLayerPreGrow(trayStates, transportTrayPreGrow.PreGrowLayerId.Value);
		//					transportTrayPreGrow.PreGrowOrderOnLayer = rackTrayCount;
		//					currentState.PreGrowOrderOnLayer = rackTrayCount;
		//				}
		//				else
		//				{
		//					var layer75 = await _rackService.GetBufferLayer(trayJobInfo.DestinationLayer.RackId);
		//					MoveTraysOnLayerPreGrow(trayStates,layer75.Id);
		//					MoveTraysOnLayerPreGrowTransport(trayStates, trayJobInfo.TransportLayerId.Value);
		//					currentState.PreGrowOrderOnLayer = rackTrayCount;
		//				}



		//				trayStates.Add(currentState);
		//			}
		//		}
		//	}

		//}

		var result = layers
			.Select(l =>
			{
				var traysonLayer = trayStates.Where(x => x.LayerId == l.Id).ToList();

				return new LayerOccupancyDTO
				{
					LayerId = l.Id,
					RackId = l.RackId,
					FloorId = l.FloorId,
					Trays = traysonLayer.Select(x => new TrayStateDTO
					{
						Id = x.Id,
						TrayTag = x.Tray.Tag,
						GrowOrderOnLayer = x.OrderOnLayer,
						PreGrowOrderOnLayer = x.OrderOnLayer,
						GrowFinishedDate = x.GrowFinishedDate,
						GrowLayerName = $"{x.GrowLayer?.Rack.Floor.Name}-{x.GrowLayer?.Rack.Name}-{x.GrowLayer?.Number}",
						PreGrowLayerName = $"{x.PreGrowLayer?.Rack.Floor.Name}-{x.PreGrowLayer?.Rack.Name}-{x.PreGrowLayer?.Number}",
						SeedDate = x.SeedDate,
						RecipeId = x.RecipeId,
						Recipe = x.RecipeId.HasValue ? new Produce.DTO.RecipeDTO()
						{
							Id = x.Recipe.Id,
							Name = x.Recipe?.Name ?? string.Empty,
						} : null,
					}).ToList(),
					LayerName = l.LayerName,
					RackName = l.RackName,
					FloorName = l.FloorName,
					Capacity = l.Capacity,
					FloorNumber = l.FloorNumber,
					RackNumber = l.RackNumber,
					LayerNumber = l.LayerNumber,
					RackTypeId = l.RackTypeId,
					IsTransportLayer = l.IsTransportLayer,

				};
			})
			.OrderBy(d => d.FloorNumber)
			.ThenBy(d => d.RackNumber)
			.ThenBy(d => d.LayerNumber)
			.ToList();

		return result;
	}
	public List<TrayState> MoveTraysOnLayerPreGrowTransport(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.PreGrowTransportLayerId == layerId
				&& x.PreGrowTransportLayerId != null
				&& x.PreGrowOrderOnLayer > 0)
			.ToList();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.PreGrowOrderOnLayer))
		{
			tray.PreGrowOrderOnLayer -= 1; //move one place up
		}
		return states;
	}

	public List<TrayState> MoveTraysOnLayerPreGrow(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.PreGrowLayerId == layerId
				&& x.PreGrowTransportLayerId == null
				&& x.PreGrowOrderOnLayer > 0)
			.ToList();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.PreGrowOrderOnLayer))
		{
			tray.PreGrowOrderOnLayer -= 1; //move one place up
		}
		return states;
	}

	public List<TrayState> MoveTraysOnLayerGrowTransport(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.GrowTransportLayerId == layerId
			&& x.GrowTransportLayerId != null
			&& x.GrowOrderOnLayer > 0)
			.ToList();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.GrowOrderOnLayer))
		{
			tray.GrowOrderOnLayer -= 1; //move one place up
		}
		return states;
	}

	public List<TrayState> MoveTraysOnLayerGrow(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.GrowLayerId == layerId
			&& x.GrowTransportLayerId == null
			&& x.GrowOrderOnLayer > 0)
			.ToList();

		foreach (var tray in traysOnLayer.OrderByDescending(x => x.GrowOrderOnLayer))
		{
			tray.GrowOrderOnLayer -= 1; //move one place up
		}
		return states;
	}

	public TrayState? GetTransportOutputTrayPreGrow(List<TrayState> states, Guid excludeTrayId, Guid rackId)
	{
		var tray = states
			.Where(x =>
				x.PreGrowLayer.RackId == rackId
				&& x.PreGrowTransportLayerId != null
				&& x.FinishedDateTime == null
				&& x.TrayId != excludeTrayId
				&& x.PreGrowOrderOnLayer == 1)
			.SingleOrDefault();

		return tray;
	}

	public TrayState? GetTransportOutputTrayGrow(List<TrayState> states, Guid excludeTrayId, Guid rackId)
	{
		var tray = states
			.Where(x =>
				x.GrowLayer.RackId == rackId
				&& x.GrowTransportLayerId != null
				&& x.FinishedDateTime == null
				&& x.TrayId != excludeTrayId
				&& x.GrowOrderOnLayer == 1)
			.SingleOrDefault();

		return tray;
	}
}

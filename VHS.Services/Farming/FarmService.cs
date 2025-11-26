using Microsoft.EntityFrameworkCore;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.DTO;
using VHS.Services.Produce.DTO;

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
	Task<List<LayerOccupancyDTO>> GetLayerOccupancyAsync(Guid farmId, DateOnly asOf, Guid? rackId, bool includeSimulations);
	Task<List<LayerOccupancyDTO>> GetRackOccupancyAsync(Guid farmId, Guid rackId, DateOnly asOf, bool includeSimulations);
	Task UpdateTrayDestinationsAsync(List<Guid> trayStateIds, Guid destinationLayerId, Guid rackTypeId);
}

public class FarmService : IFarmService
{
	private readonly IUnitOfWorkCore _unitOfWork;
	private readonly IRackService _rackService;
	private readonly ILayerService _layerService;
	private readonly ITrayStateRepository _trayStateRepository;
	private readonly IJobService _jobService;
	private readonly ITrayStateService _trayStateService;
	private readonly IJobRepository _jobRepository;

	public FarmService(
		IUnitOfWorkCore unitOfWork,
		IRackService rackService,
		ILayerService layerService,
		ITrayStateRepository trayStateRepository,
		IJobService jobService,
		ITrayStateService trayStateService,
		IJobRepository jobRepository
		)
	{
		_unitOfWork = unitOfWork;
		_rackService = rackService;
		_layerService = layerService;
		_trayStateRepository = trayStateRepository;
		_jobService = jobService;
		_trayStateService = trayStateService;
		_jobRepository = jobRepository;
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
				Floor = new FloorDTO()
				{
					Id = r.FloorId,
					Number = r.Floor.Number,
					Name = r.Floor.Name,
					Enabled = r.Floor.Enabled
				},
				TypeId = r.TypeId,
				TrayCountPerLayer = r.TrayCountPerLayer,
				Enabled = r.Enabled,
				TrayDepth = r.TrayCountPerLayer,
				Layers = r.Layers?.Select(l => new LayerDTO
				{
					Id = l.Id,
					Number = l.Number,
					RackId = l.RackId,
					RackTypeId = l.Rack.TypeId,
					IsTransportLayer = l.Number == l.Rack.LayerCount,
					Rack = new RackDTO()
					{
						Id = l.RackId,
						Number = l.Rack.Number,
						Name = l.Rack.Name,
						FloorId = l.Rack.FloorId,
						TypeId = l.Rack.TypeId,
						Enabled = l.Rack.Enabled,
						Floor = new FloorDTO()
						{
							Id = l.Rack.Floor.Id,
							Number = l.Rack.Floor.Number,
							Name = l.Rack.Floor.Name,
							Enabled = l.Rack.Floor.Enabled
						},
						Layers = r.Layers?.Select(l => new LayerDTO
						{
							Id = l.Id,
							Number = l.Number,
						}).ToList(),
						TrayCountPerLayer = l.Rack.TrayCountPerLayer
					},
					Batch = l.Batch != null ? new Batches.DTO.BatchDTO
					{
						Id = l.Batch.Id,
						Name = l.Batch.Name,
						SeedDate = l.Batch.SeedDate,
						HarvestDate = l.Batch.HarvestDate,
						FarmId = l.Batch.FarmId,
						Recipe = l.Batch.GrowPlan?.Recipe != null ? new RecipeDTO
						{
							Id = l.Batch.GrowPlan.Recipe.Id,
							Name = l.Batch.GrowPlan.Recipe.Name,
							Description = l.Batch.GrowPlan.Recipe.Description,
							GerminationDays = l.Batch.GrowPlan.Recipe.GerminationDays,
							PropagationDays = l.Batch.GrowPlan.Recipe.PropagationDays,
							GrowDays = l.Batch.GrowPlan.Recipe.GrowDays,
						} : null,
						BatchRows = l.Batch.BatchRows?.Select(br => new Batches.DTO.BatchRowDTO
						{
							Id = br.Id,
							BatchId = br.BatchId,
							FloorId = br.FloorId,
							RackId = br.RackId,
							LayerId = br.LayerId,
							EmptyCount = br.EmptyCount,
							AddedDateTime = br.AddedDateTime,
						}).ToList() ?? new List<Batches.DTO.BatchRowDTO>(),
					} : null,
					Enabled = l.Enabled,
					TrayCountPerLayer = r.TrayCountPerLayer,
					Trays = r.TypeId == GlobalConstants.RACKTYPE_GROWING ? l.GrowTrayStates?.Select(t => new TrayStateDTO
					{
						Id = t.Id,
					}).ToList() ?? new List<TrayStateDTO>() : l.PreGrowTrayStates?.Select(t => new TrayStateDTO
					{
						Id = t.Id,
					}).ToList()
				}).ToList() ?? new List<LayerDTO>(),
				BatchRows = r.BatchRows?.Select(br => new Batches.DTO.BatchRowDTO
				{
					Id = br.Id,
					BatchId = br.BatchId,
					FloorId = br.FloorId,
					RackId = br.RackId,
					LayerId = br.LayerId,
					EmptyCount = br.EmptyCount,
					AddedDateTime = br.AddedDateTime,
				}).ToList() ?? new List<BatchRowDTO>()
			}).ToList() ?? new List<RackDTO>()
		}).ToList() ?? new List<FloorDTO>()
	};

	public async Task<IEnumerable<FarmDTO>> GetAllFarmsAsync()
	{
		var farms = await _unitOfWork.Farm.GetAllAsync(includeProperties: ["Floors.Racks.Layers"]);
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

	public async Task<List<LayerOccupancyDTO>> GetRackOccupancyAsync(Guid farmId, Guid rackId, DateOnly asOf, bool includeSimulations = false)
	{
		return await GetLayerOccupancyAsync(farmId, asOf, rackId, includeSimulations);
	}

	public async Task<List<LayerOccupancyDTO>> GetLayerOccupancyAsync(Guid farmId, DateOnly asOf, Guid? rackId = null, bool includeSimulations = false)
	{
		var layers = (await _unitOfWork.Layer.GetAllAsync(
			l => l.Rack.Floor.FarmId == farmId
				&& (!rackId.HasValue || l.RackId == rackId),
			includeProperties: new[] { "Rack.Floor" })).ToList();

		var allLayers = (await _unitOfWork.Layer.GetAllAsync(
				l => l.Rack.Floor.FarmId == farmId,
				includeProperties: new[] { "Rack.Floor" })).ToList();

		var allRecipes = (await _unitOfWork.Recipe.GetAllAsync()).ToList();

		List<TrayState> simulatedTrayStates = await _trayStateRepository.GetCurrentStates();
		SimulationStats stats = new SimulationStats() { Harvested = 0 };

		if (includeSimulations)
		{
			var allFutureSeedingJobs = (await _jobService.GetAllSeedingJobsAsync())
				.Where(j => j.ScheduledDate <= asOf && j.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
				.ToList();
			var allTransplantJobs = (await _jobService.GetAllTransplantJobsAsync())
				.Where(j => j.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
				.ToList();

			var transPlantJobsTodo = new List<JobDTO>();

			var allJobs = allFutureSeedingJobs.Union(allTransplantJobs.Where(x=>!allFutureSeedingJobs.Select(y=>y.Id).Contains(x.Id))).ToList();

			if (allJobs.Any())
			{
				stats.SimulatedJobs = allJobs.Count();

				foreach (var ts in simulatedTrayStates)
				{
					ts.PreGrowLayer = allLayers.SingleOrDefault(l => l.Id == ts.PreGrowLayerId);
					ts.GrowLayer = allLayers.SingleOrDefault(l => l.Id == ts.GrowLayerId);
					ts.PreGrowTransportLayer = allLayers.SingleOrDefault(l => l.Id == ts.PreGrowTransportLayerId);
					ts.GrowTransportLayer = allLayers.SingleOrDefault(l => l.Id == ts.GrowTransportLayerId);
					ts.Recipe = allRecipes.SingleOrDefault(r => r.Id == ts.RecipeId);
				}

				for (var date = allJobs.Min(x => x.ScheduledDate); date <= asOf; date = date.AddDays(1))
				{
					var datedt = date.ToDateTime(new TimeOnly(0, 0));

					Guid[] customOrder = { GlobalConstants.JOBTYPE_EMPTY_TORACK, GlobalConstants.JOBTYPE_SEEDING_GERMINATION, GlobalConstants.JOBTYPE_SEEDING_PROPAGATION , GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT};
					var jobsForDay = allJobs
						.Where(j => j.ScheduledDate == date)
						.OrderBy(j => Array.IndexOf(customOrder, j.JobTypeId))
						.ThenBy(j => j.OrderOnDay)
						.ToList();

					if (jobsForDay.Any())
					{
						for (int i = 0; i < jobsForDay.Count; i++)
						{
							var job = jobsForDay[i];

							if (job.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT && !transPlantJobsTodo.Any(x => x.Id == job.Id))
							{
								continue;
							}

							var trayJobInfos = job.JobTrays
								.Where(jt => jt.DestinationLayerId.HasValue)
								.OrderBy(jt => jt.OrderInJob)
								.ToList();

							var batch = await _unitOfWork.Batch.GetByIdWithIncludesAsync(job.BatchId);

							foreach (var trayJobInfo in trayJobInfos)
							{
								if (trayJobInfo.Tray?.Tag != null && simulatedTrayStates.Any(ts => ts.Tray.Tag == trayJobInfo.Tray.Tag && ts.SeedDate == date))
								{
									continue;
								}
								stats.TraysInJob += 1;

								var destinationLayer = allLayers.SingleOrDefault(l => l.Id == trayJobInfo.DestinationLayerId);
								var trayToUse = simulatedTrayStates.FirstOrDefault(ts => ts.Tray.Tag == trayJobInfo.Tray?.Tag && ts.FinishedDateTime == null)?.Tray
									?? new Tray
									{
										Id = Guid.NewGuid(),
										Tag = trayJobInfo.Tray?.Tag ?? $"{job.Name}-{trayJobInfo.OrderInJob}",
										FarmId = farmId,
										StatusId = GlobalConstants.TRAYSTATUS_INUSE,
										TrayStates = new List<TrayState>()
									};

								// end any active states for this tray before starting a new one
								var existingActiveState = simulatedTrayStates.FirstOrDefault(ts => ts.TrayId == trayToUse.Id && ts.FinishedDateTime == null);
								if (existingActiveState is not null)
									existingActiveState.FinishedDateTime = datedt;

								var currentState = new TrayState
								{
									Id = Guid.NewGuid(),
									TrayId = trayToUse.Id,
									Tray = trayToUse,
									BatchId = job.BatchId,
									AddedDateTime = datedt,
									IsEstimated = true
								};
								trayToUse.TrayStates.Add(currentState);

								if (!trayJobInfo.RecipeId.HasValue)
								{
									currentState = await _trayStateService.ArrivedAtSeederEmpty(
										datedt,
										currentState,
										job.JobTypeId,
										trayJobInfo.DestinationLayerId,
										trayJobInfo.TransportLayerId,
										destinationLayer.Rack.TypeId,
										destinationLayer.Rack.TrayCountPerLayer);

									currentState.PreGrowOrderOnLayer = null;
									currentState.GrowOrderOnLayer = null;
								}
								else
								{
									var recipe = allRecipes.SingleOrDefault(r => r.Id == trayJobInfo.RecipeId.Value);
									if (recipe == null)
									{
										continue;
									}

									var growingJobTray = await _unitOfWork.JobTray.Query(x =>
										x.ParentJobTrayId == trayJobInfo.Id
										&& (
											x.DestinationLayer.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING)
											).SingleOrDefaultAsync();

									currentState = await _trayStateService.ArrivedForSeeding(
										datedt,
										currentState,
										job.JobTypeId,
										job.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT ? null : trayJobInfo.DestinationLayerId,
										job.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT ? null : trayJobInfo.TransportLayerId,
										destinationLayer.Rack.TypeId,
										destinationLayer.Rack.TrayCountPerLayer,
										recipe,
										job.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT ? trayJobInfo.DestinationLayerId : growingJobTray?.DestinationLayerId,
										job.JobTypeId == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT ? trayJobInfo.TransportLayerId : growingJobTray?.TransportLayerId);

									currentState.PreGrowOrderOnLayer = null;
									currentState.GrowOrderOnLayer = null;
									currentState.Batch = batch;
								}

								currentState.PreGrowLayer = allLayers.SingleOrDefault(l => l.Id == currentState.PreGrowLayerId);
								currentState.GrowLayer = allLayers.SingleOrDefault(l => l.Id == currentState.GrowLayerId);
								currentState.PreGrowTransportLayer = allLayers.SingleOrDefault(l => l.Id == currentState.PreGrowTransportLayerId);
								currentState.GrowTransportLayer = allLayers.SingleOrDefault(l => l.Id == currentState.GrowTransportLayerId);
								currentState.Recipe = allRecipes.SingleOrDefault(r => r.Id == currentState.RecipeId);

								simulatedTrayStates.Add(currentState);

								var rackIdForMove = destinationLayer.RackId;
								var rackTypeId = destinationLayer.Rack.TypeId;
								var rackTrayCount = destinationLayer.Rack.TrayCountPerLayer;

								if (rackTypeId == GlobalConstants.RACKTYPE_GERMINATION || rackTypeId == GlobalConstants.RACKTYPE_PROPAGATION)
								{
									var transportTrays = simulatedTrayStates.Where(x => x.PreGrowTransportLayerId != null && x.PreGrowLayer?.RackId == rackIdForMove && x.FinishedDateTime == null && x.TrayId != trayToUse.Id).ToList();
									var currentCount = transportTrays.Count;

									var transportTray = GetTransportOutputTrayPreGrow(simulatedTrayStates, trayToUse.Id, rackIdForMove);

									if (currentCount == rackTrayCount && transportTray != null)
									{
										if (!transportTray.PreGrowLayerId.HasValue || !transportTray.PreGrowTransportLayerId.HasValue)
										{
											continue;
										}
										var storageLayerId = transportTray.PreGrowLayerId.Value;
										var willBePushedOutTray = GetOutputTrayPreGrow(simulatedTrayStates, trayToUse.Id, storageLayerId);

										if (willBePushedOutTray != null)
										{
											willBePushedOutTray.PreGrowLayerId = null;
											willBePushedOutTray.PreGrowOrderOnLayer = null;
											willBePushedOutTray.WillBePushedOutPreGrow = datedt;
											willBePushedOutTray.PreGrowPushedOutByTrayId = trayToUse.Id;

											if (rackTypeId == GlobalConstants.RACKTYPE_PROPAGATION)
											{
												var transPlantJob = allTransplantJobs.Where(x => x.BatchId == willBePushedOutTray.BatchId).SingleOrDefault();
												if (!transPlantJobsTodo.Any((x=>x.Id==transPlantJob.Id)))
													transPlantJobsTodo.Add(transPlantJob);

												if (transPlantJob != null && !jobsForDay.Select(x => x.Id).Contains(transPlantJob.Id))
												{
													jobsForDay.Add(transPlantJob);
												}
											}
										}

										MoveTraysOnLayerPreGrow(simulatedTrayStates, storageLayerId);

										var transportLayerId = transportTray.PreGrowTransportLayerId.Value;
										var transportLayer = allLayers.SingleOrDefault(x => x.Id == transportLayerId);

										transportLayer.Batch = batch;
										transportTray.PreGrowTransportLayerId = null;

										MoveTraysOnLayerPreGrowTransport(simulatedTrayStates, transportLayerId);

										transportTray.PreGrowOrderOnLayer = rackTrayCount;
										currentState.PreGrowOrderOnLayer = rackTrayCount;
									}
									else
									{
										var bufferLayer = await _rackService.GetBufferLayer(rackIdForMove);
										bufferLayer.Batch = batch;

										if (bufferLayer == null)
										{
											continue;
										}
										currentState.PreGrowOrderOnLayer = transportTrays.Count + 1;
									}
								}
								else if (rackTypeId == GlobalConstants.RACKTYPE_GROWING)
								{
									currentState.ArrivedGrow = datedt;

									var rackTrayCountGrow = currentState.GrowLayer.Rack.TrayCountPerLayer;
									var test = simulatedTrayStates.Where(x => x.GrowLayer?.RackId == rackIdForMove).ToList();

									var transportTray = GetTransportOutputTrayGrow(simulatedTrayStates, trayToUse.Id, rackIdForMove);
									if (transportTray != null)
									{
										var willBePushedOutTrayGrow = GetOutputTrayGrow(simulatedTrayStates, trayToUse.Id, transportTray.GrowLayerId.Value);

										MoveTraysOnLayerGrow(simulatedTrayStates, transportTray.GrowLayerId.Value);
										MoveTraysOnLayerGrowTransport(simulatedTrayStates, transportTray.GrowTransportLayerId.Value);

										//var growLayer = allLayers.SingleOrDefault(x => x.Id == transportTray.GrowLayerId.Value);
										//growLayer.Batch = batch;

										var transportLayer = allLayers.SingleOrDefault(x => x.Id == transportTray.GrowTransportLayerId.Value);
										transportTray.Batch = batch;
										transportLayer.Batch = batch;


										transportTray.GrowOrderOnLayer = rackTrayCountGrow;
										currentState.GrowOrderOnLayer = rackTrayCount;

										transportTray.GrowTransportLayerId = null;
										transportTray.GrowTransportLayer = null;

										currentState.GrowOrderOnLayer = rackTrayCount;

										if (willBePushedOutTrayGrow != null)
										{
											willBePushedOutTrayGrow.GrowPushedOutByTrayId = currentState.TrayId;
											willBePushedOutTrayGrow.WillBePushedOutGrow = DateTime.UtcNow;
										}
									}
									else
									{
										//var transportTrays = simulatedTrayStates.Where(x => x.GrowTransportLayerId != null && x.GrowLayer?.RackId == rackIdForMove && x.FinishedDateTime == null && x.TrayId != trayToUse.Id).ToList();
										var bufferLayer = await _rackService.GetBufferLayer(rackIdForMove);
										MoveTraysOnLayerGrow(simulatedTrayStates, bufferLayer.Id);
										MoveTraysOnLayerGrowTransport(simulatedTrayStates, currentState.GrowTransportLayerId.Value);

										currentState.GrowOrderOnLayer = rackTrayCountGrow;
									}

								}

								if (trayJobInfo.OrderInJob == 1)
								{
									job.StatusId = GlobalConstants.JOBSTATUS_INPROGRESS;
								}
								if (trayJobInfo.OrderInJob == job.TrayCount)
								{
									job.StatusId = GlobalConstants.JOBSTATUS_COMPLETED;
								}
							}



							// move to grow simulation
							var traysReadyToMove = simulatedTrayStates
								.Where(ts => ts.PreGrowFinishedDate.HasValue
											 && ts.WillBePushedOutPreGrow.HasValue
											 && ts.GrowLayerId.HasValue
											 && ts.FinishedDateTime == null
											 && ts.ArrivedGrow == null)
								.OrderBy(x => x.WillBePushedOutPreGrow.Value)
								.ToList();

							foreach (var trayToMove in traysReadyToMove)
							{
								var growLayerMove = layers.SingleOrDefault(l => l.Id == trayToMove.GrowLayerId);

								if (growLayerMove == null)
									continue;

								var growRackId = growLayerMove.RackId;
								var rackTypeId = growLayerMove.Rack.TypeId;
								var rackTrayCount = growLayerMove.Rack.TrayCountPerLayer;

								trayToMove.PreGrowLayerId = null;
								trayToMove.PreGrowLayer = null;
								trayToMove.PreGrowOrderOnLayer = null;
								trayToMove.PreGrowTransportLayerId = null;
								trayToMove.PreGrowTransportLayer = null;

								trayToMove.ArrivedGrow = datedt;

								if (rackTypeId == GlobalConstants.RACKTYPE_GROWING)
								{
									if (trayToMove.GrowLayerId.HasValue)
									{
										var rackTrayCountGrow = trayToMove.GrowLayer.Rack.TrayCountPerLayer;

										var transportTrayGrowSimulation = GetTransportOutputTrayGrow(simulatedTrayStates, trayToMove.TrayId, trayToMove.GrowLayer.RackId);
										if (transportTrayGrowSimulation != null)
										{
											var willBePushedOutTrayGrow = GetOutputTrayGrow(simulatedTrayStates, trayToMove.TrayId, transportTrayGrowSimulation.GrowLayerId.Value);

											MoveTraysOnLayerGrow(simulatedTrayStates, transportTrayGrowSimulation.GrowLayerId.Value);
											MoveTraysOnLayerGrowTransport(simulatedTrayStates, transportTrayGrowSimulation.GrowTransportLayerId.Value);

											var transportLayer = layers.SingleOrDefault(x => x.Id == transportTrayGrowSimulation.GrowTransportLayerId.Value);
											transportLayer.Batch = trayToMove.Batch;
											transportTrayGrowSimulation.Batch = trayToMove.Batch;
											trayToMove.Batch = null;

											transportTrayGrowSimulation.GrowOrderOnLayer = rackTrayCountGrow;
											transportTrayGrowSimulation.GrowTransportLayerId = null;
											transportTrayGrowSimulation.GrowTransportLayer = null;

											trayToMove.GrowOrderOnLayer = rackTrayCount;

											if (willBePushedOutTrayGrow != null)
											{
												willBePushedOutTrayGrow.GrowPushedOutByTrayId = trayToMove.TrayId;
												willBePushedOutTrayGrow.WillBePushedOutGrow = DateTime.UtcNow;
											}
										}
										else
										{
											var bufferLayer = await _rackService.GetBufferLayer(trayToMove.GrowLayer.RackId);
											MoveTraysOnLayerGrow(simulatedTrayStates, bufferLayer.Id);
											MoveTraysOnLayerGrowTransport(simulatedTrayStates, trayToMove.GrowTransportLayerId.Value);

											trayToMove.GrowOrderOnLayer = rackTrayCount;
										}
									}
									else
									{
										var bufferLayer = await _rackService.GetBufferLayer(trayToMove.PreGrowLayer.RackId);
										MoveTraysOnLayerPreGrow(simulatedTrayStates, bufferLayer.Id);
										MoveTraysOnLayerPreGrowTransport(simulatedTrayStates, trayToMove.PreGrowTransportLayerId.Value);

										trayToMove.PreGrowOrderOnLayer = rackTrayCount;
									}
								}
							}

							var traysReadyToWasher = simulatedTrayStates
							.Where(ts => (ts.WillBePushedOutGrow.HasValue
											|| (ts.WillBePushedOutPreGrow.HasValue && !ts.GrowLayerId.HasValue))
										 && !ts.RecipeId.HasValue
										 && ts.FinishedDateTime == null)
							.ToList();
							foreach (var ts in traysReadyToWasher)
							{
								ts.ArrivedWashing = datedt;
								ts.FinishedDateTime = datedt;
							}
							stats.Washed += traysReadyToWasher.Count();

							var traysReadyToHarvest = simulatedTrayStates
							.Where(ts => ts.GrowFinishedDate.HasValue
										 && ts.WillBePushedOutGrow.HasValue
										 && ts.FinishedDateTime == null)
							.ToList();
							foreach (var ts in traysReadyToHarvest)
							{
								ts.FinishedDateTime = datedt;
							}
							stats.Harvested += traysReadyToHarvest.Count();
							stats.Washed += traysReadyToHarvest.Count();
						}

						allJobs = allJobs.Where(j => j.ScheduledDate > date).ToList();
					}
				}
			}
		}

		var result = layers
			.Select(l =>
			{
				var activeTrayStates = simulatedTrayStates.Where(ts => ts.FinishedDateTime == null);

				List<TrayState> traysonLayer;
				if (l.IsTransportLayer)
				{
					if (l.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING)
					{
						traysonLayer = activeTrayStates
						.Where(x => (x.GrowTransportLayerId == l.Id && x.ArrivedGrow != null))
						.ToList();
					}
					else if (l.Rack.TypeId == GlobalConstants.RACKTYPE_PROPAGATION)
					{
						traysonLayer = activeTrayStates
						.Where(x => (x.PreGrowTransportLayerId == l.Id))
						.ToList();

					}
					else if (l.Rack.TypeId == GlobalConstants.RACKTYPE_GERMINATION)
					{
						traysonLayer = activeTrayStates
						.Where(x => (x.PreGrowTransportLayerId == l.Id && x.ArrivedGrow == null))
						.ToList();
					}
					else
					{
						traysonLayer = new List<TrayState>();
					}
				}
				else
				{
					if (l.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING)
					{
						traysonLayer = activeTrayStates
							.Where(x => x.GrowLayerId == l.Id &&
										x.ArrivedGrow != null &&
										x.GrowTransportLayerId == null &&
										(x.GrowOrderOnLayer ?? 0) > 0)
							.ToList();
					}
					else if (l.Rack.TypeId == GlobalConstants.RACKTYPE_PROPAGATION)
					{
						traysonLayer = activeTrayStates
							.Where(x => x.PreGrowLayerId == l.Id &&
										x.PreGrowTransportLayerId == null &&
										(x.PreGrowOrderOnLayer ?? 0) > 0)
							.ToList();
					}
					else if (l.Rack.TypeId == GlobalConstants.RACKTYPE_GERMINATION)
					{
						traysonLayer = activeTrayStates
							.Where(x => x.PreGrowLayerId == l.Id &&
										x.ArrivedGrow == null &&
										x.PreGrowTransportLayerId == null &&
										(x.PreGrowOrderOnLayer ?? 0) > 0)
							.ToList();
					}
					else
					{
						traysonLayer = new List<TrayState>();
					}
				}

				Batch batchOnLayer = traysonLayer.Where(x => x.BatchId.HasValue).OrderBy(x => x.OrderOnLayer).FirstOrDefault()?.Batch;

				return new LayerOccupancyDTO
				{
					LayerId = l.Id,
					RackId = l.RackId,
					FloorId = l.Rack.FloorId,
					Enabled = l.Enabled && l.Rack.Enabled && l.Rack.Floor.Enabled,
					Trays = traysonLayer.Select(x => new TrayStateDTO
					{
						Id = x.Id,
						TrayTag = x.Tray?.Tag ?? "Unknown",
						GrowLayerId = x.GrowLayerId,
						PreGrowLayerId = x.PreGrowLayerId,
						GrowOrderOnLayer = x.GrowOrderOnLayer,
						PreGrowOrderOnLayer = x.PreGrowOrderOnLayer,
						GrowFinishedDate = x.GrowFinishedDate,
						PreGrowFinishedDate = x.PreGrowFinishedDate,
						PreGrowLayerNumber = x.PreGrowLayer?.Number,
						GrowLayerNumber = x.GrowLayer?.Number,
						GrowLayerName = x.GrowLayerId.HasValue ? $"{x.GrowLayer.Rack.Floor.Name}-{x.GrowLayer.Rack.Name}-{x.GrowLayer.Number}" : string.Empty,
						PreGrowLayerName = x.PreGrowLayerId.HasValue ? $"{x.PreGrowLayer.Rack.Floor.Name}-{x.PreGrowLayer.Rack.Name}-{x.PreGrowLayer.Number}" : string.Empty,
						SeedDate = x.SeedDate,
						RecipeId = x.RecipeId,
						Recipe = x.RecipeId.HasValue && x.Recipe != null ? new Produce.DTO.RecipeDTO
						{
							Id = x.Recipe.Id,
							Name = x.Recipe.Name ?? string.Empty
						} : null,
						IsEstimated = x.IsEstimated,
						BatchName = x.Batch?.Name ?? string.Empty,
					}).ToList(),
					LayerName = $"{l.Rack.Name}-L{l.Number}",
					RackName = l.Rack.Name,
					FloorName = l.Rack.Floor.Name,
					Capacity = l.Rack.TrayCountPerLayer,
					FloorNumber = l.Rack.Floor.Number,
					RackNumber = l.Rack.Number,
					LayerNumber = l.Number,
					RackTypeId = l.Rack.TypeId,
					IsTransportLayer = l.IsTransportLayer,
					SimulationStats = stats,
					Batch = batchOnLayer != null ? new Batches.DTO.BatchDTO()
					{
						Id = batchOnLayer.Id,
						Name = batchOnLayer.Name,
						SeedDate = batchOnLayer.SeedDate,
						HarvestDate = batchOnLayer.HarvestDate,
						BatchRows = batchOnLayer.BatchRows?.Where(x => x.LayerId == l.Id).Select(br => new Batches.DTO.BatchRowDTO
						{
							Id = br.Id,
							BatchId = br.BatchId,
							FloorId = br.FloorId,
							RackId = br.RackId,
							LayerId = br.LayerId,
							EmptyCount = br.EmptyCount,
							AddedDateTime = br.AddedDateTime,
							Number = br.Layer.Number,

						}).ToList() ?? new List<Batches.DTO.BatchRowDTO>(),
						GrowPlan = new Batches.DTO.GrowPlanDTO()
						{
							Id = batchOnLayer.GrowPlan.Id,
							Name = batchOnLayer.GrowPlan.Name,
							TraysPerDay = batchOnLayer.GrowPlan.TraysPerDay,
							DaysForPlan = batchOnLayer.GrowPlan.DaysForPlan,
							Recipe = batchOnLayer.GrowPlan.Recipe != null ? new Produce.DTO.RecipeDTO()
							{
								Id = batchOnLayer.GrowPlan.Recipe.Id,
								Name = batchOnLayer.GrowPlan.Recipe.Name,
								GerminationDays = batchOnLayer.GrowPlan.Recipe.GerminationDays,
								PropagationDays = batchOnLayer.GrowPlan.Recipe.PropagationDays,
								GrowDays = batchOnLayer.GrowPlan.Recipe.GrowDays
							} : null,
						}

					} : null,
				};
			})
			.OrderBy(d => d.FloorNumber)
			.ThenBy(d => d.RackNumber)
			.ThenBy(d => d.LayerNumber)
			.ToList();

		return result;
	}

	private TrayState? GetOutputTrayPreGrow(List<TrayState> states, Guid excludeTrayId, Guid layerId)
	{
		var tray = states
			.Where(x =>
				x.PreGrowLayerId == layerId
				&& x.FinishedDateTime == null
				&& x.PreGrowTransportLayerId == null
				&& x.TrayId != excludeTrayId
				&& x.PreGrowOrderOnLayer == 1)
			.SingleOrDefault();

		return tray;
	}

	private TrayState? GetOutputTrayGrow(List<TrayState> states, Guid excludeTrayId, Guid layerId)
	{
		var tray = states
			.Where(x =>
				x.GrowLayerId == layerId
				&& x.FinishedDateTime == null
				&& x.GrowTransportLayerId == null
				&& x.TrayId != excludeTrayId
				&& x.GrowOrderOnLayer == 1)
			.SingleOrDefault();

		return tray;
	}

	private void MoveTraysOnLayerPreGrowTransport(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.PreGrowTransportLayerId == layerId && x.FinishedDateTime == null && (x.PreGrowOrderOnLayer ?? 0) > 0)
			.OrderByDescending(x => x.PreGrowOrderOnLayer)
			.ToList();

		for (int i = 0; i < traysOnLayer.Count; i++)
		{
			traysOnLayer[i].PreGrowOrderOnLayer -= 1;
		}
	}

	private void MoveTraysOnLayerPreGrow(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.PreGrowLayerId == layerId && x.FinishedDateTime == null && x.PreGrowTransportLayerId == null && (x.PreGrowOrderOnLayer ?? 0) > 0)
			.OrderByDescending(x => x.PreGrowOrderOnLayer)
			.ToList();

		for (int i = 0; i < traysOnLayer.Count; i++)
		{
			traysOnLayer[i].PreGrowOrderOnLayer -= 1;
		}
	}

	private void MoveTraysOnLayerGrowTransport(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.GrowTransportLayerId == layerId && x.FinishedDateTime == null && (x.GrowOrderOnLayer ?? 0) > 0)
			.OrderByDescending(x => x.GrowOrderOnLayer)
			.ToList();

		for (int i = 0; i < traysOnLayer.Count; i++)
		{
			traysOnLayer[i].GrowOrderOnLayer -= 1;
		}
	}

	private void MoveTraysOnLayerGrow(List<TrayState> states, Guid layerId)
	{
		var traysOnLayer = states
			.Where(x => x.GrowLayerId == layerId && x.FinishedDateTime == null && x.GrowTransportLayerId == null && (x.GrowOrderOnLayer ?? 0) > 0)
			.OrderByDescending(x => x.GrowOrderOnLayer)
			.ToList();

		for (int i = 0; i < traysOnLayer.Count; i++)
		{
			traysOnLayer[i].GrowOrderOnLayer -= 1;
		}
	}

	private TrayState? GetTransportOutputTrayPreGrow(List<TrayState> states, Guid excludeTrayId, Guid rackId)
	{
		var transportTrays = states
			.Where(x => x.PreGrowLayer != null
				&& x.PreGrowLayer.RackId == rackId
				&& x.PreGrowTransportLayerId != null
				&& x.FinishedDateTime == null)
			.ToList();

		var matchingTrays = transportTrays
			.Where(x => x.TrayId != excludeTrayId
				&& x.PreGrowOrderOnLayer == 1)
			.ToList();

		if (matchingTrays.Count > 1)
		{
			return matchingTrays.OrderBy(t => t.AddedDateTime).FirstOrDefault();
		}

		return matchingTrays.SingleOrDefault();
	}

	private TrayState? GetTransportOutputTrayGrow(List<TrayState> states, Guid excludeTrayId, Guid rackId)
	{
		var transportTrays = states
			.Where(x => x.GrowLayer != null
				&& x.GrowLayer.RackId == rackId
				&& x.GrowTransportLayerId != null
				&& x.FinishedDateTime == null)
			.ToList();

		var matchingTrays = transportTrays
			.Where(x => x.TrayId != excludeTrayId
				&& x.GrowOrderOnLayer == 1)
			.ToList();

		if (matchingTrays.Count > 1)
		{
			return matchingTrays.OrderBy(t => t.AddedDateTime).FirstOrDefault();
		}

		return matchingTrays.SingleOrDefault();
	}

	public async Task UpdateTrayDestinationsAsync(List<Guid> trayStateIds, Guid destinationLayerId, Guid rackTypeId)
	{
		var trayStatesToUpdate = await _unitOfWork.TrayState
			.Query(ts => trayStateIds.Contains(ts.Id))
			.ToListAsync();

		foreach (var ts in trayStatesToUpdate)
		{
			if (rackTypeId == GlobalConstants.RACKTYPE_GERMINATION || rackTypeId == GlobalConstants.RACKTYPE_PROPAGATION)
			{
				ts.PreGrowLayerId = destinationLayerId;
			}
			else if (rackTypeId == GlobalConstants.RACKTYPE_GROWING)
			{
				ts.GrowLayerId = destinationLayerId;
			}

			_unitOfWork.TrayState.Update(ts);
		}
		await _unitOfWork.SaveChangesAsync();
	}



}


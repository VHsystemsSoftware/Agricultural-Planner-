using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VHS.OPC.Models.Message.Request;
using VHS.OPC.Models.Message.Response;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IOPCCommuncationService
{
	Task<GeneralHeartBeatResponse> ProcessGeneralHeartBeatRequestAsync(GeneralHeartBeatRequest request);
	Task<GrowLineInputTrayResponse> ProcessGrowLineInputTrayRequestAsync(GrowLineInputTrayRequest request, Guid trayId, Guid auditId);
	Task<HarvesterTrayResponse> ProcessHarvesterTrayRequest(HarvesterTrayRequest request, Guid trayId, Guid auditId, bool? allowed);
	Task<HarvestingTrayResponse> ProcessHarvestingTrayRequestAsync(HarvestingTrayRequest request, Guid trayId, Guid auditId, bool isPropagation);
	Task<PaternosterTrayResponse> ProcessPaternosterTrayRequestAsync(PaternosterTrayRequest request, Guid trayId, Guid auditId);
	Task<SeedingTrayResponse> ProcessSeedingTrayRequestAsync(Guid farmId, SeedingTrayRequest request, Job? currentJob, JobTray? jobTrayDTO, Guid auditId, bool isDoubleScan = false);
	Task<WashingTrayResponse> ProcessWashingTrayRequestAsync(WashingTrayRequest request, Guid trayId, Guid auditId);
	Task<WorkerHeartBeatResponse> ProcessWorkerHeartBeatRequestAsync(WorkerHeartBeatRequest request);
	Task<GenericResponse> ProcessFireAlarmEventAsync(GeneralFireAlarmStatusRequest request);
	Task<SeedingValidationResponse> ProcessSeedingTrayResponseAsync(SeedingValidationResponse request, Guid auditId);
	Task<GenericResponse> ProcessGeneralAlarmEventAsync(GeneralAlarmRequest request, MessageType eventType);
}

public class OPCCommuncationService : IOPCCommuncationService
{
	private readonly IUnitOfWorkCore _unitOfWork;
	private readonly ITrayStateService _trayStateService;
	private readonly ILogger<OPCCommuncationService> _logger;
	private readonly IRackService _rackService;
	private readonly IRecipeService _recipeService;


	public OPCCommuncationService(IUnitOfWorkCore unitOfWork, ITrayStateService trayStateService,
		ILogger<OPCCommuncationService> logger, IRackService rackService, IRecipeService recipeService)
	{
		_unitOfWork = unitOfWork;
		_trayStateService = trayStateService;
		_logger = logger;
		_rackService = rackService;
		_recipeService = recipeService;
	}


	public async Task<GrowLineInputTrayResponse> ProcessGrowLineInputTrayRequestAsync(GrowLineInputTrayRequest request, Guid trayId, Guid auditId)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}
		int rack = 0;
		int layer = 0;
		uint trayIdOutputTray = 0;

		var trayTag = await _unitOfWork.Tray.Query(x => x.Id == trayId).Select(x => x.Tag).SingleAsync();

		var trayCurrentState = await _unitOfWork.TrayState
			.Query(x =>
				x.TrayId == trayId
				&& x.FinishedDateTime == null, includeProperties: new[] { "GrowLayer.Rack", "PreGrowLayer.Rack" })
			.SingleOrDefaultAsync();

		if (trayCurrentState != null)
		{
			rack = trayCurrentState.RackNumber ?? 0;
			RecipeDTO? recipe = trayCurrentState.RecipeId.HasValue ? await _recipeService.GetRecipeByIdAsync(trayCurrentState.RecipeId.Value) : null;

			if (((recipe == null || trayCurrentState.Recipe.IsGerminationProduct) || trayCurrentState.Recipe.IsPropagationProduct) && trayCurrentState.GrowLayerId.HasValue)
			{

				if (trayCurrentState.GrowLayerId.HasValue)
				{
					var rackTrayCount = trayCurrentState.GrowLayer.Rack.TrayCountPerLayer;

					var transportTrayGrow = await _trayStateService.GetTransportOutputTrayGrow(trayId, trayCurrentState.GrowLayer.RackId);
					if (transportTrayGrow != null)
					{
						layer = transportTrayGrow.GrowLayer.Number;

						await _trayStateService.MoveTraysOnLayerGrowTransport(transportTrayGrow.LayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());
						await _trayStateService.MoveTraysOnLayerGrow(transportTrayGrow.GrowLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());

						transportTrayGrow.GrowOrderOnLayer = rackTrayCount;
						trayCurrentState.GrowOrderOnLayer = rackTrayCount;

						await _trayStateService.RemoveTransportLayerIdGrow(transportTrayGrow);

						var willBePushedOutTrayGrow = await _trayStateService.GetOutputTrayGrow(trayId, trayCurrentState.GrowLayer.RackId, transportTrayGrow.GrowLayerId.Value);
						if (willBePushedOutTrayGrow != null)
						{
							layer = willBePushedOutTrayGrow.GrowLayer.Number;
							trayIdOutputTray = Convert.ToUInt32(willBePushedOutTrayGrow.Tray.Tag);
							await _trayStateService.RegisterWillBePushedOutGrow(trayId, willBePushedOutTrayGrow);
						}
					}
					else
					{
						if (trayCurrentState.GrowLayer.RackId != null)
						{
							var bufferLayer = await _rackService.GetBufferLayer(trayCurrentState.GrowLayer.RackId);
							layer = bufferLayer.Number;
							await _trayStateService.MoveTraysOnLayerGrow(bufferLayer.Id, trayCurrentState.BatchId.GetValueOrDefault());
							await _trayStateService.MoveTraysOnLayerGrowTransport(trayCurrentState.GrowTransportLayerId.GetValueOrDefault(), trayCurrentState.BatchId.GetValueOrDefault());

							trayCurrentState.GrowOrderOnLayer = rackTrayCount;
						}
						else
						{
							_logger.LogInformation($"GrowLineInputTrayRequest - Cannot find rack for tray: {trayTag}");
							layer = 0;
						}
					}
				}
				else
				{
					if (trayCurrentState.GrowLayer.RackId != null && trayCurrentState.GrowTransportLayerId.HasValue)
					{
						var bufferLayer = await _rackService.GetBufferLayer(trayCurrentState.GrowLayer.RackId);
						layer = bufferLayer.Number;
						await _trayStateService.MoveTraysOnLayerGrow(bufferLayer.Id, trayCurrentState.BatchId.GetValueOrDefault());
						await _trayStateService.MoveTraysOnLayerGrowTransport(trayCurrentState.GrowTransportLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());

						var rackTrayCount = trayCurrentState.GrowLayer.Rack.TrayCountPerLayer;
						trayCurrentState.GrowOrderOnLayer = rackTrayCount;
					}
					else
					{
						_logger.LogInformation($"GrowLineInputTrayRequest - Cannot find rack for tray: {trayTag}");
						layer = 0;
					}
				}
			}
			else
			{
				if (trayCurrentState.PreGrowLayerId.HasValue)
				{
					var rackTrayCount = trayCurrentState.PreGrowLayer.Rack.TrayCountPerLayer;

					var transportTrayPreGrow = await _trayStateService.GetTransportOutputTrayPreGrow(trayId, trayCurrentState.PreGrowLayer.RackId);

					if (transportTrayPreGrow != null)
					{
						layer = transportTrayPreGrow.PreGrowLayer.Number;
						await _trayStateService.MoveTraysOnLayerPreGrowTransport(transportTrayPreGrow.PreGrowTransportLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());
						await _trayStateService.MoveTraysOnLayerPreGrow(transportTrayPreGrow.PreGrowLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());

						transportTrayPreGrow.PreGrowOrderOnLayer = rackTrayCount;
						trayCurrentState.PreGrowOrderOnLayer = rackTrayCount;
						await _trayStateService.RemoveTransportLayerIdPreGrow(transportTrayPreGrow);

						var willBePushedOutTrayPreGrow = await _trayStateService.GetOutputTrayPreGrow(trayId,
							trayCurrentState.PreGrowLayer.RackId,
							transportTrayPreGrow.PreGrowLayerId.Value);
						if (willBePushedOutTrayPreGrow != null)
						{
							layer = willBePushedOutTrayPreGrow.PreGrowLayer.Number;
							trayIdOutputTray = Convert.ToUInt32(willBePushedOutTrayPreGrow.Tray.Tag);
							await _trayStateService.RegisterWillBePushedOutPreGrow(trayId, willBePushedOutTrayPreGrow);
						}
					}
					else
					{
						//propagation trays dont have a growlayerid
						if (trayCurrentState.GrowLayerId.HasValue)
						{
							var rackTrayCountGrow = trayCurrentState.GrowLayer.Rack.TrayCountPerLayer;

							var transportTrayGrow = await _trayStateService.GetTransportOutputTrayGrow(trayId, trayCurrentState.GrowLayer.RackId);

							if (transportTrayGrow != null)
							{
								layer = transportTrayGrow.GrowLayer.Number;

								await _trayStateService.MoveTraysOnLayerGrowTransport(transportTrayGrow.GrowTransportLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());
								await _trayStateService.MoveTraysOnLayerGrow(transportTrayGrow.GrowLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());

								transportTrayGrow.GrowOrderOnLayer = rackTrayCountGrow;
								trayCurrentState.GrowOrderOnLayer = rackTrayCount;
								await _trayStateService.RemoveTransportLayerIdGrow(transportTrayGrow);

								var willBePushedOutTrayGrow = await _trayStateService.GetOutputTrayGrow(trayId,
									trayCurrentState.GrowLayer.RackId,
									transportTrayGrow.GrowLayerId.Value);
								if (willBePushedOutTrayGrow != null)
								{
									layer = willBePushedOutTrayGrow.GrowLayer.Number;
									trayIdOutputTray = Convert.ToUInt32(willBePushedOutTrayGrow.Tray.Tag);
									await _trayStateService.RegisterWillBePushedOutGrow(trayId, willBePushedOutTrayGrow);
								}

							}
							else
							{
								if (trayCurrentState.GrowLayer.RackId != null)
								{
									var bufferLayer = await _rackService.GetBufferLayer(trayCurrentState.GrowLayer.RackId);
									layer = bufferLayer.Number;
									await _trayStateService.MoveTraysOnLayerGrow(bufferLayer.Id, trayCurrentState.BatchId.GetValueOrDefault());
									await _trayStateService.MoveTraysOnLayerGrowTransport(trayCurrentState.GrowTransportLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());

									trayCurrentState.GrowOrderOnLayer = rackTrayCount;
								}
								else
								{
									_logger.LogInformation($"GrowLineInputTrayRequest - Cannot find rack for tray: {trayTag}");
									layer = 0;
								}
							}
						}
						else
						{
							if (trayCurrentState.PreGrowLayer.RackId != null)
							{
								var bufferLayer = await _rackService.GetBufferLayer(trayCurrentState.PreGrowLayer.RackId);
								layer = bufferLayer.Number;
								await _trayStateService.MoveTraysOnLayerPreGrow(bufferLayer.Id, trayCurrentState.BatchId.GetValueOrDefault());
								await _trayStateService.MoveTraysOnLayerPreGrowTransport(trayCurrentState.PreGrowTransportLayerId.Value, trayCurrentState.BatchId.GetValueOrDefault());

								trayCurrentState.PreGrowOrderOnLayer = rackTrayCount;
							}
							else
							{
								layer = 0;
							}
						}
					}
				}
				else
				{
					//TODO: unknown tray, what to do?
					_logger.LogError($"GrowLineInputTrayRequest - Unknown tray: {trayTag}");
				}
			}
		}

		await _unitOfWork.SaveChangesAsync();

		return new GrowLineInputTrayResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			AuditId = auditId,
			Data = new GrowLineInputTrayResponseData()
			{
				Destination = Convert.ToInt16(rack),
				Layer = Convert.ToInt16(layer),
				TrayId = Convert.ToUInt32(trayTag),
				TrayIdOutputTray = trayIdOutputTray
			}
		};
	}

	public async Task<SeedingTrayResponse> ProcessSeedingTrayRequestAsync(Guid farmId,
		SeedingTrayRequest request,
		Job? currentJob,
		JobTray? seedingJobTrayDTO,
		Guid auditId,
		bool isDoubleScan = false)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		if (currentJob != null && seedingJobTrayDTO != null)
		{
			if (currentJob.TrayCount == seedingJobTrayDTO.OrderInJob)
			{
				await _unitOfWork.Job.SetJobCompletedAsync(currentJob.Id);
			}
			var destinationCurrentTray = 0;
			var destinationOutputTray = 0;
			var layer = 0;
			uint trayIdOutputTray = 0;

			switch (currentJob.JobTypeId)
			{
				case var value when (value == GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT):
					destinationCurrentTray = (int)GlobalConstants.DestinationEnum.DESTINATION_TRANSPLANTER;
					break;
				case var value when value == GlobalConstants.JOBTYPE_EMPTY_TOWASHER:
					destinationCurrentTray = (int)GlobalConstants.DestinationEnum.DESTINATION_WASHER;
					break;
				case var value when
				   value == GlobalConstants.JOBTYPE_SEEDING_GERMINATION
				|| value == GlobalConstants.JOBTYPE_SEEDING_PROPAGATION
				|| value == GlobalConstants.JOBTYPE_EMPTY_TORACK:
					{
						//pushed out tray
						switch (seedingJobTrayDTO.DestinationLayer.Rack.TypeId)
						{
							case var rackTypeId when rackTypeId == GlobalConstants.RACKTYPE_GROWING:
								destinationCurrentTray = (int)GlobalConstants.DestinationEnum.DESTINATION_PATERNOSTER;
								break;
							case var rackTypeId when rackTypeId == GlobalConstants.RACKTYPE_GERMINATION:
								destinationCurrentTray = seedingJobTrayDTO.DestinationLayer.Rack.Number switch
								{
									1 => (int)GlobalConstants.DestinationEnum.DESTINATION_GERMINATIONRACK1,
									2 => (int)GlobalConstants.DestinationEnum.DESTINATION_GERMINATIONRACK2,
									3 => (int)GlobalConstants.DestinationEnum.DESTINATION_GERMINATIONRACK3,
									_ => 0
								};

								var rackTrayCount = seedingJobTrayDTO.DestinationLayer.Rack.TrayCountPerLayer;

								//pushed out tray
								var transportTrayPreGrow = await _trayStateService.GetTransportOutputTrayPreGrow(
									seedingJobTrayDTO.TrayId.Value,
									seedingJobTrayDTO.DestinationLayer.RackId);

								if (transportTrayPreGrow != null)
								{
									layer = transportTrayPreGrow.PreGrowLayer.Number;
									await _trayStateService.MoveTraysOnLayerPreGrowTransport(transportTrayPreGrow.PreGrowTransportLayerId.Value, transportTrayPreGrow.BatchId.GetValueOrDefault());
									await _trayStateService.MoveTraysOnLayerPreGrow(transportTrayPreGrow.PreGrowLayerId.Value, transportTrayPreGrow.BatchId.GetValueOrDefault());

									transportTrayPreGrow.PreGrowOrderOnLayer = rackTrayCount;
									seedingJobTrayDTO.Tray.CurrentState.PreGrowOrderOnLayer = rackTrayCount;

									await _trayStateService.RemoveTransportLayerIdPreGrow(transportTrayPreGrow);

									var willBePushedOutTray = await _trayStateService.GetOutputTrayPreGrow(
										seedingJobTrayDTO.TrayId.Value,
										seedingJobTrayDTO.DestinationLayer.RackId,
										transportTrayPreGrow.PreGrowLayerId.Value);

									if (willBePushedOutTray != null)
									{
										destinationOutputTray = willBePushedOutTray.RecipeId.HasValue ? (int)GlobalConstants.DestinationEnum.DESTINATION_PATERNOSTER : (int)GlobalConstants.DestinationEnum.DESTINATION_WASHER; //if recipe is set, then it goes to the paternoster, otherwise to the washer
										trayIdOutputTray = Convert.ToUInt32(willBePushedOutTray.Tray.Tag);
										await _trayStateService.RegisterWillBePushedOutPreGrow(seedingJobTrayDTO.TrayId.Value, willBePushedOutTray);
									}
									else
									{
										//Default to washer if unknown tray
										destinationOutputTray = (int)GlobalConstants.DestinationEnum.DESTINATION_WASHER;
										trayIdOutputTray = 0;
									}
								}
								else
								{
									//Default to washer if unknown tray
									destinationOutputTray = (int)GlobalConstants.DestinationEnum.DESTINATION_WASHER;
									trayIdOutputTray = 0;
									var bufferLayer = await _rackService.GetBufferLayer(seedingJobTrayDTO.DestinationLayer.RackId);
									layer = bufferLayer.Number;
									await _trayStateService.MoveTraysOnLayerPreGrow(bufferLayer.Id, seedingJobTrayDTO.Tray.CurrentState.BatchId.GetValueOrDefault());
									await _trayStateService.MoveTraysOnLayerPreGrowTransport(seedingJobTrayDTO.TransportLayerId.Value, seedingJobTrayDTO.Tray.CurrentState.BatchId.GetValueOrDefault());

									seedingJobTrayDTO.Tray.CurrentState.PreGrowOrderOnLayer = rackTrayCount;
								}

								break;
							case var rackTypeId when rackTypeId == GlobalConstants.RACKTYPE_PROPAGATION:
								destinationCurrentTray = (int)GlobalConstants.DestinationEnum.DESTINATION_PATERNOSTER;
								break;
						}
						break;
					}
				default:

					break;
			}

			if (!isDoubleScan) await _unitOfWork.SaveChangesAsync();

			return new SeedingTrayResponse()
			{
				RequestId = request.Id,
				OPCserverId = request.OPCserverId,
				AuditId = auditId,
				Data = new SeedingTrayResponseData()
				{
					JobAvailable = true,
					DestinationCurrentTray = destinationCurrentTray,
					DestinationOutputTray = destinationOutputTray,
					TrayIdOutputTray = trayIdOutputTray,
					Layer = layer,
					TrayId = Convert.ToUInt32(request.Data.TrayId.Value),
				}
			};
		}
		else
		{
			return new SeedingTrayResponse()
			{
				RequestId = request.Id,
				OPCserverId = request.OPCserverId,
				AuditId = auditId,
				Data = new SeedingTrayResponseData()
				{
					JobAvailable = false,
					DestinationCurrentTray = 0,
					DestinationOutputTray = 0,
					TrayIdOutputTray = 0,
					Layer = 0,
					TrayId = Convert.ToUInt32(request.Data.TrayId.Value)
				}
			};
		}
	}

	public async Task<SeedingValidationResponse> ProcessSeedingTrayResponseAsync(SeedingValidationResponse request, Guid auditId)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		return new SeedingValidationResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			AuditId = auditId,
			Data = new SeedingValidationResponseData()
			{
				TrayId = request.Data.TrayId
			}
		};
	}

	public async Task<HarvesterTrayResponse> ProcessHarvesterTrayRequest(HarvesterTrayRequest request, Guid trayId, Guid auditId, bool? pushedAllowed)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}
		var trayCurrentState = await _unitOfWork.TrayState
			.Query(x => x.TrayId == trayId && x.FinishedDateTime == null)
			.SingleOrDefaultAsync();

		var allowed = pushedAllowed.HasValue ? pushedAllowed.Value : trayCurrentState.EmptyReason == null && trayCurrentState.RecipeId.HasValue;

		return new HarvesterTrayResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			AuditId = auditId,
			Data = new HarvesterTrayResponseData()
			{
				HarvestAllowed = allowed,
				TrayId = Convert.ToUInt32(request.Data.TrayId.Value),

			}
		};
	}

	public async Task<HarvestingTrayResponse> ProcessHarvestingTrayRequestAsync(HarvestingTrayRequest request, Guid trayId, Guid auditId, bool isPropagation)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		int destination = 0;

		//comes from pregro = propagation rack, not from growing and is not empty
		if (isPropagation)
		{
			//route to transplanter, comes from propagation
			destination = (int)GlobalConstants.DestinationEnum.DESTINATION_TRANSPLANTER;
		}
		else
		{
			//fully grown or empty go to harvester
			destination = (int)GlobalConstants.DestinationEnum.DESTINATION_HARVESTER;
		}

		await Task.CompletedTask;

		return new HarvestingTrayResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			AuditId = auditId,
			Data = new HarvestingTrayResponseData()
			{
				Destination = Convert.ToInt16(destination),
				TrayId = Convert.ToUInt32(request.Data.TrayId.Value),
			}
		};
	}

	public async Task<PaternosterTrayResponse> ProcessPaternosterTrayRequestAsync(PaternosterTrayRequest request, Guid trayId, Guid auditId)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		int floor;
		var trayCurrentState = await _unitOfWork.TrayState
			.Query(x => x.TrayId == trayId && x.FinishedDateTime == null, includeProperties: new[] { "GrowLayer.Rack.Floor", "PreGrowLayer.Rack.Floor" })
			.SingleOrDefaultAsync();

		if (trayCurrentState == null)
		{
			//TODO: In dit geval is er een tray die niet bekend is bij de Grow planner, wat hiermee te doen?
			//Welke verdieping moet de tray dan heen?
			floor = 1;
		}
		else if (trayCurrentState.GrowLayer == null && trayCurrentState.PreGrowLayer != null)
		{
			floor = trayCurrentState?.PreGrowLayer?.Rack.Floor.Number ?? 0;
		}
		else if (trayCurrentState.GrowLayer != null)
		{
			floor = trayCurrentState?.GrowLayer?.Rack.Floor.Number ?? 0;
		}
		else
		{
			//unknown state, no grow or pre-grow layer
			floor = 1;
		}

		await Task.CompletedTask;

		return new PaternosterTrayResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			AuditId = auditId,
			Data = new PaternosterTrayResponseData()
			{
				Destination = Convert.ToInt16(floor),
				TrayId = Convert.ToUInt32(request.Data.TrayId.Value),
			}
		};
	}


	public async Task<WashingTrayResponse> ProcessWashingTrayRequestAsync(WashingTrayRequest request, Guid trayId, Guid auditId)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		//TODO: check allowed for washing?

		return new WashingTrayResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			AuditId = auditId,
			Data = new WashingTrayResponseData()
			{
				TrayId = Convert.ToUInt32(request.Data.TrayId.Value),
				Accepted = true
			}
		};
	}

	public async Task<WorkerHeartBeatResponse> ProcessWorkerHeartBeatRequestAsync(WorkerHeartBeatRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		return new WorkerHeartBeatResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			Data = new WorkerHeartBeatResponseData()
			{
				Value = true
			}
		};
	}

	public async Task<GeneralHeartBeatResponse> ProcessGeneralHeartBeatRequestAsync(GeneralHeartBeatRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}

		await Task.CompletedTask;

		return new GeneralHeartBeatResponse()
		{
			RequestId = request.Id,
			OPCserverId = request.OPCserverId,
			Data = new GeneralHeartBeatResponseData()
			{
				Value = true
			}
		};
	}

	public async Task<GenericResponse> ProcessFireAlarmEventAsync(GeneralFireAlarmStatusRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request), "Request cannot be null.");
		}
		string status = "UNKNOWN";
		var alarmData = request.Data.FireAlarmIsActive;
		if (alarmData != null)
		{
			status = alarmData.Value ? "ACTIVATED" : "CLEARED";

			_logger.LogInformation($"CRITICAL EVENT: Fire alarm status is {status}. Node: {alarmData.Node} at {alarmData.OPCDateTime}");
		}
		return new GenericResponse
		{
			RequestId = request.Id,
			Data = new GenericResponseData
			{
				Value = $"Fire alarm {status} event processed."
			}
		};
	}

    public async Task<GenericResponse> ProcessGeneralAlarmEventAsync(GeneralAlarmRequest request, MessageType eventType)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
        }
        string status = "UNKNOWN";
        var alarmData = request.Data.Value;
        var alarmName = eventType.ToString().Replace("AlarmActive", "");

        if (alarmData != null)
        {
            _logger.LogInformation($"ALARM EVENT: {alarmName} status is triggered. Node: {alarmData.Node} at {alarmData.OPCDateTime}");
        }
        return new GenericResponse
        {
            RequestId = Guid.Parse(request.Id),
            Data = new GenericResponseData
            {
                Value = $"Alarm {alarmName} {status} event processed."
            }
        };
    }

}
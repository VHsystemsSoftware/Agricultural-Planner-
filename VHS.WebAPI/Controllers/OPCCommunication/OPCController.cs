using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text.Json;
using VHS.Client.Components;
using VHS.Data.Core.Infrastructure;
using VHS.Data.Core.Models;
using VHS.Data.Models.Audit;
using VHS.OPC.Models;
using VHS.OPC.Models.Message.Request;
using VHS.OPC.Models.Message.Response;
using VHS.Services.SystemMessages;
using VHS.WebAPI.Hubs;

namespace VHS.WebAPI.Controllers.OPCCommunication;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class OPCController : ControllerBase
{
	private readonly ILogger<OPCController> _logger;
	private readonly IOPCCommuncationService _OPCCommuncationService;
	private readonly IUnitOfWorkCore _unitOfWorkCore;
	private readonly IHubContext<VHSNotificationHub, IHubCommunicator> _hubContext;
	private readonly IOPCAuditService _OPCAuditService;
	private readonly ITrayStateService _trayStateService;
	private readonly ISystemMessageService _systemMessageService;
	private readonly IMemoryCache _cache;
	string cacheKey = "Alarms";

	public OPCController(IMemoryCache memoryCache, ILogger<OPCController> logger, IOPCCommuncationService oPCCommuncationService, IHubContext<VHSNotificationHub, IHubCommunicator> hubContext,
		IUnitOfWorkCore unitOfWorkCore, IOPCAuditService OPCAuditService, ITrayStateService trayStateService, ISystemMessageService systemMessageService)
	{
		_OPCCommuncationService = oPCCommuncationService;
		_unitOfWorkCore = unitOfWorkCore;
		_hubContext = hubContext;
		_OPCAuditService = OPCAuditService;
		_trayStateService = trayStateService;
		_logger = logger;
		_systemMessageService = systemMessageService;
		_cache = memoryCache;


	}

	[HttpGet("alarms")]
	public async Task<IActionResult> GetAlarms()
	{
		if (!_cache.TryGetValue(cacheKey, out List<OPCAlarm>? alarms))
		{
			alarms = new();
		}
		return Ok(alarms);
	}

	[HttpPost("message/{eventId}/{trayTagUrl}")]
	public async Task<IActionResult> MessageTrigger(int eventId, uint trayTagUrl, [FromBody] JsonElement post)
	{
		try
		{
			_logger.LogInformation("OPCController MessageTrigger called with eventId: {EventId}", eventId);

			var messageData = post.ToString();
			SentrySdk.AddBreadcrumb($"{messageData}");

			var farmId = await _unitOfWorkCore.Farm.GetFirstFarmId();

			OPCAudit? audit = null;
			if (trayTagUrl > 0)
			{
				audit = await _OPCAuditService.ReceiveOPCAuditAsync(farmId, eventId, trayTagUrl.ToString());
				audit = await _OPCAuditService.UpdateOPCAuditInputMessageAsync(audit.Id, messageData);
				await _hubContext.Clients.All.GeneralOPCNotification($"{((MessageType)eventId).ToString()}");

				var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

				if (string.IsNullOrEmpty(messageData))
				{
					return BadRequest("Request body cannot be null or empty.");
				}

				switch (eventId)
				{
					case (int)MessageType.SeedingTrayRequest:
						{
							var message = JsonConvert.DeserializeObject<SeedingTrayRequest>(messageData);

							//handle logic	
							var currentJob = await _unitOfWorkCore.Job.GetCurrentJobForDate(GlobalConstants.JOBLOCATION_SEEDER, today, true);

							if (currentJob == null || currentJob.Paused)
							{
								_logger.LogInformation("No current job found for seeding on {Date}", today);

								await _systemMessageService.CreateMessageAsync(
									GlobalConstants.SYSTEM_MESSAGE_SEVERITY_INFO,
									GlobalConstants.SYSTEM_MESSAGE_CATEGORY_OPC,
									$"Unknown tray '{message.Data.TrayId.Value}' arrived at Seeder with no active job.");

								var returnMessageNoJob = await _OPCCommuncationService.ProcessSeedingTrayRequestAsync(farmId, message, null, null, audit.Id);

								//Remove to avoid overloading the db
								await _OPCAuditService.DeleteOPCAudit(audit.Id);
								return Ok(returnMessageNoJob);
							}
							else
							{
								List<string> signalrMessages = new();
								uint trayTag = message.Data.TrayId.Value;
								Guid trayId;
								JobTray? trayJobInfo;
								bool isDoubleScan = false;

								if (await _trayStateService.CheckDoubleSeedTray(trayTag.ToString(), currentJob.BatchId.Value))
								{
									//the tray was already scanned just before, prevent double registrations
									trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag, currentJob.BatchId, false);
									trayJobInfo = _unitOfWorkCore.JobTray.GetByJobAndTrayIdAsync(currentJob.Id, trayId);

									await _unitOfWorkCore.SaveChangesAsync();

									isDoubleScan = true;
								}
								else
								{
									trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag, currentJob.BatchId, true);
									trayJobInfo = _unitOfWorkCore.JobTray.GetNextJobTrayAndUpdate(currentJob.Id, trayId);
									await _unitOfWorkCore.SaveChangesAsync();

									await _trayStateService.ArrivedAtSeeder(DateTime.UtcNow, audit.Id, trayJobInfo);
									await _unitOfWorkCore.SaveChangesAsync();
								}

								if (trayJobInfo != null)
								{
									if (trayJobInfo.OrderInJob == 1)
									{
										await _unitOfWorkCore.Job.SetJobInProgressAsync(currentJob.Id);
										signalrMessages.Add($"Job {currentJob.Name} started with tray {trayTag.ToString()}");
									}

									if (trayJobInfo.OrderInJob == currentJob.TrayCount)
									{

										await _unitOfWorkCore.Job.SetJobCompletedAsync(currentJob.Id);
										signalrMessages.Add($"Job {currentJob.Name} completed for tray {trayTag.ToString()}");
									}
									await _unitOfWorkCore.SaveChangesAsync();

									if (!trayJobInfo.RecipeId.HasValue)
										signalrMessages.Add($"Empty Tray {trayTag.ToString()}");
									else
										signalrMessages.Add($"Seeding Tray {trayTag.ToString()} with recipe {trayJobInfo.Recipe?.Name}");
								}

								//return data
								var returnMessageOk = await _OPCCommuncationService.ProcessSeedingTrayRequestAsync(farmId, message, currentJob, trayJobInfo, audit.Id, isDoubleScan);
								var auditReturnMessage = JsonConvert.SerializeObject(returnMessageOk);
								await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

								//signalr
								signalrMessages.Add($"Returned tray {trayTag.ToString()}, " +
									$"\tDCT: {(GlobalConstants.DestinationEnum)returnMessageOk.Data.DestinationCurrentTray}" +
									$"\tDOT: {(GlobalConstants.DestinationEnum)returnMessageOk.Data.DestinationOutputTray}");
								foreach (var signalrMessage in signalrMessages)
								{
									await _hubContext.Clients.All.GeneralOPCNotification(signalrMessage);
								}
								await _hubContext.Clients.All.UpdateTrayState(trayId);
								await _hubContext.Clients.All.RefreshDashboardSeeder();
								await _hubContext.Clients.All.RefreshHome();

								//done
								_logger.LogInformation("Successfully returned tray {TrayTag} with job {JobId}", trayTag, currentJob.Id);
								return Ok(returnMessageOk);
							}


						}
					case (int)MessageType.HarvesterTrayRequest:
						{
							var message = JsonConvert.DeserializeObject<HarvesterTrayRequest>(messageData);

							//handle logic
							uint trayTag = message.Data.TrayId.Value;
							var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag);
							var currentJob = await _unitOfWorkCore.Job.GetCurrentJobForTray(GlobalConstants.JOBLOCATION_HARVESTER, trayId);

							await _trayStateService.ArrivedHarvester(trayId, audit.Id);
							await _unitOfWorkCore.SaveChangesAsync();

							bool? allowedHarvesting = null;

							if (currentJob == null)
							{
								await _hubContext.Clients.All.GeneralOPCNotification($"No job for harvesting for tray {trayTag.ToString()}");
								allowedHarvesting = false;
							}
							else
							{
								var trayJobInfo = _unitOfWorkCore.JobTray.GetByJobAndTrayIdAsync(currentJob.Id, trayId);
								if (currentJob.StatusId == GlobalConstants.JOBSTATUS_NOTSTARTED)
								{
									await _unitOfWorkCore.Job.SetJobInProgressAsync(currentJob.Id);
								}
								else
								{
									var statesForJob = await _trayStateService.GetCurrentStates(currentJob.BatchId.Value);
									var harvestedCount = statesForJob.Count(x => x.ArrivedHarvest != null);
									if (harvestedCount == currentJob.TrayCount)
									{
										await _unitOfWorkCore.Job.SetJobCompletedAsync(currentJob.Id);
									}
								}

								await _unitOfWorkCore.SaveChangesAsync();

								//signalr
								await _hubContext.Clients.All.GeneralOPCNotification($"Harvester tray {trayTag.ToString()}");
								await _hubContext.Clients.All.UpdateTrayState(trayId);
								await _hubContext.Clients.All.RefreshDashboardHarvester();
								await _hubContext.Clients.All.RefreshHome();
							}

							//return data	
							var returnMessage = await _OPCCommuncationService.ProcessHarvesterTrayRequest(message, trayId, audit.Id, allowedHarvesting);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							return Ok(returnMessage);
						}
					case (int)MessageType.SeedingInstructionOk:
						{
							var message = JsonConvert.DeserializeObject<SeedingValidationResponse>(messageData);

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessSeedingTrayResponseAsync(message, audit.Id);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							return Ok(returnMessage);
						}
					case (int)MessageType.HarvestingInstructionOk:
					case (int)MessageType.HarvestingInstructionNotAllowed:
					case (int)MessageType.HarvestingTrayRequest:
						{
							var message = JsonConvert.DeserializeObject<HarvestingTrayRequest>(messageData);

							//handle logic
							uint trayTag = message.Data.TrayId.Value;
							var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag);
							await _trayStateService.ArrivedHarvesting(trayId, audit.Id);
							await _unitOfWorkCore.SaveChangesAsync();

							//handle transplated trays comming DOWN
							//check for propagated product, this goes to transplant directly instead of the harvester
							var currentState = await _trayStateService.GetCurrentState(trayId);
							var isPropagation = currentState.RecipeId.HasValue ? currentState.Recipe.IsPropagationProduct : false;
							if (isPropagation)
							{
								//find transplant job for this tray and set to inprogress
								await _trayStateService.PropagationToTransplant(trayId, audit.Id);
								var currentJob = await _unitOfWorkCore.Job.GetTransplantJobForBatch(currentState.BatchId.Value);
								if (currentJob!=null)
								{
									await _unitOfWorkCore.Job.SetJobInProgressAsync(currentJob.Id);
									await _unitOfWorkCore.SaveChangesAsync();
									await _hubContext.Clients.All.RefreshDashboardSeeder();
									await _hubContext.Clients.All.RefreshDashboardTransplanter();
								}
							}

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessHarvestingTrayRequestAsync(message, trayId, audit.Id, isPropagation);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							await _hubContext.Clients.All.GeneralOPCNotification($"Harvesting tray {trayTag.ToString()}, " + $"\tDST: {(GlobalConstants.DestinationEnum)returnMessage.Data.Destination}");

							return Ok(returnMessage);
						}
					case (int)MessageType.HarvesterTrayWeightRequest:
						{
							var message = JsonConvert.DeserializeObject<HarvesterTrayWeightRequest>(messageData);

							//handle logic
							uint trayTag = message.Data.TrayId.Value;
							var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag);
							await _trayStateService.RegisterWeight(trayId, audit.Id, message.Data.Weight.Value);
							await _unitOfWorkCore.SaveChangesAsync();

							await _hubContext.Clients.All.GeneralOPCNotification($"Harvested Tray {trayTag.ToString()} with {message.Data.Weight} kg");
							await _hubContext.Clients.All.UpdateTrayState(trayId);
							await _hubContext.Clients.All.RefreshHome();

							return Ok();
						}
					case (int)MessageType.PaternosterInstructionNotAllowed:
					case (int)MessageType.PaternosterInstructionOk:
					case (int)MessageType.PaternosterTrayRequest:
						{
							var message = JsonConvert.DeserializeObject<PaternosterTrayRequest>(messageData);

							//handle logic
							uint trayTag = message.Data.TrayId.Value;
							var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag);
							await _trayStateService.ArrivedPaternosterUp(trayId, audit.Id);
							await _unitOfWorkCore.SaveChangesAsync();

							var currentState = await _trayStateService.GetCurrentState(trayId);
							//handle transplated trays going UP
							var isPropagation = currentState.RecipeId.HasValue && currentState.ArrivedGrow == null && !currentState.PreGrowLayerId.HasValue
								? currentState.Recipe.IsPropagationProduct 
								: false;
							if (isPropagation)
							{
								//get transplantjob for this tray
								var currentJob = await _unitOfWorkCore.Job.GetTransplantJobForBatch(currentState.BatchId.Value);
								if (currentJob != null)
								{
									//find all trays for this job, all trays from the transplanter should go upstairs
									var allJobTrayStates = await _trayStateService.GetCurrentStatesForJob(currentJob.Id);
									var countArrivedAtPaternoster = allJobTrayStates.Count(x => x.ArrivedPaternosterUp != null);
									if (countArrivedAtPaternoster == 1)
									{
										await _unitOfWorkCore.Job.SetJobInProgressAsync(currentJob.Id);
									}
									if (countArrivedAtPaternoster == currentJob.TrayCount)
									{
										await _unitOfWorkCore.Job.SetJobCompletedAsync(currentJob.Id);
									}
									await _unitOfWorkCore.SaveChangesAsync();

									await _hubContext.Clients.All.RefreshDashboardTransplanter();
								}
							}

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessPaternosterTrayRequestAsync(message, trayId, audit.Id);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							await _hubContext.Clients.All.GeneralOPCNotification($"Paternoster tray {trayTag.ToString()}, " +
								$"\tDST: {returnMessage.Data.Destination}");
							await _hubContext.Clients.All.UpdateTrayState(trayId);
							await _hubContext.Clients.All.RefreshHome();

							return Ok(returnMessage);
						}
					case (int)MessageType.GrowLineInputTrayRequest:
						{
							var message = JsonConvert.DeserializeObject<GrowLineInputTrayRequest>(messageData);
							uint trayTag = message.Data.TrayId.Value;
							//handle logic
							var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag);
							await _trayStateService.ArrivedGrow(trayId, audit.Id);
							await _unitOfWorkCore.SaveChangesAsync();

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessGrowLineInputTrayRequestAsync(message, trayId, audit.Id);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							//signalr
							await _hubContext.Clients.All.GeneralOPCNotification($"Growline tray {trayTag.ToString()}, " +
								$"\tLayer: {returnMessage.Data.Layer}" +
								$"\tDST: {returnMessage.Data.Destination}");
							await _hubContext.Clients.All.UpdateTrayState(trayId);
							await _hubContext.Clients.All.RefreshHome();

							return Ok(returnMessage);
						}
					case (int)MessageType.WashingTrayRequest:
						{
							var message = JsonConvert.DeserializeObject<WashingTrayRequest>(messageData);

							//handle logic
							uint trayTag = message.Data.TrayId.Value;
							var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag);
							await _trayStateService.ArrivedWashingAndFinish(trayId, audit.Id);
							await _unitOfWorkCore.SaveChangesAsync();

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessWashingTrayRequestAsync(message, trayId, audit.Id);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							//signalr
							await _hubContext.Clients.All.UpdateTrayState(trayId);
							await _hubContext.Clients.All.RefreshHome();

							return Ok(returnMessage);
						}
				}
			}

			switch (eventId)
			{

				case (int)MessageType.GrowLineInputPlannerInControl:
				case (int)MessageType.HarvesterPlannerInControl:
				case (int)MessageType.HarvestingPlannerInControl:
				case (int)MessageType.PaternosterPlannerInControl:
				case (int)MessageType.SeedingPlannerInControl:
				case (int)MessageType.WashingPlannerInControl:
					{
						var message = JsonConvert.DeserializeObject<GenericPlannerInControlRequest>(messageData);
						var returnMessage = new GenericResponse
						{
							RequestId = message.Id,
							Data = new GenericResponseData
							{
								Value = "OK"
							}
						};
						var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
						return Ok(returnMessage);
					}
				case (int)MessageType.GeneralHeartBeatRequest:
					{
						var message = JsonConvert.DeserializeObject<GeneralHeartBeatRequest>(messageData);
						var returnMessage = await _OPCCommuncationService.ProcessGeneralHeartBeatRequestAsync(message);

						await _hubContext.Clients.All.HeartBeat(returnMessage.OPCserverId);

						return Ok(returnMessage);
					}
				case (int)MessageType.WorkerHeartBeatRequest:
					{
						var message = JsonConvert.DeserializeObject<WorkerHeartBeatRequest>(messageData);
						var returnMessage = await _OPCCommuncationService.ProcessWorkerHeartBeatRequestAsync(message);

						await _hubContext.Clients.All.HeartBeatWorker();

						return Ok(returnMessage);
					}
				case (int)MessageType.FarmStateHeartBeatRequest:
					{
						var message = JsonConvert.DeserializeObject<FarmStateHeartBeatRequest>(messageData);
						//var returnMessage = await _OPCCommuncationService.ProcessWorkerHeartBeatRequestAsync(message);
						//await _hubContext.Clients.All.HeartBeatWorker();
						//await _unitOfWorkCore.SaveChangesAsync();
						return Ok();
					}

				// FIRE ALARM
				case (int)MessageType.FireAlarmStatusResponse:
				case (int)MessageType.FireAlarmStatusRequest:
					{
						var message = JsonConvert.DeserializeObject<GeneralFireAlarmStatusRequest>(messageData);

						var returnMessage = await _OPCCommuncationService.ProcessFireAlarmEventAsync(message);

						string status = message.Data.FireAlarmIsActive.Value ? "ACTIVE" : "OFF";
						await _hubContext.Clients.All.GeneralOPCNotification($"CRITICAL: FIRE ALARM IS NOW {status}");
						await _hubContext.Clients.All.FireAlarmStateChanged(message.Data.FireAlarmIsActive.Value);

						var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
						return Ok(returnMessage);
					}
				// GENERAL ALARMS
				case (int)MessageType.AlarmActiveFloor:
				case (int)MessageType.AlarmActiveGerminationArea:
				case (int)MessageType.AlarmActiveHarvestingStation:
				case (int)MessageType.AlarmActivePaternoster:
				case (int)MessageType.AlarmActiveSeedingStation:
				case (int)MessageType.AlarmActiveTransplantStation:
				case (int)MessageType.AlarmActiveTransportFromPater:
				case (int)MessageType.AlarmActiveTransportToPater:
				case (int)MessageType.AlarmActiveWashingStation:
				case (int)MessageType.AlarmActiveGrowLineInput:
				case (int)MessageType.AlarmActiveGrowLineOutput:
				case (int)MessageType.AlarmActiveRackArea1:
				case (int)MessageType.AlarmActiveRackArea2:
				case (int)MessageType.EmergencyStop:
				case (int)MessageType.ControlledStop:
				case (int)MessageType.ForcedStop:
					{
						var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						// Use the new, specific request class for deserialization
						var message = System.Text.Json.JsonSerializer.Deserialize<GeneralAlarmRequest>(messageData, options);
						var returnMessage = await _OPCCommuncationService.ProcessGeneralAlarmEventAsync(message, (MessageType)eventId);
						var alarmName = ((MessageType)eventId).ToString().Replace("AlarmActive", "");
						//string status = message.Data.Value.Value ? "ACTIVE" : "OFF";                       

						_cache.Set(cacheKey, JsonConvert.DeserializeObject<List<OPCAlarm>>(message.Data.Value.Value));

						await _hubContext.Clients.All.GeneralOPCNotification($"ALARM: {alarmName} is triggered");
						await _hubContext.Clients.All.AlarmStatusChanged(alarmName);

						return Ok(returnMessage);
					}
			}

			return Ok(new { EventId = eventId });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "OPCController error");
			SentrySdk.CaptureException(ex);
			return BadRequest("An error occurred while processing the request. Please check the input data and try again.");
		}
	}
}


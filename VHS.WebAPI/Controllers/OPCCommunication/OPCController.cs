using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog.Context;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;
using VHS.Data.Core.Infrastructure;
using VHS.Data.Core.Mappings;
using VHS.Data.Core.Models;
using VHS.Data.Models.Audit;
using VHS.OPC.Models.Message.Request;
using VHS.OPC.Models.Message.Response;
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

	public OPCController(ILogger<OPCController> logger, IOPCCommuncationService oPCCommuncationService, IHubContext<VHSNotificationHub, IHubCommunicator> hubContext,
		IUnitOfWorkCore unitOfWorkCore, IOPCAuditService OPCAuditService, ITrayStateService trayStateService)
	{
		_OPCCommuncationService = oPCCommuncationService;
		_unitOfWorkCore = unitOfWorkCore;
		_hubContext = hubContext;
		_OPCAuditService = OPCAuditService;
		_trayStateService = trayStateService;
		_logger = logger;
	}

	[HttpPost("sendmessage/{auditId}")]
	public async Task<IActionResult> UpdateSendMessageToOPC(Guid auditId)
	{
		await _OPCAuditService.SendOPCAuditAsync(auditId);
		return Ok();
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
							uint trayTag = message.Data.TrayId.Value;

							var currentJob = await _unitOfWorkCore.Job.GetCurrentJobForDate(GlobalConstants.JOBLOCATION_SEEDER, today, true);

							if (currentJob == null || currentJob.Paused)
							{
								_logger.LogInformation("No current job found for seeding on {Date}", today);

								var returnMessageNoJob = await _OPCCommuncationService.ProcessSeedingTrayRequestAsync(farmId, message, null, null, audit.Id);

								//Remove to avoid overloading the db
								await _OPCAuditService.DeleteOPCAudit(audit.Id);

								return Ok(returnMessageNoJob);
							}
							else
							{
								List<string> signalrMessages = new();
								var trayId = await _unitOfWorkCore.Tray.FindAndCreateTrayAsync(farmId, trayTag, currentJob.BatchId, true);

								JobTray trayJobInfo = await _unitOfWorkCore.JobTray.GetNextJobTrayAndUpdate(currentJob.Id, trayId);
								var trayCurrentState = await _trayStateService.GetCurrentState(trayId);

								if (!trayJobInfo.RecipeId.HasValue)
								{

									trayCurrentState = await _trayStateService.ArrivedAtSeederEmpty(trayCurrentState, trayJobInfo.Job.JobTypeId,
										trayJobInfo.DestinationLayerId,
										trayJobInfo.TransportLayerId,
										trayJobInfo.DestinationLayer?.Rack?.TypeId,
										trayJobInfo.DestinationLayer?.Rack?.TrayCountPerLayer);
									await _trayStateService.AddTrayStateAudit(trayCurrentState.Id, audit.Id);
									signalrMessages.Add($"Empty Tray {trayTag.ToString()}");
								}
								else
								{
									var growingJobTray = await _unitOfWorkCore.JobTray.Query(x =>
										x.ParentJobTrayId == trayJobInfo.Id
										&& x.DestinationLayer.Rack.TypeId == GlobalConstants.RACKTYPE_GROWING).SingleOrDefaultAsync();

									trayCurrentState = await _trayStateService.ArrivedForSeeding(trayCurrentState, trayJobInfo.Job.JobTypeId,
										trayJobInfo.DestinationLayerId,
										trayJobInfo.TransportLayerId,
										trayJobInfo.DestinationLayer?.Rack?.TypeId,
										trayJobInfo.DestinationLayer?.Rack?.TrayCountPerLayer,
										trayJobInfo.Recipe,
										growingJobTray?.DestinationLayerId,
										growingJobTray?.TransportLayerId);
									await _trayStateService.AddTrayStateAudit(trayCurrentState.Id, audit.Id);
									signalrMessages.Add($"Seeding Tray {trayTag.ToString()} with recipe {trayJobInfo.Recipe?.Name}");
								}

								await _unitOfWorkCore.SaveChangesAsync();

								//return data
								var returnMessageOk = await _OPCCommuncationService.ProcessSeedingTrayRequestAsync(farmId, message, currentJob, trayJobInfo, audit.Id);
								var auditReturnMessage = JsonConvert.SerializeObject(returnMessageOk);
								await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

								if (currentJob.TrayCount == trayJobInfo.OrderInJob)
								{
									await _unitOfWorkCore.Job.SetJobCompletedAsync(currentJob.Id);
									signalrMessages.Add($"Job {currentJob.Name} completed for tray {trayTag.ToString()}");
									await _hubContext.Clients.All.RefreshDashboardSeeder();
								}
								await _unitOfWorkCore.SaveChangesAsync();

								//signalr
								signalrMessages.Add($"Returned tray {trayTag.ToString()}, " +
									$"\tDCT: {(ComponentType)returnMessageOk.Data.DestinationCurrentTray} ({returnMessageOk.Data.DestinationCurrentTray})" +
									$"\tDOT: {(ComponentType)returnMessageOk.Data.DestinationOutputTray} ({returnMessageOk.Data.DestinationOutputTray})");
								foreach (var signalrMessage in signalrMessages)
								{
									await _hubContext.Clients.All.GeneralOPCNotification(signalrMessage);
								}
								await _hubContext.Clients.All.NewTrayAtSeeder(currentJob.Id, trayId);
								await _hubContext.Clients.All.UpdateTrayState(trayId);


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

							if (currentJob == null)
							{
								await _hubContext.Clients.All.GeneralOPCNotification($"No job for harvesting for tray {trayTag.ToString()}");

								var returnMessage = await _OPCCommuncationService.ProcessHarvesterTrayRequest(message, trayId, audit.Id, false);
								returnMessage.AuditId = audit.Id;
								var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
								await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

								return Ok(returnMessage);
							}
							else
							{
								var trayJobInfo = await _unitOfWorkCore.JobTray.GetByJobAndTrayIdAsync(currentJob.Id, trayId);
								if (currentJob.TrayCount == trayJobInfo.OrderInJob)
								{
									await _unitOfWorkCore.Job.SetJobCompletedAsync(currentJob.Id);
								}

								await _unitOfWorkCore.SaveChangesAsync();

								//return data
								var returnMessage = await _OPCCommuncationService.ProcessHarvesterTrayRequest(message, trayId, audit.Id, null);
								returnMessage.AuditId = audit.Id;
								var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
								await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

								await _hubContext.Clients.All.GeneralOPCNotification($"Harvester tray {trayTag.ToString()}");
								await _hubContext.Clients.All.UpdateTrayState(trayId);

								return Ok(returnMessage);
							}
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

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessHarvestingTrayRequestAsync(message, trayId, audit.Id);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							await _hubContext.Clients.All.GeneralOPCNotification($"Harvesting tray {trayTag.ToString()}, " +
								$"\tDST: {(ComponentType)returnMessage.Data.Destination} ({returnMessage.Data.Destination})");
							await _hubContext.Clients.All.UpdateTrayState(trayId);

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

							//return data
							var returnMessage = await _OPCCommuncationService.ProcessPaternosterTrayRequestAsync(message, trayId, audit.Id);
							returnMessage.AuditId = audit.Id;
							var auditReturnMessage = JsonConvert.SerializeObject(returnMessage);
							await _OPCAuditService.UpdateOPCAuditOutputMessageAsync(audit.Id, auditReturnMessage);

							await _hubContext.Clients.All.GeneralOPCNotification($"Paternoster tray {trayTag.ToString()}, " +
								$"\tDST: {returnMessage.Data.Destination}");
							await _hubContext.Clients.All.UpdateTrayState(trayId);

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
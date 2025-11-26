using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.Algorithm;
using VHS.WebAPI.Hubs;

namespace VHS.WebAPI.Controllers.Batches;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporary allowed
public class GrowPlanAlgoritmeController : ControllerBase
{
	private readonly IGrowPlanAlgoritmeService _growPlanAlgoritmeService;
	private readonly IHubContext<VHSNotificationHub, IHubCommunicator> _hubContext;
	public GrowPlanAlgoritmeController(IGrowPlanAlgoritmeService GrowPlanAlgoritmeService, IHubContext<VHSNotificationHub, IHubCommunicator> hubContext)
	{
		_growPlanAlgoritmeService = GrowPlanAlgoritmeService;
		_hubContext = hubContext;
	}

	[HttpPost("{farmId}/{rackSize}/{layers}/{cleanGrowRack}")]
	public async Task<IActionResult> GetGrowPlanCalculationOutput(
		Guid farmId,
		bool cleanGrowRack,
		int rackSize,
		int layers,
		[FromBody] GrowPlanDTO growPlan)
	{
		List<GrowDemandResultPerDay> plans = await _growPlanAlgoritmeService.GrowDemands(farmId, growPlan, rackSize, layers, cleanGrowRack);

		return Ok(plans);
	}


}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Farming.DTO;
using VHS.WebAPI.Authorization;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/farm")]
[Authorize]
public class FarmController : ControllerBase
{
    private readonly IFarmService _farmService;
    //private readonly IFarmPlannerService _farmPlannerService;
    private readonly ILogger<FarmController> _logger;

    public FarmController(ILogger<FarmController> logger, IFarmService farmService)//, IFarmPlannerService farmPlannerService
		{
        _logger = logger;
        _farmService = farmService;
        //_farmPlannerService = farmPlannerService;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.FarmManagerAndAbove)]
    public async Task<IActionResult> GetAllFarms()
    {
        var farms = await _farmService.GetAllFarmsAsync();
        return Ok(farms);
    }

	[HttpGet("simple")]
	[Authorize(Policy = AuthorizationPolicies.FarmManagerAndAbove)]
	public async Task<IActionResult> GetAllFarmsSimple()
	{
		var farms = await _farmService.GetAllFarmsSimpleAsync();
		return Ok(farms);
	}

	[HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.FarmManagerAndAbove)]
    public async Task<IActionResult> GetFarmById(Guid id)
    {
        var farm = await _farmService.GetFarmByIdAsync(id);
        if (farm == null)
        {
            return NotFound();
        }
        return Ok(farm);
    }

    [HttpGet("types")]
    [Authorize(Policy = AuthorizationPolicies.CanManageFarms)]
    public async Task<IActionResult> GetAllFarmTypes()
    {
        var farmTypes = await _farmService.GetAllFarmTypesAsync();
        return Ok(farmTypes);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanManageFarms)]
    public async Task<IActionResult> CreateFarm([FromBody] FarmDTO farmDto)
    {
        var createdFarm = await _farmService.CreateFarmAsync(farmDto);
        return CreatedAtAction(nameof(GetFarmById), new { id = createdFarm.Id }, createdFarm);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageFarms)]
    public async Task<IActionResult> UpdateFarm(Guid id, [FromBody] FarmDTO farmDto)
    {
        if (id != farmDto.Id)
        {
            return BadRequest("ID mismatch");
        }
        var updatedFarm = await _farmService.UpdateFarmAsync(farmDto);
        return CreatedAtAction(nameof(GetFarmById), new { id = updatedFarm.Id }, updatedFarm);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageFarms)]
    public async Task<IActionResult> DeleteFarm(Guid id)
    {
        await _farmService.DeleteFarmAsync(id);
        return NoContent();
    }

    //[HttpPost("plan/{id}")]
    //public async Task<IActionResult> PlanFarm(Guid id, [FromBody] FarmPlanRequest request)
    //{
    //    var farm = await _farmService.GetFarmByIdAsync(id);
    //    if (farm == null) return NotFound();

    //    var plan = await _farmPlannerService.PlanFarmAsync(
    //        farm,
    //        request.BatchSizes,
    //        request.TotalDays,
    //        request.TotalTraysAvailable,
    //        request.StartDate);

    //    if (!plan.Success)
    //        return BadRequest(plan.ErrorMessage);

    //    return Ok(plan);
    //}

    [HttpGet("{farmId}/occupancy")]
    [Authorize(Policy = AuthorizationPolicies.CanAccessOverviewOperations)]
    public async Task<ActionResult<List<LayerOccupancyDTO>>> GetOccupancy(Guid farmId, [FromQuery] DateTime asOf, [FromQuery] bool includeSimulations = false)
    {
        var result = await _farmService.GetLayerOccupancyAsync(farmId, DateOnly.FromDateTime(asOf), null, includeSimulations);
        return Ok(result);
    }

    [HttpGet("{farmId}/rackoccupancy/{rackId}")]
    [Authorize(Policy = AuthorizationPolicies.CanAccessOverviewOperations)]
    public async Task<ActionResult<List<LayerOccupancyDTO>>> GetOccupancy(Guid farmId, Guid rackId, [FromQuery] DateTime? asOf, [FromQuery] bool includeSimulations = false)
    {
        var dateToUse = asOf ?? DateTime.UtcNow;
        var result = await _farmService.GetRackOccupancyAsync(farmId, rackId, DateOnly.FromDateTime(dateToUse), includeSimulations);
        return Ok(result);
    }

    [HttpPut("trays/destination")]
    [Authorize(Policy = AuthorizationPolicies.FarmManagerAndAbove)]
    public async Task<IActionResult> UpdateTrayDestination([FromBody] TrayDestinationDTO request)
    {
        if (request == null || !request.TrayStateIds.Any())
        {
            return BadRequest("Invalid request data.");
        }
        await _farmService.UpdateTrayDestinationsAsync(request.TrayStateIds, request.DestinationLayerId, request.RackTypeId);
        return Ok();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/farm")]
[AllowAnonymous] // Temp allow
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
    // [Authorize(Roles = "CompanyAdmin, Grower")]
    public async Task<IActionResult> GetAllFarms()
    {
        var farms = await _farmService.GetAllFarmsAsync();
        return Ok(farms);
    }

	[HttpGet("simple")]
	// [Authorize(Roles = "CompanyAdmin, Grower")]
	public async Task<IActionResult> GetAllFarmsSimple()
	{
		var farms = await _farmService.GetAllFarmsSimpleAsync();
		return Ok(farms);
	}

	[HttpGet("{id}")]
    // [Authorize(Roles = "CompanyAdmin, Grower")]
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
    // [Authorize(Roles = "CompanyAdmin, Grower")]
    public async Task<IActionResult> GetAllFarmTypes()
    {
        var farmTypes = await _farmService.GetAllFarmTypesAsync();
        return Ok(farmTypes);
    }

    [HttpPost]
    // [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> CreateFarm([FromBody] FarmDTO farmDto)
    {
        var createdFarm = await _farmService.CreateFarmAsync(farmDto);
        return CreatedAtAction(nameof(GetFarmById), new { id = createdFarm.Id }, createdFarm);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "CompanyAdmin")]
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
    // [Authorize(Roles = "CompanyAdmin")]
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

    // GET api/farm/{farmId}/occupancy?asOf=2025-06-01
    [HttpGet("{farmId}/occupancy")]
    public async Task<ActionResult<List<LayerOccupancyDTO>>> GetOccupancy(
        Guid farmId,
        [FromQuery] DateTime asOf)
    {
        var result = await _farmService.GetLayerOccupancyAsync(farmId, asOf, null);
        return Ok(result);
    }

	[HttpGet("{farmId}/rackoccupancy/{rackId}")]
	public async Task<ActionResult<List<LayerOccupancyDTO>>> GetOccupancy(Guid farmId, Guid rackId)
	{
		var result = await _farmService.GetRackOccupancyAsync(farmId, rackId);
		return Ok(result);
	}
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VHS.Services.Batches.DTO;
using VHS.WebAPI.Hubs;

namespace VHS.WebAPI.Controllers.Batches;

[ApiController]
[Route("api/[controller]")]
public class GrowPlanController : ControllerBase
{
    private readonly IGrowPlanService _growPlanService;
	private readonly IHubContext<VHSNotificationHub, IHubCommunicator> _hubContext;
	public GrowPlanController(IGrowPlanService GrowPlanService, IHubContext<VHSNotificationHub, IHubCommunicator> hubContext)
    {
        _growPlanService = GrowPlanService;
        _hubContext = hubContext;
	}

    [HttpGet]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> GetAllBatchPlans(
        [FromQuery] Guid? productId = null,
        [FromQuery] string? name = null,
        [FromQuery] Guid? recipeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] IEnumerable<Guid>? statusIds = null)
    {
        var plans = await _growPlanService.GetAllGrowPlansAsync(
            productId,
            name,
            recipeId,
            startDateFrom.HasValue ? DateOnly.FromDateTime(startDateFrom.Value) : null,
            startDateTo.HasValue ? DateOnly.FromDateTime(startDateTo.Value) : null,
            statusIds);
        return Ok(plans);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> GetGrowPlanById(Guid id)
    {
        var plan = await _growPlanService.GetGrowPlanByIdAsync(id);
        if (plan == null)
            return NotFound();
        return Ok(plan);
    }

    [HttpPost]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> CreateBatchPlan([FromBody] GrowPlanDTO planDto)
    {
        var createdConfig = await _growPlanService.CreateGrowPlanAsync(planDto);
        return CreatedAtAction(nameof(GetGrowPlanById), new { id = createdConfig.Id }, createdConfig);
    }

    [HttpPost("multiple")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> CreateGrowPlanMultipe([FromBody] GrowPlanDTO planDto)
    {
        await _growPlanService.CreateGrowPlanMultipleAsync(planDto);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> UpdateBatchPlan(Guid id, [FromBody] GrowPlanDTO planDto)
    {
        if (id != planDto.Id)
            return BadRequest("ID mismatch");
        await _growPlanService.UpdateGrowPlanAsync(planDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> DeleteBatchPlan(Guid id)
    {
        await _growPlanService.DeleteGrowPlanAsync(id);
		
       

		return NoContent();
    }

    [HttpGet("calculate-assignments")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<ActionResult<List<BatchRowDTO>>> CalculateAssignments(
        [FromQuery] Guid rackTypeId,
        [FromQuery] int traysPerDay,
        [FromQuery] int days,
        [FromQuery] bool includeTransportLayer = true)
    {
        var result = await _growPlanService.CalculateAssignmentsAsync(rackTypeId, traysPerDay, days, includeTransportLayer);
        return Ok(result);
    }

    [HttpPost("startplan")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> StartPlan([FromBody] GrowPlanDTO plan)
    {
        await _growPlanService.StartGrowPlanAsync(plan);
		
        await _hubContext.Clients.All.RefreshDashboardSeeder();

		return Ok();
    }

	[HttpPost("startplan/{growPlanId}")]
	[Authorize(Policy = "CanAccessPlanningOperations")]
	public async Task<IActionResult> StartPlan(Guid growPlanId)
	{
		await _growPlanService.StartGrowPlanAsync(growPlanId);
		
        await _hubContext.Clients.All.RefreshDashboardSeeder();

		return Ok();
	}

    [HttpPost("{id}/duplicate")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> DuplicateBatchPlan(Guid id)
    {
        var newPlan = await _growPlanService.DuplicateGrowPlanAsync(id);
        return Ok(newPlan);
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopBatchPlan(Guid id, [FromBody] DateOnly endDate)
    {
        try
        {
            await _growPlanService.StopGrowPlanAsync(id, endDate);
            await _hubContext.Clients.All.RefreshDashboardSeeder();
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBatchPlan(Guid id)
    {
        try
        {
            await _growPlanService.CancelGrowPlanAsync(id);
            await _hubContext.Clients.All.RefreshDashboardSeeder();
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

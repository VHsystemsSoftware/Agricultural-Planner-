using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VHS.Services.Batches.DTO;
using VHS.WebAPI.Hubs;

namespace VHS.WebAPI.Controllers.Batches;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporary allowed
public class BatchPlanController : ControllerBase
{
    private readonly IBatchPlanService _batchPlanService;
	private readonly IHubContext<VHSNotificationHub, IHubCommunicator> _hubContext;
	public BatchPlanController(IBatchPlanService BatchPlanService, IHubContext<VHSNotificationHub, IHubCommunicator> hubContext)
    {
        _batchPlanService = BatchPlanService;
        _hubContext = hubContext;
	}

    [HttpGet]
    public async Task<IActionResult> GetAllBatchPlans(
        [FromQuery] Guid? productId = null,
        [FromQuery] string? name = null,
        [FromQuery] Guid? recipeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] IEnumerable<Guid>? statusIds = null)
    {
        var plans = await _batchPlanService.GetAllBatchPlansAsync(
            productId,
            name,
            recipeId,
            startDateFrom,
            startDateTo,
            statusIds);
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBatchPlanById(Guid id)
    {
        var plan = await _batchPlanService.GetBatchPlanByIdAsync(id);
        if (plan == null)
            return NotFound();
        return Ok(plan);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBatchPlan([FromBody] BatchPlanDTO planDto)
    {
        var createdConfig = await _batchPlanService.CreateBatchPlanAsync(planDto);
        return CreatedAtAction(nameof(GetBatchPlanById), new { id = createdConfig.Id }, createdConfig);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBatchPlan(Guid id, [FromBody] BatchPlanDTO planDto)
    {
        if (id != planDto.Id)
            return BadRequest("ID mismatch");
        await _batchPlanService.UpdateBatchPlanAsync(planDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBatchPlan(Guid id)
    {
        await _batchPlanService.DeleteBatchPlanAsync(id);
		
       

		return NoContent();
    }

    [HttpGet("calculate-assignments")]
    public async Task<ActionResult<List<BatchRowDTO>>> CalculateAssignments(
        [FromQuery] Guid rackTypeId,
        [FromQuery] int traysPerDay,
        [FromQuery] int days,
        [FromQuery] bool includeTransportLayer = true)
    {
        var result = await _batchPlanService.CalculateAssignmentsAsync(rackTypeId, traysPerDay, days, includeTransportLayer);
        return Ok(result);
    }

    [HttpPost("startplan")]
    public async Task<IActionResult> StartPlan([FromBody] BatchPlanDTO plan)
    {
        await _batchPlanService.StartBatchPlanAsync(plan);
		
        await _hubContext.Clients.All.RefreshDashboardSeeder();

		return Ok();
    }

	[HttpPost("startplan/{batchPlanId}")]
	public async Task<IActionResult> StartPlan(Guid batchPlanId)
	{
		await _batchPlanService.StartBatchPlanAsync(batchPlanId);
		
        await _hubContext.Clients.All.RefreshDashboardSeeder();

		return Ok();
	}
}

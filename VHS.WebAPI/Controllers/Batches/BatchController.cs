using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Batches.DTO;
using VHS.WebAPI.Authorization;

namespace VHS.WebAPI.Controllers.Batches;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BatchController : ControllerBase
{
	private readonly IBatchService _batchService;

	public BatchController(IBatchService batchService)
	{
		_batchService = batchService;
	}

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanAccessPlanningOperations)]
    public async Task<IActionResult> GetAllBatches(
        [FromQuery] Guid? farmId,
        [FromQuery] string? batchName,
        [FromQuery] string? planName,
        [FromQuery] DateTime? seedDateFrom,
        [FromQuery] DateTime? seedDateTo,
        [FromQuery] DateTime? harvestDateFrom,
        [FromQuery] DateTime? harvestDateTo,
        CancellationToken cancellationToken)
    {
        var batches = await _batchService.GetAllBatchesAsync(
            farmId,
            batchName,
            planName,
            seedDateFrom,
            seedDateTo,
            harvestDateFrom,
            harvestDateTo);
        return Ok(batches);
    }

	[HttpGet("{id}")]
	[Authorize(Policy = AuthorizationPolicies.CanAccessPlanningOperations)]
	public async Task<IActionResult> GetBatchById(Guid id, CancellationToken cancellationToken)
	{
		var batch = await _batchService.GetBatchByIdAsync(id);
		if (batch == null)
			return NotFound();
		return Ok(batch);
	}

	[HttpPost]
	[Authorize(Policy = AuthorizationPolicies.CanAccessPlanningOperations)]
	public async Task<IActionResult> CreateBatch([FromBody] BatchDTO batchDto, CancellationToken cancellationToken)
	{
		var createdBatch = await _batchService.CreateBatchAsync(batchDto);
		return CreatedAtAction(nameof(GetBatchById), new { id = createdBatch.Id }, createdBatch);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = AuthorizationPolicies.CanAccessPlanningOperations)]
	public async Task<IActionResult> UpdateBatch(Guid id, [FromBody] BatchDTO batchDto, CancellationToken cancellationToken)
	{
		if (id != batchDto.Id)
			return BadRequest("ID mismatch");
		await _batchService.UpdateBatchAsync(batchDto);
		return NoContent();
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = AuthorizationPolicies.CanAccessPlanningOperations)]
	public async Task<IActionResult> DeleteBatch(Guid id)
	{
		await _batchService.DeleteBatchAsync(id);
		return NoContent();
	}

	[HttpPut("lofreference/{id}/{jobId}")]
	[Authorize(Policy = AuthorizationPolicies.CanAccessPlanningOperations)]
	public async Task<IActionResult> UpdateBatchLotReference(Guid id, Guid jobId, [FromBody] string lotReference, CancellationToken cancellationToken)
	{
		await _batchService.UpdateLotReference(id,jobId, lotReference);
		return NoContent();
	}
}

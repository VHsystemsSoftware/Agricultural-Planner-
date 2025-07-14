using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/rack")]
[AllowAnonymous] // Temp allow
public class RackController : ControllerBase
{
    private readonly IRackService _rackService;

    public RackController(IRackService rackService)
    {
        _rackService = rackService;
    }

    [HttpGet]
    // [Authorize(Roles = "CompanyAdmin, Grower")]
    public async Task<IActionResult> GetAllRacks(Guid? farmId = null)
    {
        var racks = await _rackService.GetAllRacksAsync(farmId);
        return Ok(racks);
    }

    [HttpGet("type/{farmId}/{typeId}")]
    // [Authorize(Roles = "CompanyAdmin, Grower")]
    public async Task<IActionResult> GetAllRacksByType(Guid farmId, Guid typeId)
    {
        var racks = await _rackService.GetAllRacksByTypeAsync(farmId, typeId);
        return Ok(racks);
    }

    [HttpGet("{id}")]
    // [Authorize(Roles = "CompanyAdmin, Grower")]
    public async Task<IActionResult> GetRackById(Guid id)
    {
        var rack = await _rackService.GetRackByIdAsync(id);
        if (rack == null)
            return NotFound();
        return Ok(rack);
    }

    [HttpPost]
    // [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> CreateRack([FromBody] RackDTO rackDto)
    {
        var createdRack = await _rackService.CreateRackAsync(rackDto);
        return CreatedAtAction(nameof(GetRackById), new { id = createdRack.Id }, createdRack);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> UpdateRack(Guid id, [FromBody] RackDTO rackDto)
    {
        if (id != rackDto.Id)
            return BadRequest("ID mismatch");
        await _rackService.UpdateRackAsync(rackDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> DeleteRack(Guid id)
    {
        await _rackService.DeleteRackAsync(id);
        return NoContent();
    }

    [HttpPut("enable/{id}")]
    public async Task<IActionResult> EnableRack(Guid id, [FromBody] EnabledDTO enabledDto)
    {
        if (id != enabledDto.Id)
            return BadRequest("ID mismatch");
        await _rackService.UpdateRackEnabledAsync(enabledDto);
        return NoContent();
    }
}

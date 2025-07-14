using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Growth.DTO;

namespace VHS.WebAPI.Controllers.Growth;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporary allowed.
public class WaterZoneController : ControllerBase
{
    private readonly IWaterZoneService _waterZoneService;

    public WaterZoneController(IWaterZoneService waterZoneService)
    {
        _waterZoneService = waterZoneService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWaterZones(Guid? farmId = null)
    {
        var zones = await _waterZoneService.GetAllWaterZonesAsync(farmId);
        return Ok(zones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWaterZoneById(Guid id)
    {
        var zone = await _waterZoneService.GetWaterZoneByIdAsync(id);
        if (zone == null)
            return NotFound();
        return Ok(zone);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWaterZone([FromBody] WaterZoneDTO waterZoneDto)
    {
        var createdZone = await _waterZoneService.CreateWaterZoneAsync(waterZoneDto);
        return CreatedAtAction(nameof(GetWaterZoneById), new { id = createdZone.Id }, createdZone);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWaterZone(Guid id, [FromBody] WaterZoneDTO waterZoneDto)
    {
        if (id != waterZoneDto.Id)
            return BadRequest("ID mismatch");

        await _waterZoneService.UpdateWaterZoneAsync(waterZoneDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWaterZone(Guid id)
    {
        await _waterZoneService.DeleteWaterZoneAsync(id);
        return NoContent();
    }
}

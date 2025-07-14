using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Growth.DTO;

namespace VHS.WebAPI.Controllers.Growth;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temp allow
public class LightZoneController : ControllerBase
{
    private readonly ILightZoneService _lightZoneService;

    public LightZoneController(ILightZoneService lightZoneService)
    {
        _lightZoneService = lightZoneService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLightZones(Guid? farmId = null)
    {
        var zones = await _lightZoneService.GetAllLightZonesAsync(farmId);
        return Ok(zones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLightZoneById(Guid id)
    {
        var zone = await _lightZoneService.GetLightZoneByIdAsync(id);
        if (zone == null)
            return NotFound();
        return Ok(zone);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLightZone([FromBody] LightZoneDTO lightZoneDto)
    {
        var createdZone = await _lightZoneService.CreateLightZoneAsync(lightZoneDto);
        return CreatedAtAction(nameof(GetLightZoneById), new { id = createdZone.Id }, createdZone);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLightZone(Guid id, [FromBody] LightZoneDTO lightZoneDto)
    {
        if (id != lightZoneDto.Id)
            return BadRequest("ID mismatch");

        await _lightZoneService.UpdateLightZoneAsync(lightZoneDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLightZone(Guid id)
    {
        await _lightZoneService.DeleteLightZoneAsync(id);
        return NoContent();
    }
}

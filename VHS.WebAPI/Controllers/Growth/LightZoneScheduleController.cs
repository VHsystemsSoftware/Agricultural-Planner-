using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Growth.DTO;

namespace VHS.WebAPI.Controllers.Growth;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporarily allowed
public class LightZoneScheduleController : ControllerBase
{
    private readonly ILightZoneScheduleService _scheduleService;

    public LightZoneScheduleController(ILightZoneScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLightZoneSchedules(Guid? farmId = null)
    {
        var schedules = await _scheduleService.GetAllLightZoneSchedulesAsync(farmId);
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLightZoneScheduleById(Guid id)
    {
        var schedule = await _scheduleService.GetLightZoneScheduleByIdAsync(id);
        if (schedule == null)
            return NotFound();
        return Ok(schedule);
    }

    [HttpGet("zone/{zoneId}")]
    public async Task<IActionResult> GetByZone(Guid zoneId)
    {
        var list = await _scheduleService.GetLightZoneSchedulesByZoneAsync(zoneId);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLightZoneSchedule([FromBody] LightZoneScheduleDTO scheduleDto)
    {
        var createdSchedule = await _scheduleService.CreateLightZoneScheduleAsync(scheduleDto);
        return CreatedAtAction(nameof(GetLightZoneScheduleById), new { id = createdSchedule.Id }, createdSchedule);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLightZoneSchedule(Guid id, [FromBody] LightZoneScheduleDTO scheduleDto)
    {
        if (id != scheduleDto.Id)
            return BadRequest("ID mismatch");

        await _scheduleService.UpdateLightZoneScheduleAsync(scheduleDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLightZoneSchedule(Guid id)
    {
        await _scheduleService.DeleteLightZoneScheduleAsync(id);
        return NoContent();
    }

    [HttpPost("calculatedli/{id}")]
    public async Task<IActionResult> CalculateDLI(Guid id)
    {
        double newDLI = await _scheduleService.CalculateDLIAsync(id);
        return Ok(new { CalculatedDLI = newDLI });
    }
}

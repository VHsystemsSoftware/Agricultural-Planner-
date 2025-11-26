using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Growth.DTO;

namespace VHS.WebAPI.Controllers.Growth;

[ApiController]
[Route("api/[controller]")]
public class WaterZoneScheduleController : ControllerBase
{
    private readonly IWaterZoneScheduleService _scheduleService;

    public WaterZoneScheduleController(IWaterZoneScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetAllWaterZoneSchedules(Guid? farmId = null)
    {
        var schedules = await _scheduleService.GetAllWaterZoneSchedulesAsync(farmId);
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetWaterZoneScheduleById(Guid id)
    {
        var schedule = await _scheduleService.GetWaterZoneScheduleByIdAsync(id);
        if (schedule == null)
            return NotFound();
        return Ok(schedule);
    }

    [HttpGet("zone/{zoneId}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetByZone(Guid zoneId)
    {
        var list = await _scheduleService.GetWaterZoneSchedulesByZoneAsync(zoneId);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> CreateWaterZoneSchedule([FromBody] WaterZoneScheduleDTO scheduleDto)
    {
        var createdSchedule = await _scheduleService.CreateWaterZoneScheduleAsync(scheduleDto);
        return CreatedAtAction(nameof(GetWaterZoneScheduleById), new { id = createdSchedule.Id }, createdSchedule);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> UpdateWaterZoneSchedule(Guid id, [FromBody] WaterZoneScheduleDTO scheduleDto)
    {
        if (id != scheduleDto.Id)
            return BadRequest("ID mismatch");

        await _scheduleService.UpdateWaterZoneScheduleAsync(scheduleDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> DeleteWaterZoneSchedule(Guid id)
    {
        await _scheduleService.DeleteWaterZoneScheduleAsync(id);
        return NoContent();
    }
}

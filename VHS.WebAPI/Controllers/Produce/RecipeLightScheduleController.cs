using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Produce.DTO;

namespace VHS.WebAPI.Controllers.Produce;

[ApiController]
[Route("api/[controller]")]
public class RecipeLightScheduleController : ControllerBase
{
    private readonly IRecipeLightScheduleService _scheduleService;

    public RecipeLightScheduleController(IRecipeLightScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetAllRecipeLightSchedules(Guid? farmId = null)
    {
        var schedules = await _scheduleService.GetAllRecipeLightSchedulesAsync(farmId);
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetRecipeLightScheduleById(Guid id)
    {
        var schedule = await _scheduleService.GetRecipeLightScheduleByIdAsync(id);
        if (schedule == null)
            return NotFound();
        return Ok(schedule);
    }

    [HttpPost]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> CreateRecipeLightSchedule([FromBody] RecipeLightScheduleDTO scheduleDto)
    {
        var createdSchedule = await _scheduleService.CreateRecipeLightScheduleAsync(scheduleDto);
        return CreatedAtAction(nameof(GetRecipeLightScheduleById), new { id = createdSchedule.Id }, createdSchedule);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> UpdateRecipeLightSchedule(Guid id, [FromBody] RecipeLightScheduleDTO scheduleDto)
    {
        if (id != scheduleDto.Id)
            return BadRequest("ID mismatch");

        await _scheduleService.UpdateRecipeLightScheduleAsync(scheduleDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> DeleteRecipeLightSchedule(Guid id)
    {
        await _scheduleService.DeleteRecipeLightScheduleAsync(id);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Produce.DTO;

namespace VHS.WebAPI.Controllers.Produce;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporarily allowed
public class RecipeWaterScheduleController : ControllerBase
{
    private readonly IRecipeWaterScheduleService _scheduleService;

    public RecipeWaterScheduleController(IRecipeWaterScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRecipeWaterSchedules(Guid? farmId = null)
    {
        var schedules = await _scheduleService.GetAllRecipeWaterSchedulesAsync(farmId);
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipeWaterScheduleById(Guid id)
    {
        var schedule = await _scheduleService.GetRecipeWaterScheduleByIdAsync(id);
        if (schedule == null)
            return NotFound();
        return Ok(schedule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecipeWaterSchedule([FromBody] RecipeWaterScheduleDTO scheduleDto)
    {
        var createdSchedule = await _scheduleService.CreateRecipeWaterScheduleAsync(scheduleDto);
        return CreatedAtAction(nameof(GetRecipeWaterScheduleById), new { id = createdSchedule.Id }, createdSchedule);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRecipeWaterSchedule(Guid id, [FromBody] RecipeWaterScheduleDTO scheduleDto)
    {
        if (id != scheduleDto.Id)
            return BadRequest("ID mismatch");

        await _scheduleService.UpdateRecipeWaterScheduleAsync(scheduleDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipeWaterSchedule(Guid id)
    {
        await _scheduleService.DeleteRecipeWaterScheduleAsync(id);
        return NoContent();
    }
}

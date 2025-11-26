using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/floor")]
public class FloorController : ControllerBase
{
    private readonly IFloorService _floorService;

    public FloorController(IFloorService floorService)
    {
        _floorService = floorService;
    }

    [HttpGet]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetAllFloors(Guid? farmId = null, bool enabledOnly = false)
    {
        var floors = await _floorService.GetAllFloorsAsync(farmId, enabledOnly);
        return Ok(floors);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetFloorById(Guid id)
    {
        var floor = await _floorService.GetFloorByIdAsync(id);
        if (floor == null)
            return NotFound();
        return Ok(floor);
    }

    [HttpPost]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> CreateFloor([FromBody] FloorDTO floorDto)
    {
        var createdFloor = await _floorService.CreateFloorAsync(floorDto);
        return CreatedAtAction(nameof(GetFloorById), new { id = createdFloor.Id }, createdFloor);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> UpdateFloor(Guid id, [FromBody] FloorDTO floorDto)
    {
        if (id != floorDto.Id)
            return BadRequest("ID mismatch");
        await _floorService.UpdateFloorAsync(floorDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> DeleteFloor(Guid id)
    {
        await _floorService.DeleteFloorAsync(id);
        return NoContent();
    }

    [HttpPut("enable/{id}")]
    [Authorize(Policy = "CanDefineRacksAndLayers")]
    public async Task<IActionResult> EnableRack(Guid id, [FromBody] EnabledDTO enabledDto)
    {
        if (id != enabledDto.Id)
            return BadRequest("ID mismatch");
        await _floorService.UpdateFloorEnabledAsync(enabledDto);
        return NoContent();
    }
}

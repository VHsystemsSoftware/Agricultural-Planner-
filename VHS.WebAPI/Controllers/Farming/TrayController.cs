using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/tray")]
[AllowAnonymous] // Temp allow
public class TrayController : ControllerBase
{
    private readonly ITrayService _trayService;

    public TrayController(ITrayService trayService)
    {
        _trayService = trayService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTrays(Guid farmId)
    {
        var trays = await _trayService.GetAllTraysAsync(farmId);
        return Ok(trays);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrayById(Guid id)
    {
        var tray = await _trayService.GetTrayByIdAsync(id);
        if (tray == null)
            return NotFound();
        return Ok(tray);
    }

    //[HttpGet("status")]
    //public async Task<IActionResult> GetTraysByStatus(
    //    [FromQuery] Guid trayStatus,
    //    [FromQuery] int pageIndex,
    //    [FromQuery] int pageSize,
    //    [FromQuery] string sortColumn,
    //    [FromQuery] SortDirectionEnum sortDirection)
    //{
    //    if (trayStatus == Guid.Empty)
    //        return BadRequest("Tray status is required.");

    //    var result = await _trayService.GetTraysByStatusAsync(trayStatus, pageIndex, pageSize, sortColumn, sortDirection);
    //    return Ok(result);
    //}

    [HttpPost]
    public async Task<IActionResult> CreateTray([FromBody] TrayDTO trayDto)
    {
        var createdTray = await _trayService.CreateTrayAsync(trayDto);
        return CreatedAtAction(nameof(GetTrayById), new { id = createdTray.Id }, createdTray);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTray(Guid id, [FromBody] TrayDTO trayDto)
    {
        if (id != trayDto.Id)
            return BadRequest("ID mismatch");

        await _trayService.UpdateTrayAsync(trayDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTray(Guid id)
    {
        await _trayService.DeleteTrayAsync(id);
        return NoContent();
    }
}

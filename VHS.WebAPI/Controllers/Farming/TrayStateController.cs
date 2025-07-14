using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/traystate")]
[AllowAnonymous] // Temp allow
public class TrayStateController : ControllerBase
{
    private readonly ITrayStateService _trayStateService;

    public TrayStateController(ITrayStateService trayStateService)
    {
        _trayStateService = trayStateService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var trays = await _trayStateService.GetCurrentStates();
        return Ok(trays);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrayState(Guid id, [FromBody] TrayStateDTO trayDto)
    {
        if (id != trayDto.Id)
            return BadRequest("ID mismatch");

        try
        {
            await _trayStateService.UpdateTrayStateAsync(trayDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

}

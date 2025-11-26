using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/traystate")]
public class TrayStateController : ControllerBase
{
    private readonly ITrayStateService _trayStateService;

    public TrayStateController(ITrayStateService trayStateService)
    {
        _trayStateService = trayStateService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet("current")]
    //[Authorize(Policy = "CanViewDashboards")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrent()
    {
        var trays = await _trayStateService.GetCurrentStates();
        return Ok(trays);
    }

    [HttpGet("{batchId}")]
    //[Authorize(Policy = "CanViewDashboards")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatesForBatch(Guid batchId)
    {
        var trays = await _trayStateService.GetCurrentStates(batchId);
        return Ok(trays);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanAccessOverviewOperations")]
    public async Task<IActionResult> UpdateTrayState(Guid id, [FromBody] TrayStateDTO trayDto)
    {
        if (id != trayDto.Id)
            return BadRequest("ID mismatch");

        try
        {
            var tray = await _trayStateService.UpdateTrayStateAsync(trayDto, GetCurrentUserId());
            return Ok(tray);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

}

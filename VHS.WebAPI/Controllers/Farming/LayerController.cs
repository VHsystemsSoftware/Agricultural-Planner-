using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.WebAPI.Controllers.Farming;

[ApiController]
[Route("api/layer")]
[AllowAnonymous] // Temp allow
public class LayerController : ControllerBase
{
    private readonly ILayerService _layerService;

    public LayerController(ILayerService layerService)
    {
        _layerService = layerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLayers(Guid? farmId = null)
    {
        var layers = await _layerService.GetAllLayersAsync(farmId);
        return Ok(layers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLayerById(Guid id)
    {
        var layer = await _layerService.GetLayerByIdAsync(id);
        if (layer == null)
            return NotFound();
        return Ok(layer);
    }

    [HttpGet("rack")]
    public async Task<IActionResult> GetLayersByRack(
        [FromQuery] Guid rackId,
        [FromQuery] int pageIndex,
        [FromQuery] int pageSize,
        [FromQuery] string sortColumn,
        [FromQuery] SortDirectionEnum sortDirection,
        [FromQuery] string? rackNamePrefix = null)
    {
        if (rackId == Guid.Empty)
        {
            return BadRequest("Rack ID is required.");
        }

        var result = await _layerService.GetLayersByRackAsync(
            rackId, pageIndex, pageSize, sortColumn, sortDirection,
            string.IsNullOrWhiteSpace(rackNamePrefix) ? "SK" : rackNamePrefix);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLayer([FromBody] LayerDTO layerDto)
    {
        var createdLayer = await _layerService.CreateLayerAsync(layerDto);
        return CreatedAtAction(nameof(GetLayerById), new { id = createdLayer.Id }, createdLayer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLayer(Guid id, [FromBody] LayerDTO layerDto)
    {
        if (id != layerDto.Id)
            return BadRequest("ID mismatch");
        await _layerService.UpdateLayerAsync(layerDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLayer(Guid id)
    {
        await _layerService.DeleteLayerAsync(id);
        return NoContent();
    }

    [HttpPut("enable/{id}")]
    public async Task<IActionResult> EnableRack(Guid id, [FromBody] EnabledDTO enabledDto)
    {
        if (id != enabledDto.Id)
            return BadRequest("ID mismatch");
        await _layerService.UpdateLayerEnabledAsync(enabledDto);
        return NoContent();
    }
}

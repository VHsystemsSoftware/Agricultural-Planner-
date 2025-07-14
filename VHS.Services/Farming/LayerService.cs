using Microsoft.EntityFrameworkCore;
using VHS.Data.Core.Models;
using VHS.Services.Common.DataGrid;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.Services;

public interface ILayerService
{
    Task<IEnumerable<LayerDTO>> GetAllLayersAsync(Guid? farmId = null);
    Task<LayerDTO?> GetLayerByIdAsync(Guid id);
    Task<PaginatedResult<LayerDTO>> GetLayersByRackAsync(Guid rackId, int page, int pageSize, string? sortLabel, SortDirectionEnum sortDirection, string? rackNamePrefix);
    Task<LayerDTO> CreateLayerAsync(LayerDTO layerDto);
    Task UpdateLayerAsync(LayerDTO layerDto);
    Task UpdateLayerEnabledAsync(EnabledDTO enabledDto);
    Task DeleteLayerAsync(Guid id);
    Task InsertLayersForRacksAsync(Guid farmId);
}

public class LayerService : ILayerService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public LayerService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static LayerDTO SelectLayerToDTO(Layer layer) => new LayerDTO
    {
        Id = layer.Id,
        RackId = layer.Rack.Id,
        TrayCountPerLayer = layer.Rack.TrayCountPerLayer,
        Number = layer.Number,
        Enabled = layer.Enabled,
    };

    public async Task<IEnumerable<LayerDTO>> GetAllLayersAsync(Guid? farmId = null)
    {
        var layers = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.Layer.GetAllAsync(x => x.Rack.Floor.FarmId == farmId.Value)
            : await _unitOfWork.Layer.GetAllAsync();
        return layers.Select(l => new LayerDTO
        {
            Id = l.Id,
            RackId =l.Rack.Id,
            IsTransportLayer = l.Number==l.Rack.LayerCount,
            TrayCountPerLayer = l.Rack.TrayCountPerLayer,
				Number = l.Number
			});
    }

    public async Task<LayerDTO?> GetLayerByIdAsync(Guid id)
    {
        var layer = await _unitOfWork.Layer.GetByIdAsync(id);
        if (layer == null)
            return null;
        return SelectLayerToDTO(layer);
    }       

    public async Task<LayerDTO> CreateLayerAsync(LayerDTO layerDto)
    {
        var layer = new Layer
        {
            Id = layerDto.Id == Guid.Empty ? Guid.NewGuid() : layerDto.Id,
            RackId = layerDto.RackId,
				Number = layerDto.Number,
            AddedDateTime = DateTime.UtcNow,
        };

        await _unitOfWork.Layer.AddAsync(layer);
        await _unitOfWork.SaveChangesAsync();

        return SelectLayerToDTO(layer);
    }

    public async Task UpdateLayerAsync(LayerDTO layerDto)
    {
        var layer = await _unitOfWork.Layer.GetByIdAsync(layerDto.Id);
        if (layer == null)
            throw new Exception("Layer not found");

        layer.RackId = layerDto.RackId;
        layer.Number = layerDto.Number;
        layer.Enabled = layerDto.Enabled;
        _unitOfWork.Layer.Update(layer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateLayerEnabledAsync(EnabledDTO enabledDto)
    {
        var layer = await _unitOfWork.Layer.GetByIdAsync(enabledDto.Id);
        if (layer == null)
            throw new KeyNotFoundException("Rack not found");
        layer.Enabled = enabledDto.Enabled;
        _unitOfWork.Layer.Update(layer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteLayerAsync(Guid id)
    {
        var layer = await _unitOfWork.Layer.GetByIdAsync(id);
        if (layer == null)
            throw new Exception("Layer not found");

        layer.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.Layer.Update(layer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task InsertLayersForRacksAsync(Guid farmId)
    {
        var racks = await _unitOfWork.Rack.GetAllAsync(r => r.Floor.FarmId == farmId);

        if (racks == null || !racks.Any())
        {
            throw new ArgumentException("No racks available to assign layers for the given farm.");
        }

        var layersToInsert = new List<Layer>();

        int rackCount = 0;
        foreach (var rack in racks.OrderBy(x => x.Name))
        {
            rackCount++;
            var floor = await _unitOfWork.Floor.GetByIdAsync(rack.FloorId);
            if (floor == null)
            {
                throw new Exception($"Floor not found for Rack {rack.Name}");
            }

            for (int layerNum = 1; layerNum <= rack.LayerCount; layerNum++)
            {
                var newLayer = new Layer
                {
                    Id = Guid.NewGuid(),
                    RackId = rack.Id,
						Number = layerNum,
                    AddedDateTime = DateTime.UtcNow,
                };
                layersToInsert.Add(newLayer);
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.Layer.AddRangeAsync(layersToInsert);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<PaginatedResult<LayerDTO>> GetLayersByRackAsync(Guid rackId, int page, int pageSize, string? sortLabel, SortDirectionEnum sortDirection, string? rackNamePrefix = "SK")
    {
        var query = _unitOfWork.Layer.Query()
            .Include(l => l.Rack)
            .Where(l => l.RackId == rackId);

        var projectedData = await query
            .Select(l => new
            {
                l.Id,
                l.RackId,
                RackName = l.Rack.Name,
                FloorName = l.Rack.Floor.Name,
                l.Number,
                l.Enabled,
                l.AddedDateTime
            })
            .ToListAsync();

        var data = projectedData
            .OrderBy(l => l.Number)
            .Select(l => {
            return new LayerDTO
            {
                Id = l.Id,
					Number = l.Number,
                Enabled = l.Enabled
            };
        }).ToList();

        var totalCount = data.Count;
        var items = data.Skip(page * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<LayerDTO>
        {
            TotalCount = totalCount,
            Items = items
        };
    }


}

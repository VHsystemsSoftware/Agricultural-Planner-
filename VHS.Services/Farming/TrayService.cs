using Microsoft.EntityFrameworkCore;
using VHS.Services.Common.DataGrid;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.Services;

public interface ITrayService
{
    Task<IEnumerable<TrayDTO>> GetAllTraysAsync(Guid farmId);
    Task<TrayDTO?> GetTrayByIdAsync(Guid id);
    Task<TrayDTO> CreateTrayAsync(TrayDTO trayDto);
    Task UpdateTrayAsync(TrayDTO trayDto);
    Task DeleteTrayAsync(Guid id);
    //Task<PaginatedResult<TrayDTO>> GetTraysByStatusAsync(Guid trayStatus, int page, int pageSize, string? sortLabel, SortDirectionEnum sortDirection);
}

public class TrayService : ITrayService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public TrayService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static TrayDTO SelectTrayToDTO(Tray x) => new TrayDTO
    {
        Id = x.Id,
        FarmId = x.FarmId,
        Tag = x.Tag,
        StatusId = x.StatusId
    };

    public async Task<IEnumerable<TrayDTO>> GetAllTraysAsync(Guid farmId)
    {
        var trays = await _unitOfWork.Tray.GetAllAsync(x => x.FarmId == farmId);
        return trays.Select(SelectTrayToDTO);
    }

    public async Task<TrayDTO?> GetTrayByIdAsync(Guid id)
    {
        var tray = await _unitOfWork.Tray.GetByIdAsync(id);
        if (tray == null)
            return null;
        return SelectTrayToDTO(tray);
    }

    public async Task<TrayDTO> CreateTrayAsync(TrayDTO trayDto)
    {
        var trayEntity = new Tray
        {
            Id = trayDto.Id == Guid.Empty ? Guid.NewGuid() : trayDto.Id,
            FarmId = trayDto.FarmId,
				Tag = trayDto.Tag,
            StatusId = trayDto.StatusId,
        };

        await _unitOfWork.Tray.AddAsync(trayEntity);
        await _unitOfWork.SaveChangesAsync();

        return new TrayDTO
        {
            Id = trayEntity.Id,
            FarmId = trayEntity.FarmId,
				Tag = trayEntity.Tag,
            StatusId = trayEntity.StatusId,
        };
    }

    public async Task UpdateTrayAsync(TrayDTO trayDto)
    {
        var tray = await _unitOfWork.Tray.GetByIdAsync(trayDto.Id);
        if (tray == null)
            throw new Exception("Tray not found");

        tray.Tag = trayDto.Tag;
        tray.StatusId = trayDto.StatusId;

        _unitOfWork.Tray.Update(tray);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteTrayAsync(Guid id)
    {
        var tray = await _unitOfWork.Tray.GetByIdAsync(id);
        if (tray == null)
            throw new Exception("Tray not found");

        tray.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.Tray.Update(tray);
        await _unitOfWork.SaveChangesAsync();
    }          

    
    //public async Task<PaginatedResult<TrayDTO>> GetTraysByStatusAsync(Guid trayStatus, int page, int pageSize, string? sortLabel, SortDirectionEnum sortDirection)
    //{
    //    var query = _unitOfWork.TrayCurrentState.Query().Where(t => t.CurrentPhaseId == trayStatus && t.Tray.DeletedDateTime == null && t.Tray.Farm.DeletedDateTime == null);

    //    var data = await query.Select(t => new TrayDTO
    //    {
    //        Id = t.Tray.Id,
    //        FarmId = t.Tray.FarmId,
    //        Tag = t.Tray.Tag,
    //        StatusId = t.Tray.StatusId,
    //        CurrentPhaseId = t.CurrentPhaseId
    //    }).ToListAsync();

    //    if (!string.IsNullOrWhiteSpace(sortLabel))
    //    {
    //        var propertyInfo = typeof(TrayDTO).GetProperty(sortLabel);
    //        if (propertyInfo != null)
    //        {
    //            data = sortDirection == SortDirectionEnum.Ascending
    //                ? data.OrderBy(t => propertyInfo.GetValue(t)).ToList()
    //                : data.OrderByDescending(t => propertyInfo.GetValue(t)).ToList();
    //        }
    //        else
    //        {
    //            data = data.OrderByDescending(t => t.Id).ToList();
    //        }
    //    }
    //    else
    //    {
    //        data = data.OrderByDescending(t => t.Id).ToList();
    //    }

    //    var totalCount = data.Count;
    //    var items = data.Skip(page * pageSize).Take(pageSize).ToList();

    //    return new PaginatedResult<TrayDTO>
    //    {
    //        TotalCount = totalCount,
    //        Items = items
    //    };
    //}
}

using VHS.Services.Growth.DTO;

namespace VHS.Services;

public interface IWaterZoneService
{
    Task<IEnumerable<WaterZoneDTO>> GetAllWaterZonesAsync(Guid? farmId = null);
    Task<WaterZoneDTO?> GetWaterZoneByIdAsync(Guid id);
    Task<WaterZoneDTO> CreateWaterZoneAsync(WaterZoneDTO waterZoneDto);
    Task UpdateWaterZoneAsync(WaterZoneDTO waterZoneDto);
    Task DeleteWaterZoneAsync(Guid id);
}

public class WaterZoneService : IWaterZoneService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public WaterZoneService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    private static WaterZoneDTO SelectWaterZoneToDTO(WaterZone wz) => new WaterZoneDTO
    {
        Id = wz.Id,
        FarmId = wz.FarmId,
        Name = wz.Name,
        TargetDWR = wz.TargetDWR,
    };

    public async Task<IEnumerable<WaterZoneDTO>> GetAllWaterZonesAsync(Guid? farmId = null)
    {
        var zones = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.WaterZone.GetAllAsync(x => x.FarmId == farmId.Value)
            : await _unitOfWork.WaterZone.GetAllAsync();
        return zones
            .OrderBy(wz => wz.Name)
            .Select(SelectWaterZoneToDTO);
    }

    public async Task<WaterZoneDTO?> GetWaterZoneByIdAsync(Guid id)
    {
        var zone = await _unitOfWork.WaterZone.GetByIdWithIncludesAsync(id);
        if (zone == null)
            return null;

        return SelectWaterZoneToDTO(zone);
    }

    public async Task<WaterZoneDTO> CreateWaterZoneAsync(WaterZoneDTO waterZoneDto)
    {
        var zone = new WaterZone
        {
            Id = waterZoneDto.Id == Guid.Empty ? Guid.NewGuid() : waterZoneDto.Id,
            FarmId = waterZoneDto.FarmId,
            Name = waterZoneDto.Name,
            TargetDWR = waterZoneDto.TargetDWR
        };

        await _unitOfWork.WaterZone.AddAsync(zone);
        await _unitOfWork.SaveChangesAsync();        

        return SelectWaterZoneToDTO(zone);
    }

    public async Task UpdateWaterZoneAsync(WaterZoneDTO waterZoneDto)
    {
        var zone = await _unitOfWork.WaterZone.GetByIdAsync(waterZoneDto.Id);
        if (zone == null)
            throw new Exception("WaterZone not found");

        zone.Name = waterZoneDto.Name;
        zone.FarmId = waterZoneDto.FarmId;
        zone.TargetDWR = waterZoneDto.TargetDWR;
        zone.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.WaterZone.Update(zone);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteWaterZoneAsync(Guid id)
    {
        var zone = await _unitOfWork.WaterZone.GetByIdAsync(id);
        if (zone == null)
            throw new Exception("WaterZone not found");

        zone.DeletedDateTime = DateTime.UtcNow;

        _unitOfWork.WaterZone.Update(zone);
        await _unitOfWork.SaveChangesAsync();
    }
}

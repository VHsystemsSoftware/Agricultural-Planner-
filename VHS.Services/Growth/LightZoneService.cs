using VHS.Services.Growth.DTO;

namespace VHS.Services;

public interface ILightZoneService
{
    Task<IEnumerable<LightZoneDTO>> GetAllLightZonesAsync(Guid? farmId = null);
    Task<LightZoneDTO?> GetLightZoneByIdAsync(Guid id);
    Task<LightZoneDTO> CreateLightZoneAsync(LightZoneDTO lightZoneDto);
    Task UpdateLightZoneAsync(LightZoneDTO lightZoneDto);
    Task DeleteLightZoneAsync(Guid id);
}
public class LightZoneService : ILightZoneService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public LightZoneService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static LightZoneDTO SelectLightZoneToDTO(LightZone z) => new LightZoneDTO
    {
        Id = z.Id,
        FarmId = z.FarmId,
        Name = z.Name,
        TargetDLI = z.TargetDLI,
    };

    public async Task<IEnumerable<LightZoneDTO>> GetAllLightZonesAsync(Guid? farmId = null)
    {
        var zones = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.LightZone.GetAllAsync(x => x.FarmId == farmId.Value)
            : await _unitOfWork.LightZone.GetAllAsync();
        return zones
            .OrderBy(z => z.Name)
            .Select(SelectLightZoneToDTO);
    }

    public async Task<LightZoneDTO?> GetLightZoneByIdAsync(Guid id)
    {
        var zone = await _unitOfWork.LightZone.GetByIdWithIncludesAsync(id);
        if (zone == null)
            return null;

        return SelectLightZoneToDTO(zone);
    }

    public async Task<LightZoneDTO> CreateLightZoneAsync(LightZoneDTO lightZoneDto)
    {
        var zone = new LightZone
        {
            Id = lightZoneDto.Id == Guid.Empty ? Guid.NewGuid() : lightZoneDto.Id,
            FarmId = lightZoneDto.FarmId,
            Name = lightZoneDto.Name,
            TargetDLI = lightZoneDto.TargetDLI
        };

        await _unitOfWork.LightZone.AddAsync(zone);
        await _unitOfWork.SaveChangesAsync();

        return SelectLightZoneToDTO(zone);
    }

    public async Task UpdateLightZoneAsync(LightZoneDTO lightZoneDto)
    {
        var zone = await _unitOfWork.LightZone.GetByIdAsync(lightZoneDto.Id);
        if (zone == null)
            throw new Exception("LightZone not found");

        zone.Name = lightZoneDto.Name;
        zone.FarmId = lightZoneDto.FarmId;
        zone.TargetDLI = lightZoneDto.TargetDLI;
        zone.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.LightZone.Update(zone);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteLightZoneAsync(Guid id)
    {
        var zone = await _unitOfWork.LightZone.GetByIdAsync(id);
        if (zone == null)
            throw new Exception("LightZone not found");

        zone.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.LightZone.Update(zone);

        await _unitOfWork.SaveChangesAsync();
    }
}

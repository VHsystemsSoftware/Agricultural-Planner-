using VHS.Services.Growth.DTO;
using VHS.Services.Growth.Helpers;

namespace VHS.Services;

public interface IWaterZoneScheduleService
{
    Task<IEnumerable<WaterZoneScheduleDTO>> GetAllWaterZoneSchedulesAsync(Guid? farmId = null);
    Task<WaterZoneScheduleDTO?> GetWaterZoneScheduleByIdAsync(Guid id);
    Task<IEnumerable<WaterZoneScheduleDTO>> GetWaterZoneSchedulesByZoneAsync(Guid WaterZoneId);
    Task<WaterZoneScheduleDTO> CreateWaterZoneScheduleAsync(WaterZoneScheduleDTO scheduleDto);
    Task UpdateWaterZoneScheduleAsync(WaterZoneScheduleDTO scheduleDto);
    Task DeleteWaterZoneScheduleAsync(Guid id);
}

public class WaterZoneScheduleService : IWaterZoneScheduleService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public WaterZoneScheduleService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    private static WaterZoneScheduleDTO SelectWaterZoneScheduleToDTO(WaterZoneSchedule s) => new WaterZoneScheduleDTO
    {
        Id = s.Id,
        WaterZone = new WaterZoneDTO
        {
            Id = s.WaterZoneId,
            Name = s.WaterZone?.Name ?? string.Empty
        },
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Volume = s.Volume,
        VolumeUnit = s.VolumeUnit,
        CalculatedDWR = s.CalculatedDWR,
    };

    public async Task<IEnumerable<WaterZoneScheduleDTO>> GetAllWaterZoneSchedulesAsync(Guid? farmId = null)
    {
        var schedules = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.WaterZoneSchedule.GetAllAsync(x => x.WaterZone.FarmId == farmId)
            : await _unitOfWork.WaterZoneSchedule.GetAllAsync();
        return schedules.OrderBy(s => s.AddedDateTime).Select(SelectWaterZoneScheduleToDTO);
    }

    public async Task<WaterZoneScheduleDTO?> GetWaterZoneScheduleByIdAsync(Guid id)
    {
        var includes = new string[] { "WaterZone" };
        var schedule = await _unitOfWork.WaterZoneSchedule.GetByIdWithIncludesAsync(id, includes);
        if (schedule == null)
            return null;
        return new WaterZoneScheduleDTO
        {
            Id = schedule.Id,
            //WaterZoneId = schedule.WaterZoneId,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Volume = schedule.Volume,
            VolumeUnit = schedule.VolumeUnit,
            CalculatedDWR = schedule.CalculatedDWR,
        };
    }

    public async Task<IEnumerable<WaterZoneScheduleDTO>> GetWaterZoneSchedulesByZoneAsync(Guid WaterZoneId)
    {
        var schedules = await _unitOfWork.WaterZoneSchedule.GetAllAsync(x => x.WaterZoneId == WaterZoneId);
        return schedules
            .OrderBy(s => s.AddedDateTime)
            .Select(SelectWaterZoneScheduleToDTO);
    }

    public async Task<WaterZoneScheduleDTO> CreateWaterZoneScheduleAsync(WaterZoneScheduleDTO scheduleDto)
    {
        var scheduleEntity = new WaterZoneSchedule
        {
            Id = scheduleDto.Id == Guid.Empty ? Guid.NewGuid() : scheduleDto.Id,
            WaterZoneId = scheduleDto.WaterZone.Id,
            StartTime = scheduleDto.StartTime,
            EndTime = scheduleDto.EndTime,
            Volume = scheduleDto.Volume,
            VolumeUnit = scheduleDto.VolumeUnit,
            CalculatedDWR = scheduleDto.CalculatedDWR
        };

        if (scheduleEntity.CalculatedDWR == null)
        {
            scheduleEntity.CalculatedDWR = WaterZoneScheduleDWRHelper.CalculateDWR(scheduleEntity.Volume, scheduleEntity.StartTime, scheduleEntity.EndTime);
        }

        await _unitOfWork.WaterZoneSchedule.AddAsync(scheduleEntity);
        await _unitOfWork.SaveChangesAsync();

        return SelectWaterZoneScheduleToDTO(scheduleEntity);
    }

    public async Task UpdateWaterZoneScheduleAsync(WaterZoneScheduleDTO scheduleDto)
    {
        var schedule = await _unitOfWork.WaterZoneSchedule.GetByIdAsync(scheduleDto.Id);
        if (schedule == null)
            throw new Exception("WaterZoneSchedule not found");

        schedule.StartTime = scheduleDto.StartTime;
        schedule.EndTime = scheduleDto.EndTime;
        schedule.Volume = scheduleDto.Volume;
        schedule.VolumeUnit = scheduleDto.VolumeUnit;
        schedule.CalculatedDWR = scheduleDto.CalculatedDWR;
        schedule.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.WaterZoneSchedule.Update(schedule);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteWaterZoneScheduleAsync(Guid id)
    {
        var schedule = await _unitOfWork.WaterZoneSchedule.GetByIdAsync(id);
        if (schedule == null)
            throw new Exception("WaterZoneSchedule not found");

        schedule.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.WaterZoneSchedule.Update(schedule);
        await _unitOfWork.SaveChangesAsync();
    }
}

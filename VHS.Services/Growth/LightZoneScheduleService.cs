using VHS.Services.Growth.DTO;
using VHS.Services.Growth.Helpers;

namespace VHS.Services;

public interface ILightZoneScheduleService
{
    Task<IEnumerable<LightZoneScheduleDTO>> GetAllLightZoneSchedulesAsync(Guid? farmId = null);
    Task<LightZoneScheduleDTO?> GetLightZoneScheduleByIdAsync(Guid id);
    Task<IEnumerable<LightZoneScheduleDTO>> GetLightZoneSchedulesByZoneAsync(Guid LightZoneId);
    Task<LightZoneScheduleDTO> CreateLightZoneScheduleAsync(LightZoneScheduleDTO scheduleDto);
    Task UpdateLightZoneScheduleAsync(LightZoneScheduleDTO scheduleDto);
    Task DeleteLightZoneScheduleAsync(Guid id);
    Task<double> CalculateDLIAsync(Guid scheduleId);
}

public class LightZoneScheduleService : ILightZoneScheduleService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public LightZoneScheduleService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static LightZoneScheduleDTO SelecteLightzoneToDTO(LightZoneSchedule s) => new LightZoneScheduleDTO
    {
        Id = s.Id,
        LightZone = new LightZoneDTO
        {
            Id = s.LightZoneId,
            Name = s.LightZone?.Name ?? string.Empty
        },
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Intensity = s.Intensity,
        CalculatedDLI = s.CalculatedDLI,
    };

    public async Task<IEnumerable<LightZoneScheduleDTO>> GetAllLightZoneSchedulesAsync(Guid? farmId = null)
    {
        var schedules = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.LightZoneSchedule.GetAllAsync(x => x.LightZone.FarmId == farmId)
            : await _unitOfWork.LightZoneSchedule.GetAllAsync();
        return schedules
            .OrderBy(s => s.AddedDateTime)
            .Select(SelecteLightzoneToDTO);
    }

    public async Task<LightZoneScheduleDTO?> GetLightZoneScheduleByIdAsync(Guid id)
    {
        var schedule = await _unitOfWork.LightZoneSchedule.GetByIdWithIncludesAsync(id);
        if (schedule == null) return null;
        return SelecteLightzoneToDTO(schedule);
    }

    public async Task<IEnumerable<LightZoneScheduleDTO>> GetLightZoneSchedulesByZoneAsync(Guid LightZoneId)
    {
        var schedules = await _unitOfWork.LightZoneSchedule.GetAllAsync(x => x.LightZoneId == LightZoneId);
        return schedules
            .OrderBy(s => s.AddedDateTime)
            .Select(SelecteLightzoneToDTO);
    }

    public async Task<LightZoneScheduleDTO> CreateLightZoneScheduleAsync(LightZoneScheduleDTO scheduleDto)
    {
        var schedule = new LightZoneSchedule
        {
            Id = scheduleDto.Id == Guid.Empty ? Guid.NewGuid() : scheduleDto.Id,
            LightZoneId = scheduleDto.LightZone.Id,
            StartTime = scheduleDto.StartTime,
            EndTime = scheduleDto.EndTime,
            Intensity = scheduleDto.Intensity,
            CalculatedDLI = scheduleDto.CalculatedDLI
        };

        if (schedule.CalculatedDLI == null)
        {
            schedule.CalculatedDLI = LightZoneScheduleDLIHelper.CalculateDLI(schedule.Intensity, schedule.StartTime, schedule.EndTime);
        }

        await _unitOfWork.LightZoneSchedule.AddAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        return SelecteLightzoneToDTO(schedule);
    }

    public async Task UpdateLightZoneScheduleAsync(LightZoneScheduleDTO scheduleDto)
    {
        var schedule = await _unitOfWork.LightZoneSchedule.GetByIdAsync(scheduleDto.Id);
        if (schedule == null)
            throw new Exception("LightZoneSchedule not found");

        schedule.StartTime = scheduleDto.StartTime;
        schedule.EndTime = scheduleDto.EndTime;
        schedule.Intensity = scheduleDto.Intensity;
        schedule.CalculatedDLI = scheduleDto.CalculatedDLI;
        schedule.ModifiedDateTime = DateTime.UtcNow;
        if (schedule.CalculatedDLI == null)
        {
            schedule.CalculatedDLI = LightZoneScheduleDLIHelper.CalculateDLI(schedule.Intensity, schedule.StartTime, schedule.EndTime);
        }

        _unitOfWork.LightZoneSchedule.Update(schedule);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteLightZoneScheduleAsync(Guid id)
    {
        var schedule = await _unitOfWork.LightZoneSchedule.GetByIdAsync(id);
        if (schedule == null)
            throw new Exception("LightZoneSchedule not found");

        schedule.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.LightZoneSchedule.Update(schedule);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<double> CalculateDLIAsync(Guid scheduleId)
    {
        var schedule = await _unitOfWork.LightZoneSchedule.GetByIdAsync(scheduleId);
        if (schedule == null)
            throw new Exception("Schedule not found");

        double newDLI = LightZoneScheduleDLIHelper.CalculateDLI(schedule.Intensity, schedule.StartTime, schedule.EndTime);
        schedule.CalculatedDLI = newDLI;
        schedule.ModifiedDateTime = DateTime.UtcNow;
        _unitOfWork.LightZoneSchedule.Update(schedule);
        await _unitOfWork.SaveChangesAsync();
        return newDLI;
    }
}

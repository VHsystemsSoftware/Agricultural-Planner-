using VHS.Services.Growth.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;
public interface IRecipeLightScheduleService
{
    Task<IEnumerable<RecipeLightScheduleDTO>> GetAllRecipeLightSchedulesAsync(Guid? farmId = null);
    Task<RecipeLightScheduleDTO?> GetRecipeLightScheduleByIdAsync(Guid id);
    Task<RecipeLightScheduleDTO> CreateRecipeLightScheduleAsync(RecipeLightScheduleDTO scheduleDto);
    Task UpdateRecipeLightScheduleAsync(RecipeLightScheduleDTO scheduleDto);
    Task DeleteRecipeLightScheduleAsync(Guid id);
}
public class RecipeLightScheduleService : IRecipeLightScheduleService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public RecipeLightScheduleService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static RecipeLightScheduleDTO SelectLightScheduleToDTO(RecipeLightSchedule rls) => new RecipeLightScheduleDTO
    {
        Id = rls.Id,
        Recipe = rls.Recipe != null ? new RecipeDTO
        {
            Id = rls.RecipeId,
            Name = rls.Recipe.Name ?? string.Empty
        } : new RecipeDTO { Id = rls.RecipeId, Name = string.Empty },
        LightZoneSchedule = rls.LightZoneSchedule != null ? new LightZoneScheduleDTO
        {
            Id = rls.LightZoneScheduleId,
            LightZone = rls.LightZoneSchedule.LightZone != null ? new LightZoneDTO
            {
                Id = rls.LightZoneSchedule.LightZone.Id,
                Name = rls.LightZoneSchedule.LightZone.Name ?? string.Empty
            } : new LightZoneDTO(),
            StartTime = rls.LightZoneSchedule.StartTime,
            EndTime = rls.LightZoneSchedule.EndTime,
            CalculatedDLI = rls.LightZoneSchedule.CalculatedDLI
        } : new LightZoneScheduleDTO(),
        TargetDLI = rls.TargetDLI,
        AddedDateTime = rls.AddedDateTime,
        ModifiedDateTime = rls.ModifiedDateTime
    };

    public async Task<IEnumerable<RecipeLightScheduleDTO>> GetAllRecipeLightSchedulesAsync(Guid? farmId = null)
    {
        var schedules = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.RecipeLightSchedule.GetAllAsync(x => x.Recipe.Product.FarmId == farmId)
            : await _unitOfWork.RecipeLightSchedule.GetAllAsync();

        return schedules.OrderBy(rls => rls.AddedDateTime)
            .Select(schedule => SelectLightScheduleToDTO(schedule));
    }

    public async Task<RecipeLightScheduleDTO?> GetRecipeLightScheduleByIdAsync(Guid id)
    {
        var schedule = await _unitOfWork.RecipeLightSchedule.GetByIdWithIncludesAsync(id);
        if (schedule == null)
            return null;
        return SelectLightScheduleToDTO(schedule);
    }

    public async Task<RecipeLightScheduleDTO> CreateRecipeLightScheduleAsync(RecipeLightScheduleDTO scheduleDto)
    {
        var schedule = new RecipeLightSchedule
        {
            Id = scheduleDto.Id == Guid.Empty ? Guid.NewGuid() : scheduleDto.Id,
            RecipeId = scheduleDto.Recipe.Id,
            LightZoneScheduleId = scheduleDto.LightZoneSchedule.Id,
            TargetDLI = scheduleDto.TargetDLI
        };

        await _unitOfWork.RecipeLightSchedule.AddAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        return SelectLightScheduleToDTO(schedule);
    }

    public async Task UpdateRecipeLightScheduleAsync(RecipeLightScheduleDTO scheduleDto)
    {
        var entity = await _unitOfWork.RecipeLightSchedule.GetByIdAsync(scheduleDto.Id);
        if (entity == null)
            throw new Exception("RecipeLightSchedule not found");

        entity.RecipeId = scheduleDto.Recipe.Id;
        entity.LightZoneScheduleId = scheduleDto.LightZoneSchedule.Id;
        entity.TargetDLI = scheduleDto.TargetDLI;
        entity.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.RecipeLightSchedule.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteRecipeLightScheduleAsync(Guid id)
    {
        var entity = await _unitOfWork.RecipeLightSchedule.GetByIdAsync(id);
        if (entity == null)
            throw new Exception("RecipeLightSchedule not found");

        entity.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.RecipeLightSchedule.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}

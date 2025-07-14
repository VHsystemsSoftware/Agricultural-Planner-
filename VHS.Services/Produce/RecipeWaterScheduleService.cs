using VHS.Services.Growth.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IRecipeWaterScheduleService
{
    Task<IEnumerable<RecipeWaterScheduleDTO>> GetAllRecipeWaterSchedulesAsync(Guid? farmId = null);
    Task<RecipeWaterScheduleDTO?> GetRecipeWaterScheduleByIdAsync(Guid id);
    Task<RecipeWaterScheduleDTO> CreateRecipeWaterScheduleAsync(RecipeWaterScheduleDTO scheduleDto);
    Task UpdateRecipeWaterScheduleAsync(RecipeWaterScheduleDTO scheduleDto);
    Task DeleteRecipeWaterScheduleAsync(Guid id);
}
public class RecipeWaterScheduleService : IRecipeWaterScheduleService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public RecipeWaterScheduleService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static RecipeWaterScheduleDTO SelectWaterScheduleToDTO(RecipeWaterSchedule rws) => new RecipeWaterScheduleDTO
    {
        Id = rws.Id,
        Recipe = rws.Recipe != null ? new RecipeDTO
        {
            Id = rws.RecipeId,
            Name = rws.Recipe.Name ?? string.Empty
        } : new RecipeDTO { Id = rws.RecipeId, Name = string.Empty },
        WaterZoneSchedule = rws.WaterZoneSchedule != null ? new WaterZoneScheduleDTO
        {
            Id = rws.WaterZoneScheduleId,
            WaterZone = rws.WaterZoneSchedule.WaterZone != null ? new WaterZoneDTO
            {
                Id = rws.WaterZoneSchedule.WaterZone.Id,
                Name = rws.WaterZoneSchedule.WaterZone.Name ?? string.Empty
            } : new WaterZoneDTO(),
            StartTime = rws.WaterZoneSchedule.StartTime,
            EndTime = rws.WaterZoneSchedule.EndTime,
            CalculatedDWR = rws.WaterZoneSchedule.CalculatedDWR
        } : new WaterZoneScheduleDTO(),
        TargetDWR = rws.TargetDWR,
        AddedDateTime = rws.AddedDateTime,
        ModifiedDateTime = rws.ModifiedDateTime
    };

    public async Task<IEnumerable<RecipeWaterScheduleDTO>> GetAllRecipeWaterSchedulesAsync(Guid? farmId = null)
    {
        var schedules = farmId.HasValue && farmId.Value != Guid.Empty
            ? await _unitOfWork.RecipeWaterSchedule.GetAllAsync(x => x.Recipe.Product.FarmId == farmId)
            : await _unitOfWork.RecipeWaterSchedule.GetAllAsync();
        return schedules.OrderBy(rws => rws.AddedDateTime).Select(SelectWaterScheduleToDTO);
    }

    public async Task<RecipeWaterScheduleDTO?> GetRecipeWaterScheduleByIdAsync(Guid id)
    {
        var includes = new string[] { "Recipe", "WaterZoneSchedule", "WaterZoneSchedule.WaterZone" };
        var schedule = await _unitOfWork.RecipeWaterSchedule.GetByIdWithIncludesAsync(id, includes);
        if (schedule == null)
            return null;
        return SelectWaterScheduleToDTO(schedule);
    }

    public async Task<RecipeWaterScheduleDTO> CreateRecipeWaterScheduleAsync(RecipeWaterScheduleDTO scheduleDto)
    {
        var schedule = new RecipeWaterSchedule
        {
            Id = scheduleDto.Id == Guid.Empty ? Guid.NewGuid() : scheduleDto.Id,
            RecipeId = scheduleDto.Recipe.Id,
            WaterZoneScheduleId = scheduleDto.WaterZoneSchedule.Id,
            TargetDWR = scheduleDto.TargetDWR
        };

        await _unitOfWork.RecipeWaterSchedule.AddAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        return SelectWaterScheduleToDTO(schedule);
    }

    public async Task UpdateRecipeWaterScheduleAsync(RecipeWaterScheduleDTO scheduleDto)
    {
        var entity = await _unitOfWork.RecipeWaterSchedule.GetByIdAsync(scheduleDto.Id);
        if (entity == null)
            throw new Exception("RecipeWaterSchedule not found");

        entity.RecipeId = scheduleDto.Recipe.Id;
        entity.WaterZoneScheduleId = scheduleDto.WaterZoneSchedule.Id;
        entity.TargetDWR = scheduleDto.TargetDWR;
        entity.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.RecipeWaterSchedule.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteRecipeWaterScheduleAsync(Guid id)
    {
        var entity = await _unitOfWork.RecipeWaterSchedule.GetByIdAsync(id);
        if (entity == null)
            throw new Exception("RecipeWaterSchedule not found");

        entity.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.RecipeWaterSchedule.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}

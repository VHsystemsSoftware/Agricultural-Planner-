using VHS.Services.Growth.DTO;

namespace VHS.Services.Produce.DTO;

public class RecipeWaterScheduleDTO
{
    public Guid Id { get; set; }
    public RecipeDTO Recipe { get; set; }

    public WaterZoneScheduleDTO WaterZoneSchedule { get; set; }
    /// <summary>
    /// Target Daily Water Requirement (DWR) that the recipe requires.
    /// </summary>
    public double? TargetDWR { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
}

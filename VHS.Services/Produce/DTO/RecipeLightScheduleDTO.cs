using VHS.Services.Growth.DTO;

namespace VHS.Services.Produce.DTO;

public class RecipeLightScheduleDTO
{
    public Guid Id { get; set; }
    public RecipeDTO Recipe { get; set; }
    public LightZoneScheduleDTO LightZoneSchedule { get; set; }

    /// <summary>
    /// Target Daily Light Integral (DLI) that the recipe requires.
    /// </summary>
    public double? TargetDLI { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
}

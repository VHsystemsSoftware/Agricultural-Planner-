namespace VHS.Data.Core.Models;

public class RecipeWaterSchedule
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public virtual Recipe Recipe { get; set; }
    public Guid WaterZoneScheduleId { get; set; }
    public virtual WaterZoneSchedule WaterZoneSchedule { get; set; }

    /// <summary>
    /// Target Daily Water Requirement (DWR) for this recipe schedule
    /// </summary>
    public double? TargetDWR { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public RecipeWaterSchedule()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

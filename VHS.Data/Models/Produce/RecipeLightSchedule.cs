namespace VHS.Data.Core.Models;

public class RecipeLightSchedule
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public virtual Recipe Recipe { get; set; }
    public Guid LightZoneScheduleId { get; set; }
    public virtual LightZoneSchedule LightZoneSchedule { get; set; }

    /// <summary>
    /// Target Daily Light Integral (DLI) for this recipe schedule
    /// </summary>
    public double? TargetDLI { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public RecipeLightSchedule()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class WaterZone
{
    public Guid Id { get; set; }

    [Required]
    public Guid FarmId { get; set; }
    public virtual Farm Farm { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Target Daily Water Requirement (DWR) in L/m²/day
    /// </summary>
    public double? TargetDWR { get; set; }
    public virtual ICollection<WaterZoneSchedule> WaterZoneSchedules { get; set; } = new List<WaterZoneSchedule>();

    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public WaterZone()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

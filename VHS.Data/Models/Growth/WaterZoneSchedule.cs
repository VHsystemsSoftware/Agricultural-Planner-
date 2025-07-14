using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class WaterZoneSchedule
{
    public Guid Id { get; set; }

    [Required]
    public Guid WaterZoneId { get; set; }
    public virtual WaterZone WaterZone { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }
    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    public decimal Volume { get; set; }

    [Required]
    [MaxLength(20)]
    public string VolumeUnit { get; set; } = "Liter";

    /// <summary>
    /// Calculated Daily Water Requirement (DWR) based on volume and duration.
    /// </summary>
    public double? CalculatedDWR { get; set; }

    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public WaterZoneSchedule()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class LightZoneSchedule
{
    public Guid Id { get; set; }

    [Required]
    public Guid LightZoneId { get; set; }
    public virtual LightZone LightZone { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }
    [Required]
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Represents brightness as a percentage (0 - 100%).
    /// </summary>
    [Required]
    [Range(0, 100)]
    public decimal Intensity { get; set; }

    /// <summary>
    /// Calculated Daily Light Integral (DLI) based on intensity and duration.
    /// </summary>
    public double? CalculatedDLI { get; set; }

    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public LightZoneSchedule()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

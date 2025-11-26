using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Auth.Models.Auth;

public class UserSetting
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "en-US";

    [MaxLength(20)]
    public string PreferredTheme { get; set; } = "Light";
    [MaxLength(20)]
    public string PreferredMeasurementSystem { get; set; } = "Metric";
    [MaxLength(20)]
    public string PreferredWeightUnit { get; set; } = "Kilogram";
    [MaxLength(20)]
    public string PreferredLengthUnit { get; set; } = "Meter";
    [MaxLength(20)]
    public string PreferredTemperatureUnit { get; set; } = "Celsius";
    [MaxLength(20)]
    public string PreferredVolumeUnit { get; set; } = "Liter";
    [MaxLength(50)]
    public string PreferredDateTimeFormat { get; set; } = "dd-MM-yyyy HH:mm";

    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public UserSetting()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

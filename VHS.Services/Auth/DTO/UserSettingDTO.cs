using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VHS.Services.Auth.DTO
{
    public class UserSettingDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "en-US";
        public string PreferredTheme { get; set; } = "Light";
        public string PreferredMeasurementSystem { get; set; } = "Metric";
        public string PreferredWeightUnit { get; set; } = "Kilogram";
        public string PreferredLengthUnit { get; set; } = "Meter";
        public string PreferredTemperatureUnit { get; set; } = "Celsius";
        public string PreferredVolumeUnit { get; set; } = "Liter";
        public DateTime AddedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}

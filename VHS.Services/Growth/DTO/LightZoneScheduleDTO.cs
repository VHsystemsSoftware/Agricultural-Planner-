namespace VHS.Services.Growth.DTO;

public class LightZoneScheduleDTO
{
    public Guid Id { get; set; }

    public LightZoneDTO LightZone { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal Intensity { get; set; }
    public double? CalculatedDLI { get; set; }

}

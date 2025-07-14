namespace VHS.Services.Growth.DTO;

public class WaterZoneScheduleDTO
{
    public Guid Id { get; set; }
    public WaterZoneDTO WaterZone { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal Volume { get; set; }
    public double? CalculatedDWR { get; set; }
    public string VolumeUnit { get; set; } = "Liter";
}

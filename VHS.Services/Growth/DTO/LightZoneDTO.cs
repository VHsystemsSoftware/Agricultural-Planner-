namespace VHS.Services.Growth.DTO;

public class LightZoneDTO
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? TargetDLI { get; set; }

}

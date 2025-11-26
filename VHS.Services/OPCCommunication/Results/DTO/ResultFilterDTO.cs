namespace VHS.Services.Results.DTO;

public class ResultFilterDTO
{
     public string? TrayTag { get; set; }
    public DateOnly? SeedFrom { get; set; }
    public DateOnly? SeedTo { get; set; }
    public DateOnly? PlannedHarvestFrom { get; set; }
    public DateOnly? PlannedHarvestTo { get; set; }
	public DateOnly? RealHarvestFrom { get; set; }
	public DateOnly? RealHarvestTo { get; set; }
}

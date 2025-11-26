namespace VHS.Services.Results.DTO;

public class ResultItemDTO
{
	public string TrayTag { get; set; }
	public DateOnly SeedDate { get; set; }
	public DateOnly RealHarvestDate { get; set; }
	public DateOnly PlannedHarvestDate { get; set; }
	public float? WeightKg { get; set; } = 0;
	public float? WeightPounds => WeightKg.HasValue ? WeightKg.Value * 2.20462f : null;
	public string BatchName { get; set; }
	public string ProductName { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime? WeightDateTime { get; set; }
}

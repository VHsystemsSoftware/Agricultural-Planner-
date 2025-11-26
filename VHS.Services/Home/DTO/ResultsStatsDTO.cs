namespace VHS.Services.Home.DTO;

public class ResultsStatsDTO
{
    public float TotalWeightThisMonth { get; set; }
	public float TotalWeightThisYear { get; set; }
	public float TotalWeightToday { get; set; }
    public float AverageWeightPerTray { get; set; }
    public string WeightUnit { get; set; }

	public int SeederToday { get; set; }
	public int SeederThisMonth { get; set; }
	public int SeederThisYear { get; set; }

	public int WashedToday { get; set; }
	public int WashedThisMonth { get; set; }
	public int WashedThisYear { get; set; }

	public int HarvestedToday { get; set; }
	public int HarvestedThisMonth { get; set; }
	public int HarvestedThisYear { get; set; }

	public int PaternosterUpToday { get; set; }
	public int PaternosterUpThisMonth { get; set; }
	public int PaternosterUpThisYear { get; set; }

    public bool WeightMinusAlert { get; set; }
}


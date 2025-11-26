namespace VHS.Services.Home.DTO;

public class PlanningStatsDTO
{
    public double GerminationOccupancy { get; set; }
    public double PropagationOccupancy { get; set; }
    public double GrowingOccupancy { get; set; }
    public double PercentageFinishedToday { get; set; }
    public double PercentagePlannedPushOut { get; set; }
    public double Difference => PercentagePlannedPushOut - PercentageFinishedToday;
}

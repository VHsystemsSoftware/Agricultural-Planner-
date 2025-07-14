using VHS.Services.Produce.DTO;

namespace VHS.Services.Batches.DTO;

public class BatchPlanDTO
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public Guid FarmId { get; set; }
	public RecipeDTO? Recipe { get; set; } = new RecipeDTO();
    //public BatchDTO? Batch { get; set; }
    public DateTime? StartDate { get; set; }
	public int DaysForPlan { get; set; } = 1;
	public int TraysPerDay { get; set; } = 1;
	public DateTime? EndDate => StartDate?.AddDays(DaysForPlan - 1);
	public int TotalTrays => TraysPerDay * DaysForPlan;
	public Guid BatchPlanTypeId { get; set; }
	public Guid StatusId { get; set; } = GlobalConstants.BATCHPLANSTATUS_NEW;
	public DateTime AddedDateTime { get; set; }

	public virtual ICollection<BatchDTO> Batches { get; set; } = new List<BatchDTO>();
}

using System.ComponentModel.DataAnnotations.Schema;
using VHS.Services.Produce.DTO;

namespace VHS.Services.Batches.DTO;

public class GrowPlanDTO
{
	public Guid Id { get; set; }
    public Guid SetId { get; set; }
    public string Name { get; set; } = string.Empty;
	public Guid FarmId { get; set; }
	public RecipeDTO? Recipe { get; set; } = new RecipeDTO();
    public DateOnly? StartDate { get; set; }
	public int DaysForPlan { get; set; } = 1;
	public int TraysPerDay { get; set; } = 1;
	public DateOnly? EndDate => StartDate?.AddDays(DaysForPlan - 1);
	public int TotalTrays => TraysPerDay * DaysForPlan;
	public Guid GrowPlanTypeId { get; set; }
	public Guid StatusId { get; set; } = GlobalConstants.BATCHPLANSTATUS_NEW;
	public DateTime AddedDateTime { get; set; }
	
	public virtual ICollection<BatchDTO> Batches { get; set; } = new List<BatchDTO>();
}

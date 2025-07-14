using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class BatchPlan
{
	public Guid Id { get; set; }
	[Required]
	public string Name { get; set; } = string.Empty;

	[Required]
	public Guid FarmId { get; set; }
	public virtual Farm Farm { get; set; }

	public Guid? RecipeId { get; set; }
	public virtual Recipe? Recipe { get; set; }

	[Required]
	public int TraysPerDay { get; set; } = 1;

	public DateTime? StartDate { get; set; }            //unplanned plan, no batches, jobs and trays can be planned
	public int DaysForPlan { get; set; } = 1;

	public Guid BatchPlanTypeId { get; set; }           //recipe/racks/washing/transplant

	public int TotalTraysForBatch
	{
		get
		{
			return StartDate.HasValue ? (int)(DaysForPlan * TraysPerDay) : 0;
		}
	}
	[Required]
	public Guid StatusId { get; set; }

	public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

	public DateTime AddedDateTime { get; set; }
	public DateTime ModifiedDateTime { get; set; }
	public DateTime? DeletedDateTime { get; set; }

	public BatchPlan()
	{
		AddedDateTime = DateTime.UtcNow;
		ModifiedDateTime = DateTime.UtcNow;
	}
}

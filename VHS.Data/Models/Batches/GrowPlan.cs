using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;

public class GrowPlan
{
	public Guid Id { get; set; }

	public Guid SetId { get; set; } = Guid.NewGuid();

	[Required]
	public string Name { get; set; } = string.Empty;

	[Required]
	public Guid FarmId { get; set; }
	public virtual Farm Farm { get; set; }

	public Guid? RecipeId { get; set; }
	public virtual Recipe? Recipe { get; set; }

	[Required]
	public int TraysPerDay { get; set; } = 1;

	public DateOnly? StartDate { get; set; }            //unplanned plan, no batches, jobs and trays can be planned
	public int DaysForPlan { get; set; } = 1;

	public Guid GrowPlanTypeId { get; set; }           //recipe/racks/washing/transplant

	[NotMapped]
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

	public GrowPlan()
	{
		AddedDateTime = DateTime.UtcNow;
		ModifiedDateTime = DateTime.UtcNow;
	}
}

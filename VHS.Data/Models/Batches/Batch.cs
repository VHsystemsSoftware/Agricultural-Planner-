using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;
public class Batch
{
	public Guid Id { get; set; }
	[Required]
	public string BatchName { get; set; } = string.Empty;

	[Required]
	public Guid FarmId { get; set; }
	public virtual Farm Farm { get; set; }

	[Required]
	public Guid BatchPlanId { get; set; }
	public virtual BatchPlan BatchPlan { get; set; }

	[Required]
	public int TrayCount { get; set; } = 0;

	[Required]
	public DateTime ScheduledDateTime { get; set; }

	[Required]
	public DateTime? SeedDate { get; set; }
	public DateTime? HarvestDate { get; set; }
	public Guid StatusId { get; set; }

	public string? LotReference { get; set; }

	public virtual ICollection<Job> Jobs { get; set; }
    public virtual ICollection<BatchRow> BatchRows { get; set; } = new List<BatchRow>();

    public DateTime AddedDateTime { get; set; }
	public DateTime ModifiedDateTime { get; set; }
	public DateTime? DeletedDateTime { get; set; }

	public Batch()
	{
		AddedDateTime = DateTime.UtcNow;
		ModifiedDateTime = DateTime.UtcNow;
	}
}

using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class Job
{
	public Guid Id { get; set; }

	public Guid? BatchId { get; set; }      //a job can be made without a batch. in future?
	public virtual Batch Batch { get; set; }

	[Required]
	public int OrderOnDay { get; set; } = 1;

	[Required, MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	public int TrayCount { get; set; }

	public DateOnly ScheduledDate { get; set; }

	public Guid JobLocationTypeId { get; set; } //where does the job start? what location, seeding/harvesting/replanting/transport		

	public Guid StatusId { get; set; }

	public Guid JobTypeId { get; set; } = Guid.Empty;

	public bool Paused { get; set; } = false;

	public virtual ICollection<JobTray> JobTrays { get; set; }

	public DateTime AddedDateTime { get; set; }
	public DateTime ModifiedDateTime { get; set; }
	public DateTime? DeletedDateTime { get; set; }

	public Job()
	{
		AddedDateTime = DateTime.UtcNow;
		ModifiedDateTime = DateTime.UtcNow;
	}
}

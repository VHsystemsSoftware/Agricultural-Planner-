using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class JobTray
{
	public Guid Id { get; set; }

	[Required]
	public Guid JobId { get; set; }
	public virtual Job Job { get; set; }

	public Guid? TrayId { get; set; }               //trayid can be null if not scanned yet at the start of the job
	public virtual Tray Tray { get; set; }

	public Guid? ParentJobTrayId { get; set; }      //the jobtray that started this batch and this job (with seeding)
	public virtual JobTray? ParentJobTray { get; set; }

	public int OrderInJob { get; set; }

	[Required]
	public Guid DestinationLocation { get; set; }  

	public Guid? DestinationLayerId { get; set; }   //null because not all trays go to a layer, some go to a location

	public Guid? TransportLayerId { get; set; }		//only filled when this is the last layer of the batch


	public virtual Layer? DestinationLayer { get; set; }
	public virtual Recipe? Recipe { get; set; }

	public int? OrderOnLayer { get; set; }
	public Guid? RecipeId { get; set; }

	public DateTime AddedDateTime { get; set; }
}

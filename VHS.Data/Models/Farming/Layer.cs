using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;

public class Layer
{
    public Guid Id { get; set; }

    [Required]
    public Guid RackId { get; set; }
    public virtual Rack Rack { get; set; }

    [Required]
    public int Number { get; set; }        //0=top, 1=next, 2=next, etc.

    public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public bool Enabled { get; set; } = true;

	public virtual ICollection<TrayState> PreGrowTrayStates { get; set; } = new List<TrayState>();
	public virtual ICollection<TrayState> GrowTrayStates { get; set; } = new List<TrayState>();

	public Layer()
    {
        AddedDateTime = DateTime.UtcNow;
    }

	[NotMapped]
	public virtual ICollection<TrayState> TrayStates
	{
		get
		{
			return PreGrowTrayStates.Any() ? PreGrowTrayStates : GrowTrayStates;
		}
	}
	[NotMapped]
	public bool IsEmpty
	{
		get
		{
			return !TrayStates.Any(x => x.RecipeId.HasValue);
		}
	}

}

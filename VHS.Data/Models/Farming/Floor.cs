using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;

public class Floor
{
    public Guid Id { get; set; }

    [Required]
    public Guid FarmId { get; set; }
    public virtual Farm Farm { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Number { get; set; }

    public virtual ICollection<Rack> Racks { get; set; } = new List<Rack>();

    public bool Enabled { get; set; } = true;

    public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public Floor()
    {
        AddedDateTime = DateTime.UtcNow;
    }

	[NotMapped]
	public virtual ICollection<TrayState> TrayStates
	{
		get
		{
			return this.Racks.SelectMany(x => x.TrayStates).ToList();
		}
	}
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;  

public partial class Farm
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid FarmTypeId { get; set; }
    public virtual FarmType FarmType { get; set; }

    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

    public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public Farm()
    {
        AddedDateTime = DateTime.UtcNow;
    }

	[NotMapped]
	public virtual ICollection<Rack> Racks
    {
        get
        {
            return this.Floors.SelectMany(x=>x.Racks).ToList();
        }
    }

	[NotMapped]
	public virtual ICollection<TrayState> TrayStates
	{
		get
		{
			return this.Floors.SelectMany(x => x.TrayStates).ToList();
		}
	}
}

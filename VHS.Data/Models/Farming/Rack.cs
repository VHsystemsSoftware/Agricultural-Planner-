using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class Rack
{
	public Guid Id { get; set; }

	[Required]
	public Guid FloorId { get; set; }
	public virtual Floor Floor { get; set; }

	[Required]
	[MaxLength(50)]
	public string Name { get; set; } = string.Empty;

	public int Number { get; set; }

	[Required]
	public Guid TypeId { get; set; }        //grow/germination/progagation

	[Required]
	public int LayerCount { get; set; }

	[Required]
	public int TrayCountPerLayer { get; set; }

	public virtual ICollection<Layer> Layers { get; set; } = new List<Layer>();

	public bool Enabled { get; set; } = true;

	public DateTime AddedDateTime { get; set; }
	public DateTime? DeletedDateTime { get; set; }

	public Rack()
	{
		AddedDateTime = DateTime.UtcNow;
	}
}


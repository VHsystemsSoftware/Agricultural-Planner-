using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
	public virtual ICollection<BatchRow> BatchRows { get; set; } = new List<BatchRow>();

    public bool Enabled { get; set; } = true;

	public DateTime AddedDateTime { get; set; }
	public DateTime? DeletedDateTime { get; set; }

	public Rack()
	{
		AddedDateTime = DateTime.UtcNow;
	}

	public Rack(Guid rackType, int trayCount, int layerCount, int number)
	{
		this.TypeId = rackType;
		this.TrayCountPerLayer = trayCount;
		this.LayerCount = layerCount;
		this.Number = number;

		for (int i = 1; i <= this.LayerCount; i++)
		{
			this.Layers.Add(new Layer() { Id=Guid.NewGuid(), Rack = this, Number = i, IsTransportLayer = (i == this.LayerCount) });
		}
	}

	[NotMapped]
	public virtual Layer TransportLayer
	{
		get {
			return this.Layers.OrderBy(x => x.Number).Last();
		}
	}


	[NotMapped]
	public virtual ICollection<TrayState> TrayStates
	{
		get
		{
			return this.Layers.SelectMany(x=>x.TrayStates).Where(x=>x.RackId==this.Id).Select(x=>x).ToList();
		}
	}
}


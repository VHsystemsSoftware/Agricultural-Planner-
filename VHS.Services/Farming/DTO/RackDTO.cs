using System.ComponentModel.DataAnnotations.Schema;
using VHS.Services.Batches.DTO;

namespace VHS.Services.Farming.DTO;

public class RackDTO
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Number { get; set; }
	public Guid TypeId { get; set; }
	public int LayerCount { get; set; }
	public int TrayCountPerLayer { get; set; }

	public Guid FarmId { get; set; }
	public Guid FloorId { get; set; }
	public FloorDTO Floor { get; set; }
	public List<LayerDTO> Layers { get; set; } = new List<LayerDTO>();

	public List<BatchRowDTO> BatchRows { get; set; } = new List<BatchRowDTO>();

    public bool Enabled { get; set; }

	public string ProductType { get; set; }
	//public int OccupiedLayers => Layers.Count(layer => layer.HasRoom == false);
	public int TrayDepth { get; set; }

	public LayerDTO TransportLayer
	{
		get
		{
			return this.Layers.OrderBy(x => x.Number).Last();
		}
	}

	public RackDTO(Guid id, int layerCount)
	{
		this.Id = id;
		this.LayerCount = layerCount;

		for (int i = 1; i <= this.LayerCount; i++)
		{
			this.Layers.Add(new LayerDTO() { Id = Guid.NewGuid(), RackId = this.Id, TrayCountPerLayer = this.TrayCountPerLayer, Number = i, Trays = new List<TrayStateDTO>(), Enabled = true });
		}
	}

	public RackDTO()
	{
	
	}


}

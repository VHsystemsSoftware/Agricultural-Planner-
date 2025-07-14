namespace VHS.Services.Farming.DTO;

public class RackDTO
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Number { get; set; }
	public Guid TypeId { get; set; }
	public int LayerCount { get; set; }
	public int TrayCountPerLayer { get; set; }
	public FloorDTO Floor { get; set; }
	public List<LayerDTO> Layers { get; set; } = new List<LayerDTO>();

	public bool Enabled { get; set; }

	public string ProductType { get; set; }
	//public int OccupiedLayers => Layers.Count(layer => layer.HasRoom == false);
	public int TrayDepth { get; set; }

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

	//public bool HasRoom
	//{
	//    get
	//    {
	//        return Layers.Any(x => x.HasRoom);
	//    }
	//}

	//public void AddOccupiedDays(DateTime currentDateTime)
	//{
	//    foreach (var layer in Layers)
	//    {
	//        foreach (var tray in layer.Trays)
	//        {
	//            tray.AddOccupiedDay(currentDateTime);
	//        }
	//    }
	//}
}

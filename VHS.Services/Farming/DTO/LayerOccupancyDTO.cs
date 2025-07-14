namespace VHS.Services.Farming.DTO;

public class LayerOccupancyDTO
{
	public Guid LayerId { get; set; }
	public Guid RackId { get; set; }
	public Guid FloorId { get; set; }

	public string LayerName { get; set; } = string.Empty;
	public string RackName { get; set; } = string.Empty;
	public string FloorName { get; set; } = string.Empty;

	public Guid RackTypeId { get; set; }

	public int FloorNumber { get; set; }
	public int RackNumber { get; set; }
	public int LayerNumber { get; set; }

	public bool IsTransportLayer { get; set; }

	public List<TrayStateDTO> Trays { get; set; } = new List<TrayStateDTO>();

	public int Capacity { get; set; }


}

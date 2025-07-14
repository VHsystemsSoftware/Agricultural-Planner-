namespace VHS.Services.Batches.DTO;

public class BatchRowDTO
{
	public Guid Id { get; set; }
	public Guid BatchId { get; set; }
    public Guid? FloorId { get; set; }
	public Guid? RackId { get; set; }
	public Guid? LayerId { get; set; }

	public int TrayCount { get; set; }
	public DateTime AddedDateTime { get; set; }
	public int Number { get; set; }
	public bool IsTransportLayer { get; set; } = false;
    public Guid LayerRackTypeId { get; set; }

	public int EmptyCount { get; set; }

}

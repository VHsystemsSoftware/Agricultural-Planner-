namespace VHS.Data.Core.Models;
public class BatchRow
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public virtual Batch Batch { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? RackId { get; set; }
    public Guid? LayerId { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public virtual Layer? Layer { get; set; }
    public virtual Rack? Rack { get; set; }
    public virtual Floor? Floor { get; set; }

	public int EmptyCount { get; set; }

	public BatchRow()
    {
        AddedDateTime = DateTime.UtcNow;
    }
}

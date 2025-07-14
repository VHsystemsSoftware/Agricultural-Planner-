namespace VHS.OPC.Models.Message;

public class BaseResponse<T>
{
    public Guid Id => Guid.NewGuid();

	public Guid RequestId { get; set; }

	public Guid AuditId { get; set; } = Guid.Empty;

	public DateTime Date => DateTime.UtcNow;

    public int OPCserverId { get; set; }

	virtual public T Data { get; set; }
}

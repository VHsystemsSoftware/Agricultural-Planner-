using VHS.OPC.Models.Message.Request;

namespace VHS.OPC.Models.Message;

public class BaseRequest<T>
{
    public Guid Id => Guid.NewGuid();

    public DateTime Date => DateTime.UtcNow;

    public int OPCserverId { get; set; }

	public T Data { get; set; }

	public bool IsSimulation { get; set; } = false;

	public string Name { get => this.Type.ToString(); }

	public MessageType Type { get; set; }

	public BaseRequest(MessageType type)
	{
		Type = type;
	}
}

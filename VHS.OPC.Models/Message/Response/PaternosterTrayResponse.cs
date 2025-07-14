namespace VHS.OPC.Models.Message.Response;

public class PaternosterTrayResponse : BaseResponse<PaternosterTrayResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.PaternosterTrayResponse; }
}

public class PaternosterTrayResponseData
{
	public short Destination { get; set; }

    public uint TrayId { get; set; }
}
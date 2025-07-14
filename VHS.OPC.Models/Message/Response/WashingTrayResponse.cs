namespace VHS.OPC.Models.Message.Response;

public class WashingTrayResponse : BaseResponse<WashingTrayResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.WashingTrayResponse; }
}

public class WashingTrayResponseData
{
	public bool Accepted { get; set; }

    public uint TrayId { get; set; }
}
namespace VHS.OPC.Models.Message.Response;

public class GrowLineInputTrayResponse : BaseResponse<GrowLineInputTrayResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.GrowLineInputTrayResponse; }
}

public class GrowLineInputTrayResponseData
{
	public short Destination { get; set; }
    public short Layer { get; set; }
    public uint TrayId { get; set; }
	public uint TrayIdOutputTray { get; set; }
}
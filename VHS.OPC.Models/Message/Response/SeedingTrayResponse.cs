namespace VHS.OPC.Models.Message.Response;

public class SeedingTrayResponse : BaseResponse<SeedingTrayResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.SeedingTrayResponse; }
}

public class SeedingTrayResponseData
{
	public int DestinationCurrentTray { get; set; }
	public int DestinationOutputTray { get; set; }

	public int Layer { get; set; }
	public uint TrayIdOutputTray { get; set; }
	public uint TrayId { get; set; }

	public bool JobAvailable { get; set; }
}
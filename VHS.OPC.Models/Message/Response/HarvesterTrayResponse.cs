namespace VHS.OPC.Models.Message.Response;

public class HarvesterTrayResponse : BaseResponse<HarvesterTrayResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.HarvesterTrayResponse; }
}

public class HarvesterTrayResponseData
{
	public bool HarvestAllowed { get; set; }

    public uint TrayId { get; set; }
}
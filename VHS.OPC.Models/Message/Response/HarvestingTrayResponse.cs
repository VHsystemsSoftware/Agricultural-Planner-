using System.ComponentModel;

namespace VHS.OPC.Models.Message.Response;

public class HarvestingTrayResponse : BaseResponse<HarvestingTrayResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.HarvestingTrayResponse; }
}

public class HarvestingTrayResponseData
{	
	public short Destination { get; set; }
	public uint TrayId { get; set; }
}
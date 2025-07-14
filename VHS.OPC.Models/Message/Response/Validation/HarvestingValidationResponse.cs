namespace VHS.OPC.Models.Message.Response;

public class HarvestingValidationResponse : BaseResponse<HarvestingValidationResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get; set; }
}

public class HarvestingValidationResponseData
{
    public uint TrayId { get; set; }
}
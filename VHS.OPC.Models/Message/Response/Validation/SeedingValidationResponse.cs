namespace VHS.OPC.Models.Message.Response;

public class SeedingValidationResponse : BaseResponse<SeedingValidationResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get; set; }
}

public class SeedingValidationResponseData
{
	public VarTrayId TrayId { get; set; }
}
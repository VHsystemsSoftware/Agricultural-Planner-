namespace VHS.OPC.Models.Message.Response;

public class GenericResponse : BaseResponse<GenericResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.GenericResponse; }
}

public class GenericResponseData
{
	public string Value { get; set; } = string.Empty;
}
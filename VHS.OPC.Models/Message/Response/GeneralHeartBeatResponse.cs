namespace VHS.OPC.Models.Message.Response;
public class GeneralHeartBeatResponse : BaseResponse<GeneralHeartBeatResponseData>
{

	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.GeneralHeartBeatResponse;}
}

public class GeneralHeartBeatResponseData
{
	public bool Value { get; set; }
}

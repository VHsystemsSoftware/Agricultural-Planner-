namespace VHS.OPC.Models.Message.Request;

public class GeneralHeartBeatRequest : BaseRequest<GeneralHeartBeatRequestData>
{
	public GeneralHeartBeatRequest(MessageType type = MessageType.GeneralHeartBeatRequest) : base(type)
	{		
	}
}

public class GeneralHeartBeatRequestData
{
    public  VarHeartBeat HeartBeat { get; set; }
}
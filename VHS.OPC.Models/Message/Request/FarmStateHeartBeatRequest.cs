namespace VHS.OPC.Models.Message.Request;

public class FarmStateHeartBeatRequest : BaseRequest<FarmStateHeartBeatRequestData>
{
	public FarmStateHeartBeatRequest(MessageType type = MessageType.FarmStateHeartBeatRequest) : base(type)
	{		
	}
}

public class FarmStateHeartBeatRequestData
{
    public uint SeederWaitingTrayId { get; set; }
}
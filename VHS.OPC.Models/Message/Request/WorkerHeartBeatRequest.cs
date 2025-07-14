namespace VHS.OPC.Models.Message.Request;

public class WorkerHeartBeatRequest : BaseRequest<GeneralHeartBeatRequestData>
{
	public WorkerHeartBeatRequest(MessageType type = MessageType.WorkerHeartBeatRequest) : base(type)
	{		
	}
}

public class WorkerHeartBeatRequestData
{
    public  VarHeartBeat HeartBeat { get; set; }
}
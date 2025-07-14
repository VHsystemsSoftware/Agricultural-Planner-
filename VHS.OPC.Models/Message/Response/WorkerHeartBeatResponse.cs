namespace VHS.OPC.Models.Message.Response;

public class WorkerHeartBeatResponse : BaseResponse<WorkerHeartBeatResponseData>
{
	public string Name { get => this.Type.ToString(); }
	public MessageType Type { get => MessageType.WorkerHeartBeatResponse;}
}

public class WorkerHeartBeatResponseData
{
	public bool Value { get; set; }
}
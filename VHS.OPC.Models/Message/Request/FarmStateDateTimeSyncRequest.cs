namespace VHS.OPC.Models.Message.Request;

public class FarmStateDateTimeSyncRequest : BaseRequest<FarmStateDateTimeSyncRequestData>
{
	public FarmStateDateTimeSyncRequest(MessageType type = MessageType.FarmStateDateTimeSync) : base(type)
	{		
	}
}

public class FarmStateDateTimeSyncRequestData
{
    public TimeSpan TimeDifference { get; set; }

	public TimeSyncStatus Health { get; set; }
}
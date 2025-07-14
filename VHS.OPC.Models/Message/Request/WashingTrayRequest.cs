namespace VHS.OPC.Models.Message.Request;

public class WashingTrayRequest : BaseRequest<WashingTrayRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public WashingTrayRequest(MessageType type= MessageType.WashingTrayRequest) : base(type)
	{
	}
}

public class WashingTrayRequestData
{
    public VarRequestTrayInfo Request { get; set; }

    public VarTrayId TrayId { get; set; }
}
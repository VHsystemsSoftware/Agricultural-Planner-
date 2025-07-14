namespace VHS.OPC.Models.Message.Request;

public class PaternosterTrayRequest : BaseRequest<PaternosterTrayRequestData>
{
    public bool? IsPlannerInControl { get; set; }
    public PaternosterTrayRequest(MessageType type= MessageType.PaternosterTrayRequest) : base(type)
	{
	}
}

public class PaternosterTrayRequestData
{
    public VarRequestTrayInfo RequestTrayInfo { get; set; }

    public VarTrayId TrayId { get; set; }
}
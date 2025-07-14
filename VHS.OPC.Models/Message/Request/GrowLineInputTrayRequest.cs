namespace VHS.OPC.Models.Message.Request;

public class GrowLineInputTrayRequest : BaseRequest<GrowLineInputTrayRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public GrowLineInputTrayRequest(MessageType type = MessageType.GrowLineInputTrayRequest) : base(type)
    {
    }
}
public class GrowLineInputTrayRequestData
{
    public VarRequestTrayInfo RequestTrayInfo { get; set; }

    public VarTrayId TrayId { get; set; }
}
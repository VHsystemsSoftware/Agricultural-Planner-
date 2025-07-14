namespace VHS.OPC.Models.Message.Request;
public class HarvestingTrayRequest : BaseRequest<HarvestingTrayRequestData>
{
    public bool? IsPlannerInControl { get; set; }
    public HarvestingTrayRequest(MessageType type = MessageType.HarvestingTrayRequest) : base(type)
	{
	}
}

public class HarvestingTrayRequestData
{
    public VarRequestTrayInfo RequestTrayInfo { get; set; }

    public VarTrayId TrayId { get; set; }
}
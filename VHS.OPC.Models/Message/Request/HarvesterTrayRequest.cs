namespace VHS.OPC.Models.Message.Request;

public class HarvesterTrayRequest : BaseRequest<HarvesterTrayRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public HarvesterTrayRequest(MessageType type= MessageType.HarvesterTrayRequest) : base(type)
	{
	}
}

public class HarvesterTrayRequestData
{
    public VarRequestTrayInfo RequestHarvest { get; set; }

    public VarTrayId TrayId { get; set; }
}
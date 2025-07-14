namespace VHS.OPC.Models.Message.Request;

public class HarvesterTrayWeightRequest : BaseRequest<HarvesterTrayWeightRequestData>
{
    public bool? IsPlannerInControl { get; set; }
    public HarvesterTrayWeightRequest(MessageType type= MessageType.HarvesterTrayWeightRequest) : base(type)
	{
	}

}

public class HarvesterTrayWeightRequestData
{
    public VarTrayWeight Weight { get; set; }

    public VarTrayId TrayId { get; set; }
}
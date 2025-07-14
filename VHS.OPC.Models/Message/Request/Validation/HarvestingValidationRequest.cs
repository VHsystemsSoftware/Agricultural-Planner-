
namespace VHS.OPC.Models.Message.Request;

public class HarvestingValidationRequest : BaseRequest<HarvestingValidationRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public HarvestingValidationRequest(MessageType type) : base(type)
	{
	}
}

public class HarvestingValidationRequestData
{
    public VarTrayInfoAccepted TrayInfoAccepted { get; set; }

    public VarTrayId TrayId { get; set; }
}
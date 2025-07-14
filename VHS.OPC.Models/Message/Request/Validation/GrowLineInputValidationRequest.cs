
namespace VHS.OPC.Models.Message.Request;

public class GrowLineInputValidationRequest : BaseRequest<GrowLineInputValidationRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public GrowLineInputValidationRequest(MessageType type) : base(type)
	{
	}
}

public class GrowLineInputValidationRequestData
{
    public VarTrayInfoAccepted TrayInfoAccepted { get; set; }

    public VarTrayId TrayId { get; set; }
}
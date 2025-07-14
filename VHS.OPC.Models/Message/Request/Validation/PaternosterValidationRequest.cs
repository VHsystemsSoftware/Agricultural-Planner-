
namespace VHS.OPC.Models.Message.Request;

public class PaternosterValidationRequest : BaseRequest<PaternosterValidationRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public PaternosterValidationRequest(MessageType type) : base(type)
	{
	}
}

public class PaternosterValidationRequestData
{
    public VarTrayInfoAccepted TrayInfoAccepted { get; set; }

    public VarTrayId TrayId { get; set; }
}
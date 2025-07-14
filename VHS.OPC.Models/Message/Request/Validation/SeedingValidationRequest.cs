
namespace VHS.OPC.Models.Message.Request;

public class SeedingValidationRequest : BaseRequest<SeedingValidationRequestData>
{
    public bool? IsPlannerInControl { get; set; }

    public SeedingValidationRequest(MessageType type) : base(type)
	{
	}
}

public class SeedingValidationRequestData
{
    public VarTrayInfoAccepted TrayInfoAccepted { get; set; }

    public VarTrayId TrayId { get; set; }
}
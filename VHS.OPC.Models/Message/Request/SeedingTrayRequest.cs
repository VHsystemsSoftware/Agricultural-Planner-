namespace VHS.OPC.Models.Message.Request;

public class SeedingTrayRequest : BaseRequest<SeedingTrayRequestData>
{
    public bool? IsPlannerInControl { get; set; }
    public SeedingTrayRequest(MessageType type= MessageType.SeedingTrayRequest) : base(type)
	{
	}
}

public class SeedingTrayRequestData
{
    public VarRequestTrayInfo RequestTrayInfo { get; set; }

    public VarTrayId TrayId { get; set; }
}
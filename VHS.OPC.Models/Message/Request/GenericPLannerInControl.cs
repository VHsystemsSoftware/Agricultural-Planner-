
namespace VHS.OPC.Models.Message.Request;

public class GenericPlannerInControlRequest : BaseRequest<GenericPlannerInControlRequestData>
{
	public GenericPlannerInControlRequest(MessageType type) : base(type)
	{		
	}
}

public class GenericPlannerInControlRequestData
{
    public  VarPLannerInControl PLannerInControl { get; set; }
}
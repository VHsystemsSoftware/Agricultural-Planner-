
namespace VHS.OPC.Models.Message.Request;
public class GenericRequest : BaseRequest<GenericRequestData>
{
	public GenericRequest(MessageType type= MessageType.GenericRequest) : base(type)
	{
	}
}

public class GenericRequestData
{

}
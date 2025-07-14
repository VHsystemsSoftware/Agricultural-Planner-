namespace VHS.OPC.Models.Message.Request;

public class GeneralFireAlarmStatusRequest : BaseRequest<GeneralFireAlarmStatusRequestData>
{
	public GeneralFireAlarmStatusRequest(MessageType type = MessageType.FireAlarmStatusRequest) : base(type)
	{		
	}
}

public class GeneralFireAlarmStatusRequestData
{
    public VarFireAlarmIsActive FireAlarmIsActive { get; set; }
}
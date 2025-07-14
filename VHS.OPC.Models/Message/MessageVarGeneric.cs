namespace VHS.OPC.Models.Message;

public class VarBase<T>
{
    public required string Node { get; set; }
    public DateTime OPCDateTime { get; set; }
    public string? DataType { get; set; }
	public required T Value { get; set; }
}

public class VarTrayId : VarBase<uint> {}
public class VarRequestTrayInfo : VarBase<bool>{ }
public class VarHeartBeat : VarBase<bool> { }
public class VarFireAlarmIsActive : VarBase<bool> { }

public class VarTrayInfoAccepted : VarBase<bool>{}
public class VarTrayWeight : VarBase<float>{}
public class VarPLannerInControl : VarBase<bool> { }

public class VarTrayWaitingSeeder : VarBase<uint> { }

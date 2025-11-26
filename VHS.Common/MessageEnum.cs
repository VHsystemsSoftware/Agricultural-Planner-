namespace VHS.Common;

public enum MessageType
{
    None = 0,

    GeneralHeartBeatRequest = 120,
    GeneralHeartBeatResponse = 121,

    GeneralTimeSyncCheckRequest = 122,

    HeartBeatNotFound = 123,

    FireAlarmStatusRequest = 125,
    FireAlarmStatusResponse = 126,

    SeedingTrayRequest = 200,
    SeedingTrayResponse = 201,
    SeedingInstructionOk = 202,
    SeedingInstructionNotAllowed = 203,
    SeedingInstructionResponse = 204,
    SeedingPlannerInControl = 205,

    HarvesterTrayRequest = 240,
    HarvesterTrayResponse = 241,
    HarvesterTrayWeightRequest = 242,
    HarvesterTrayWeightResponse = 243,
    HarvesterPlannerInControl = 244,

    HarvestingTrayRequest = 250,
    HarvestingTrayResponse = 251,
    HarvestingInstructionOk = 252,
    HarvestingInstructionNotAllowed = 253,
    HarvestingInstructionResponse = 254,
    HarvestingPlannerInControl = 265,

    PaternosterTrayRequest = 270,
    PaternosterTrayResponse = 271,
    PaternosterInstructionOk = 272,
    PaternosterInstructionNotAllowed = 273,
    PaternosterInstructionResponse = 274,
    PaternosterPlannerInControl = 275,

    GrowLineInputTrayRequest = 280,
    GrowLineInputTrayResponse = 281,
    GrowLineInputInstructionOk = 282,
    GrowLineInputInstructionNotAllowed = 283,
    GrowLineInputInstructionResponse = 284,
    GrowLineInputPlannerInControl = 285,

    WashingTrayRequest = 290,
    WashingTrayResponse = 291,
    WashingPlannerInControl = 292,

    GenericRequest = 301,
    GenericResponse = 302,

    // General Alarms and Stops (400s)
    AlarmActiveFloor = 400,
    AlarmActiveGerminationArea = 401,
    AlarmActiveHarvestingStation = 402,
    AlarmActivePaternoster = 403,
    AlarmActiveSeedingStation = 404,
    AlarmActiveTransplantStation = 405,
    AlarmActiveTransportFromPater = 406,
    AlarmActiveTransportToPater = 407,
    AlarmActiveWashingStation = 408,
    AlarmActiveGrowLineInput = 409,
    AlarmActiveGrowLineOutput = 410,
    AlarmActiveRackArea1 = 411,
    AlarmActiveRackArea2 = 412,
    EmergencyStop = 413,
    ControlledStop = 414,
    ForcedStop = 415,

    //Specfic messages Worker to API
    WorkerHeartBeatRequest = 810,
    WorkerHeartBeatResponse = 811,
    FarmStateHeartBeatRequest = 820,
    FarmStateDateTimeSync = 821,

    ConnectionFailed = 999
}

public enum Component
{
    General = 0,
    Seeding = 1,
    Paternoster = 2,
    GrowLineInput = 3,
    Harvesting = 4,
    Harvester = 5,
    Washing = 6,
    TransplantInput = 7,
	TransplantOutput = 8
}

public enum TimeSyncStatus
{
    None,
    Perfect,
    Ok,
    Inaccurate,
    Poor,
    Bad
}
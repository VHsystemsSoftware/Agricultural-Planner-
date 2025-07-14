using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reactive;
using System.Text;
using VHS.Common;
using VHS.OPC.Models.Message;
using VHS.OPC.Models.Message.Request;
using VHS.OPC.Models.Message.Response;
using VHS.OPCUA.Models;
using Workstation.ServiceModel.Ua;

namespace VHS.OPCUA.Services;

/// <summary>
/// Handles monitored item notifications and writes responses back to the OPC-UA server.
/// </summary>
public class OpcUaVariableHandler
{
    public IConfiguration Configuration { get; }
    private readonly ILogger<OpcUaVariableHandler> _logger;

    // Node namespace prefix (update as needed or move to configuration)
    private string _apiBase = string.Empty;
    private string _apiToken = string.Empty;
    private bool _isSimulation = false;

    public OpcUaVariableHandler(IConfiguration configuration, ILogger<OpcUaVariableHandler> logger)
    {
        _logger = logger;
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _apiBase = $"{Configuration.GetValue<string>("ApiURL")}message";
        _apiToken = Configuration.GetValue<string>("ApiToken") ?? string.Empty;
        _isSimulation = Configuration.GetValue<bool>("OpcUa:IsSimulation", false);
    }

    /// <summary>
    /// Dispatches variable handling based on ClientHandle.
    /// </summary>
    public async Task HandleVariablesAsync(
        OpcUaServer server,
        MonitoredItemNotification min)
    {
        try
        {
            string compName = server.GetComponentVarName(min.ClientHandle);
            switch (compName)
            {
                case "General.FireAlarmActive":// 1000102: SK1 (all floors)
                    await HandleFireAlarmRequestedAsync(server, min);
                    break;

                case "General.HeartBeat": //1000103 -- SK1/SK2/SK3
                    await HandleHeartBeatRequestedAsync(server, min);
                    break;

                case "Harvester.RequestHarvest": // 1000202: SK1
                    await HandleHarvesterRequestedAsync(server, min);
                    break;

                case "Harvester.Weight_kg": // 1000204: SK1

                    await HandleHarvesterAcceptedAsync(server, min);
                    break;

                case "Harvester.PlannerInControl": // 1000205: SK1
                    await HandleHarvesterPlannerInControlAsync(server, min);
                    break;

                case "Harvesting.RequestTrayInfo": // 1000302: SK1
                    await HandleHarvestingRequestedAsync(server, min);
                    break;

                case "Harvesting.TrayInfoAccepted": // 1000304: SK1
                    await HandleHarvestingAcceptedAsync(server, min);
                    break;

                case "Harvesting.PlannerInControl":// 1000508: SK1
                    await HandleHarvestingPlannerInControlAsync(server, min);
                    break;

                case "Paternoster.RequestTrayInfo":// 1000402: SK1/SK2/SK3
                    await HandlePaternosterRequestedAsync(server, min);
                    break;

                case "Paternoster.TrayInfoAccepted": // 1000404: SK1/SK2/SK3
                    await HandlePaternosterAcceptedAsync(server, min);
                    break;

                case "Paternoster.PlannerInControl":// 1000508: SK1
                    await HandlePaternosterPlannerInControlAsync(server, min);
                    break;

                case "Seeding.RequestTrayInfo": // 1000506: SK1
                    await HandleSeedingRequestedAsync(server, min);
                    break;

                case "Seeding.TrayInfoAccepted":// 1000508: SK1
                    await HandleSeedingAcceptedAsync(server, min);
                    break;

                case "Seeding.PlannerInControl":// 1000508: SK1
                    await HandleSeedingPlannerInControlAsync(server, min);
                    break;

                case "Washing.Request": // 1000602: SK1
                    await HandleWashingRequestedAsync(server, min);
                    break;

                case "Washing.Accepted": // 1000601: SK1
                    await HandleWashingAcceptedAsync(server, min);
                    break;

                case "Washing.PlannerInControl": // 1000604: SK1
                    await HandleWashingPlannerInControlAsync(server, min);
                    break;

                case "GrowLineInput.RequestTrayInfo":// 2000105: SK2/SK3
                    await HandleGrowLineRequestedAsync(server, min);
                    break;

                case "GrowLineInput.TrayInfoAccepted": // 2000107: SK2/SK3
                    await HandleGrowLineAcceptedAsync(server, min);
                    break;

                case "GrowLineInput.PlannerInControl": // 2000107: SK2/SK3
                    await HandleGrowLinePlannerInControlAsync(server, min);
                    break;
                default:
                    _logger.LogInformation($"Cannot find {compName}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleVariablesAsync");
            SentrySdk.CaptureException(ex);
        }
    }

    private async Task HandleFireAlarmRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isFireAlarmActive = min.Value.GetValueOrDefault<bool>();

        _logger.LogInformation($"HandleFireAlarmRequested - Request fire alarm: {isFireAlarmActive}");

        MessageType messageType = isFireAlarmActive
            ? MessageType.FireAlarmStatusResponse
            : MessageType.FireAlarmStatusRequest;

        GeneralFireAlarmStatusRequest requestMessage = new()
        {
            IsSimulation = false, // Fire alarm is not simulated
            OPCserverId = server.ServerId,
            Data = new GeneralFireAlarmStatusRequestData
            {
                FireAlarmIsActive = new VarFireAlarmIsActive
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isFireAlarmActive,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };

        string response = await GetResponseFromAPI(requestMessage, 0);

        _logger.LogInformation($"Fire alarm response: {response}");
    }

    private async Task HandleHeartBeatRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();
        if (isRequest)
        {

            var read = await server.ReadNodeDataAsync(new List<string> { $"ns={server.NamespaceIndex};s=::ExtComm:CS.General.HeartBeat" });


            GeneralHeartBeatRequest requestMessage = new()
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                Data = new()
                {
                    HeartBeat = new VarHeartBeat
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isRequest,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime(),
                    }
                }
            };
            string response = await GetResponseFromAPI(requestMessage, 0);
        }
    }

    public async Task HandleHeartBeatResponseAsync(OpcUaServer server, bool heartBeatValue)
    {
        //Handling of API communication, 
        WorkerHeartBeatRequest requestMessage = new()
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,
            Data = new()
            {
                HeartBeat = new VarHeartBeat
                {
                    Node = "Worker.HeartBeat",
                    Value = heartBeatValue,
                    DataType = "Boolean",
                    OPCDateTime = DateTime.UtcNow
                }
            }
        };
        await GetResponseFromAPI(requestMessage, 0);

        try
        {
            string readNode = $"ns={server.NamespaceIndex};s=::ExtComm:STS.General.IsHeartBeatEnabled";

            if (server.NamespaceIndex > -1)
            {
                ReadResponse readResultInfeed = await server.ReadNodeDataAsync([readNode]);
                DataValue readItemIsHeartBeatEnabled = readResultInfeed.Results[0];
                bool isHeartBeatEnabled = Convert.ToBoolean(readItemIsHeartBeatEnabled.Variant.Value);

                if (isHeartBeatEnabled)
                {
                    List<OpcUaNodeWriteValue> listNodes =
                    [
                        new OpcUaNodeWriteValue{
                        NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.General.HeartBeat",
                        Value = heartBeatValue
                    },
                ];

                    var resultR = await server.WriteNodeDataAsync(listNodes);

                    CheckWriteResponse(resultR, listNodes);
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async Task HandleFarmStateHeartBeatResponseAsync(List<OpcUaServer> servers)
    {
        _logger.LogInformation("Farm state heartbeat");

        FarmStateHeartBeatRequest requestMessage = new()
        {
            IsSimulation = _isSimulation,
            OPCserverId = 0,
            Data = new()
            {
                SeederWaitingTrayId = 0
            }
        };

        var response = await GetResponseFromAPI(requestMessage, 0);

        if (DateTime.Now.Minute % 5 == 0 && DateTime.Now.Second <= 30)
        {
            foreach (OpcUaServer server in servers)
            {
                DateTime? opcDateTime = GetSystemDateTime(server).Result;
                TimeSyncStatus status = TimeSyncStatus.None;
                TimeSpan timeDifference = new();

                if (opcDateTime != null)
                {
                    status = CompareOpcServerTime((DateTime)opcDateTime, out timeDifference);
                }

                FarmStateDateTimeSyncRequest dateTimeSyncRequest = new()
                {
                    IsSimulation = _isSimulation,
                    OPCserverId = 0,
                    Data = new FarmStateDateTimeSyncRequestData()
                    {
                        Health = status,
                        TimeDifference = timeDifference
                    }
                };

                var dateTimeSyncResponse = await GetResponseFromAPI(dateTimeSyncRequest, 0);

                _logger.LogInformation($"FarmStateHeartBeatResponse - Check Time sync: {dateTimeSyncResponse}");
            }
        }
    }

    private async Task HandleGrowLineRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();

        if (isRequest == true)
        {
            _logger.LogInformation($"HandleGrowLineInputTrayInfoRequested - Request In feed {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.GrowLineInput);

            _logger.LogInformation($"HandleGrowLineInputTrayInfoRequested - Request TrayId {trayId}");

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.GrowLineInput).Result;

            GrowLineInputTrayRequest requestMessage = new()
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new GrowLineInputTrayRequestData()
                {
                    RequestTrayInfo = new VarRequestTrayInfo
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isRequest,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);

            GrowLineInputTrayResponse responseGrowLineInput = JsonConvert.DeserializeObject<GrowLineInputTrayResponse>(response);

            if (trayId == responseGrowLineInput.Data.TrayId)
            {
                Int16 rackId = responseGrowLineInput.Data.Destination;
                ushort layer = (ushort)responseGrowLineInput.Data.Layer;
                uint trayIdOuput = (uint)responseGrowLineInput.Data.TrayIdOutputTray;

                await SetGrowLineInputDestinationAsync(server, rackId, layer, trayIdOuput);

                _logger.LogInformation($"HandleGrowLineInputTrayInfoRequested - Write Destination: {rackId}, Layer: {layer}, TrayIdOutput: {trayIdOuput}");
            }
            else
            {
                //ToDo($"HandleGrowLineInputTrayInfoRequested - TrayId is not right!!");
            }
        }
    }

    private async Task HandleGrowLineAcceptedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isTrayInfoAccepted = min.Value.GetValueOrDefault<bool>();

        if (isTrayInfoAccepted == true)
        {
            _logger.LogInformation($"HandleGrowLineInputTrayInfoAccepted - Request In feed {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.GrowLineInput);

            _logger.LogInformation($"HandleGrowLineInputTrayInfoAccepted - Request TrayId {trayId}");

            MessageType type = isTrayInfoAccepted ? MessageType.GrowLineInputInstructionOk : MessageType.GrowLineInputInstructionNotAllowed;

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.GrowLineInput).Result;

            GrowLineInputValidationRequest requestMessage = new(type)
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new GrowLineInputValidationRequestData()
                {
                    TrayInfoAccepted = new VarTrayInfoAccepted
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isTrayInfoAccepted,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);

            GenericResponse responseSeeding = JsonConvert.DeserializeObject<GenericResponse>(response);
            if (isTrayInfoAccepted == true)
            {
                _logger.LogInformation($"HandleGrowLineInputTrayInfoAccepted - {trayId}");

            }
            else
            {
                _logger.LogInformation($"HandleGrowLineInputTrayInfoAccepted - trayId: {trayId}");
            }
        }
        else
        {
            //TODO: HandleGrowInputTrayInfo - ToDo: False value must be handled
        }

        //Reset destination to defaultValue
        if (isTrayInfoAccepted == true)
        {
            //Reset destination of the Grow line input to default value
            Int16 defaultDestination = 0;
            UInt16 defaultLayer = 0;
            ushort defaultTrayIdOutput = 0;

            await SetGrowLineInputDestinationAsync(server, defaultDestination, defaultLayer, defaultTrayIdOutput);

            _logger.LogInformation($"HandleGrowLineInputTrayInfoAccepted - Reset OPC to default destination: {defaultDestination}, default Layer {defaultLayer}");
        }
    }

    private async Task HandleGrowLinePlannerInControlAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isPlannerInControl = min.Value.GetValueOrDefault<bool>();

        GenericPlannerInControlRequest requestMessage = new(MessageType.GrowLineInputPlannerInControl)
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,
            Data = new GenericPlannerInControlRequestData()
            {
                PLannerInControl = new VarPLannerInControl
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isPlannerInControl,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };
        string response = await GetResponseFromAPI(requestMessage, 0);

        GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);
    }

    private async Task HandleHarvesterRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();
        if (isRequest)
        {
            _logger.LogInformation($"HandleHarvesterRequested - Request harvester: {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Harvester);

            _logger.LogInformation($"HandleHarvesterRequested - TrayId: {trayId}");

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Harvester).Result;

            HarvesterTrayRequest requestMessage = new()
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new HarvesterTrayRequestData()
                {
                    RequestHarvest = new VarRequestTrayInfo
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isRequest,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);
            HarvesterTrayResponse responseHarvester = JsonConvert.DeserializeObject<HarvesterTrayResponse>(response);

            if (trayId == responseHarvester.Data.TrayId)
            {
                await SetHarvesterAllowedAsync(server, responseHarvester.Data.HarvestAllowed);

                _logger.LogInformation($"HandleHarvesterRequested - Write HarvestAllowed: {responseHarvester.Data.HarvestAllowed}");
            }
        }
    }

    private async Task HandleHarvesterAcceptedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        if (min.Value != null && min.Value.Value is double doubleValue)
        {
            _logger.LogInformation($"HandleHarvesterAccepted - Request In feed {doubleValue}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Harvester);

            if (trayId != 0)
            {
                _logger.LogInformation($"HandleHarvesterAccepted - Request TrayId {trayId}");
                bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Harvester).Result;

                HarvesterTrayWeightRequest requestMessage = new()
                {
                    IsSimulation = _isSimulation,
                    OPCserverId = server.ServerId,
                    IsPlannerInControl = isPlannerInControl,
                    Data = new HarvesterTrayWeightRequestData()
                    {
                        Weight = new VarTrayWeight
                        {
                            Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                            Value = (float)doubleValue,
                            DataType = min.Value.Variant.Type.ToString(),
                            OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                        },
                        TrayId = new VarTrayId
                        {
                            Node = readNode,
                            Value = trayId,
                            DataType = readItemTrayId.Variant.Type.ToString(),
                            OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                        }
                    }
                };

                string response = await GetResponseFromAPI(requestMessage, trayId);

                GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);

                _logger.LogInformation($"HandleHarvesterAccepted - Updated batch grow planner {trayId}");
            }
            else
            {
                _logger.LogInformation($"HandleHarvesterAccepted - Unknow: {trayId}");
            }
        }

        bool defaultHarvestAllowed = false;

        await SetHarvesterAllowedAsync(server, defaultHarvestAllowed);

        _logger.LogInformation($"HandleHarvesterAccepted - Reset OPC to default harvestAllowed: {defaultHarvestAllowed}");
    }

    private async Task HandleHarvesterPlannerInControlAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isPlannerInControl = min.Value.GetValueOrDefault<bool>();

        GenericPlannerInControlRequest requestMessage = new(MessageType.HarvesterPlannerInControl)
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,
            Data = new GenericPlannerInControlRequestData()
            {
                PLannerInControl = new VarPLannerInControl
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isPlannerInControl,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };
        string response = await GetResponseFromAPI(requestMessage, 0);

        GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);
    }

    private async Task HandleHarvestingRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();

        if (isRequest == true)
        {
            _logger.LogInformation($"HandleHarvestingRequested - Request harvester: {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Harvesting);

            _logger.LogInformation($"HandleHarvestingRequested - Request TrayId {trayId}");

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Harvesting).Result;

            HarvestingTrayRequest requestMessage = new()
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new HarvestingTrayRequestData()
                {
                    RequestTrayInfo = new VarRequestTrayInfo
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isRequest,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };
            string response = await GetResponseFromAPI(requestMessage, trayId);

            HarvestingTrayResponse responseHarvesting = JsonConvert.DeserializeObject<HarvestingTrayResponse>(response);

            if (trayId == responseHarvesting.Data.TrayId)
            {
                Int16 destination = responseHarvesting.Data.Destination;

                await SetHarvestingDestinationAsync(server, destination);

                _logger.LogInformation($"HandleHarvestingRequested - Write Destination: {destination}");
            }
            else
            {
                _logger.LogInformation($"HandleHarvestingRequested - TrayId is not right!!");
                throw new Exception($"HandleHarvestingRequested - TrayId is not right!!");
            }
        }
    }

    private async Task HandleHarvestingAcceptedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isTrayInfoAccepted = min.Value.GetValueOrDefault<bool>();

        _logger.LogInformation($"HandleHarvestingAccepted - Request In feed {min.Value}");

        (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Harvesting);

        if (trayId != 0)
        {
            _logger.LogInformation($"HandleHarvestingAccepted - Request TrayId {trayId}");

            MessageType type = isTrayInfoAccepted ? MessageType.HarvestingInstructionOk : MessageType.HarvestingInstructionNotAllowed;

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Harvesting).Result;

            HarvestingValidationRequest requestMessage = new(type)
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new HarvestingValidationRequestData()
                {
                    TrayInfoAccepted = new VarTrayInfoAccepted
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isTrayInfoAccepted,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);

            HarvestingValidationResponse responseHarvesting = JsonConvert.DeserializeObject<HarvestingValidationResponse>(response);

            if (trayId == responseHarvesting.Data.TrayId)
            {
                if (isTrayInfoAccepted)
                {
                    _logger.LogInformation($"HandleHarvestingAccepted - Request TrayId {trayId}");
                    _logger.LogInformation($"HandleHarvestingAccepted - Updated batch grow planner {trayId}");
                }
                else
                {
                    //TODO: What to do when it is not accepted? Retry or throw an exception
                    _logger.LogInformation($"HandleHarvestingAccepted - TrayInfo is not ACCEPTED!!!?");
                }
            }
        }

        //Reset destination to defaultValue
        if (isTrayInfoAccepted == true)
        {
            Int16 defaultDestination = 0;

            await SetHarvestingDestinationAsync(server, defaultDestination);

            _logger.LogInformation($"HandleHarvestingAccepted - Reset OPC to default destination: {defaultDestination}");
        }
    }

    private async Task HandleHarvestingPlannerInControlAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isPlannerInControl = min.Value.GetValueOrDefault<bool>();

        GenericPlannerInControlRequest requestMessage = new(MessageType.HarvestingPlannerInControl)
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,

            Data = new GenericPlannerInControlRequestData()
            {
                PLannerInControl = new VarPLannerInControl
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isPlannerInControl,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };
        string response = await GetResponseFromAPI(requestMessage, 0);

        GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);
    }

    private async Task HandlePaternosterRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();

        if (isRequest == true)
        {
            _logger.LogInformation($"HandlePaternosterRequested - Request paternoster: {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Paternoster);

            _logger.LogInformation($"HandlePaternosterRequested - Request TrayId {trayId}");
            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Paternoster).Result;

            PaternosterTrayRequest requestMessage = new()
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new PaternosterTrayRequestData()
                {
                    RequestTrayInfo = new VarRequestTrayInfo
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isRequest,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);

            PaternosterTrayResponse responsePaternoster = JsonConvert.DeserializeObject<PaternosterTrayResponse>(response);

            if (trayId == responsePaternoster.Data.TrayId)
            {
                short destination = responsePaternoster.Data.Destination;

                await SetPaternosterDestinationAsync(server, destination);

                _logger.LogInformation($"HandlePaternosterRequested - Write Destination: {responsePaternoster.Data.Destination}");
            }
            else
            {
                //  throw new Exception($"HandlePaternosterRequested - TrayId is not right!!");
            }
        }
    }

    private async Task HandlePaternosterAcceptedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isTrayInfoAccepted = min.Value.GetValueOrDefault<bool>();

        _logger.LogInformation($"HandlePaternosterAccepted - Request paternoster {min.Value}");

        (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Paternoster);

        if (trayId != 0)
        {
            _logger.LogInformation($"HandlePaternosterAccepted - Request TrayId {trayId}");

            MessageType type = isTrayInfoAccepted ? MessageType.PaternosterInstructionOk : MessageType.PaternosterInstructionNotAllowed;

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.GrowLineInput).Result;

            PaternosterValidationRequest requestMessage = new(type)
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new PaternosterValidationRequestData()
                {
                    TrayInfoAccepted = new VarTrayInfoAccepted
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isTrayInfoAccepted,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);

            GenericResponse responsePaternoster = JsonConvert.DeserializeObject<GenericResponse>(response);

            if (isTrayInfoAccepted == true)
            {
                _logger.LogInformation($"HandlePaternosterAccepted - Updated batch grow planner {trayId}");

            }
            else
            {
                _logger.LogInformation($"HandlePaternosterAccepted - ToDo: False value must be handled, trayId: {trayId}");
            }
        }

        //Reset destination to defaultValue
        if (isTrayInfoAccepted == true)
        {
            Int16 defaultDestination = 0;

            await SetPaternosterDestinationAsync(server, defaultDestination);

            _logger.LogInformation($"HandlePaternosterRequested - Reset OPC to default destination: {defaultDestination}");
        }
    }

    private async Task HandlePaternosterPlannerInControlAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isPlannerInControl = min.Value.GetValueOrDefault<bool>();

        GenericPlannerInControlRequest requestMessage = new(MessageType.PaternosterPlannerInControl)
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,
            Data = new GenericPlannerInControlRequestData()
            {
                PLannerInControl = new VarPLannerInControl
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isPlannerInControl,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };
        string response = await GetResponseFromAPI(requestMessage, 0);

        GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);
    }

    private async Task HandleSeedingRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();

        if (isRequest == true)
        {
            _logger.LogInformation($"HandleSeedingInputTrayInfoRequested - Request In feed {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Seeding);
            if (trayId > 0)
            {
                _logger.LogInformation($"HandleSeedingInputTrayInfoRequested - Request TrayId {trayId}");

                bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Seeding).Result;

                SeedingTrayRequest requestMessage = new()
                {
                    IsSimulation = _isSimulation,
                    OPCserverId = server.ServerId,
                    IsPlannerInControl = isPlannerInControl,
                    Data = new SeedingTrayRequestData()
                    {
                        RequestTrayInfo = new VarRequestTrayInfo
                        {
                            Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                            Value = isRequest,
                            DataType = min.Value.Variant.Type.ToString(),
                            OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                        },
                        TrayId = new VarTrayId
                        {
                            Node = readNode,
                            Value = trayId,
                            DataType = readItemTrayId.Variant.Type.ToString(),
                            OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                        }
                    }
                };

                string response = await GetResponseFromAPI(requestMessage, trayId);

                SeedingTrayResponse responseSeeding = JsonConvert.DeserializeObject<SeedingTrayResponse>(response);

                if (responseSeeding.Data.JobAvailable == true)
                {
                    short destination = (short)responseSeeding.Data.DestinationCurrentTray;
                    ushort layer = (ushort)responseSeeding.Data.Layer;
                    uint outputTray = (uint)responseSeeding.Data.TrayIdOutputTray;
                    short destinationOutput = (short)responseSeeding.Data.DestinationOutputTray;

                    await SetSeedingDestinationAsync(server, destination, layer, outputTray, destinationOutput);

                    _logger.LogInformation($"HandleSeederInputTrayInfoRequested - Request Destination Id {destination}");
                }
                else
                {
                    _logger.LogInformation($"Retry HandleSeedingRequestedAsync - No job available for tray {trayId}");
                    //no job available, try again later
                    Thread.Sleep(30000);

                    await HandleSeedingRequestedAsync(server, min);
                }
            }
        }
    }

    private async Task HandleSeedingAcceptedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isTrayInfoAccepted = min.Value.GetValueOrDefault<bool>();

        if (isTrayInfoAccepted == true)
        {
            _logger.LogInformation($"HandleSeederInputTrayInfoAccepted - Request In feed {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Seeding);

            _logger.LogInformation($"HandleSeederInputTrayInfoAccepted - Request TrayId {trayId}");

            MessageType type = isTrayInfoAccepted ? MessageType.SeedingInstructionOk : MessageType.SeedingInstructionNotAllowed;

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Seeding).Result;

            SeedingValidationRequest requestMessage = new(type)
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new SeedingValidationRequestData()
                {
                    TrayInfoAccepted = new VarTrayInfoAccepted
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isTrayInfoAccepted,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };

            string response = await GetResponseFromAPI(requestMessage, trayId);

            GenericResponse responseSeeding = JsonConvert.DeserializeObject<GenericResponse>(response);

            _logger.LogInformation($"HandleSeederInputTrayInfoAccepted - Updated batch grow planner {trayId}");
        }
        else
        {
            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Seeding);

            if (trayId != 0)
            {
                _logger.LogInformation($"HandleSeederInputTrayInfoAccepted - ToDo: False value must be handled, trayId: {trayId}");
            }
        }

        //Reset destination to defaultValue
        if (isTrayInfoAccepted == true)
        {
            short defaultDestination = 0;
            ushort defaultLayer = 0;
            uint defaultOutputTrayId = 0;
            short defaultDestinationOutput = 0;

            await SetSeedingDestinationAsync(server, defaultDestination, defaultLayer, defaultOutputTrayId, defaultDestinationOutput);

            _logger.LogInformation($"HandleSeederInputTrayInfoAccepted - Reset OPC to default destination: {defaultDestination}, defaultLayer: {defaultLayer}, defaultOutputTrayId: {defaultOutputTrayId}, defaultDestinationOutput: {defaultDestinationOutput}");
        }
    }

    private async Task HandleSeedingPlannerInControlAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isPlannerInControl = min.Value.GetValueOrDefault<bool>();

        GenericPlannerInControlRequest requestMessage = new(MessageType.SeedingPlannerInControl)
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,
            Data = new GenericPlannerInControlRequestData()
            {
                PLannerInControl = new VarPLannerInControl
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isPlannerInControl,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };
        string response = await GetResponseFromAPI(requestMessage, 0);

        GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);
    }

    private async Task HandleWashingRequestedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();

        if (isRequest == true)
        {
            _logger.LogInformation($"HandleWasherRequested - Request washing: {min.Value}");

            (DataValue readItemTrayId, uint trayId, string readNode) = await GetTrayId(server, Component.Washing);

            _logger.LogInformation($"HandleWasherRequested - Request TrayId {trayId}");

            bool? isPlannerInControl = GetIsPlannerInControl(server, Component.Washing).Result;

            WashingTrayRequest requestMessage = new()
            {
                IsSimulation = _isSimulation,
                OPCserverId = server.ServerId,
                IsPlannerInControl = isPlannerInControl,
                Data = new WashingTrayRequestData()
                {
                    Request = new VarRequestTrayInfo
                    {
                        Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                        Value = isRequest,
                        DataType = min.Value.Variant.Type.ToString(),
                        OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                    },
                    TrayId = new VarTrayId
                    {
                        Node = readNode,
                        Value = trayId,
                        DataType = readItemTrayId.Variant.Type.ToString(),
                        OPCDateTime = readItemTrayId.SourceTimestamp.ToUniversalTime()
                    }
                }
            };
            string response = await GetResponseFromAPI(requestMessage, trayId);

            WashingTrayResponse responseWashing = JsonConvert.DeserializeObject<WashingTrayResponse>(response);

            if (trayId == responseWashing.Data.TrayId)
            {
                bool accepted = responseWashing.Data.Accepted;

                List<OpcUaNodeWriteValue> listNodes =
                [
                    new OpcUaNodeWriteValue
                        {
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Washing.Accepted",
                            Value = accepted
                        },
                    ];

                var resultWrite = await server.WriteNodeDataAsync(listNodes);

                CheckWriteResponse(resultWrite, listNodes);

                _logger.LogInformation($"HandleWashingRequested - Write Accepted: {accepted}");
            }
            else
            {
                throw new Exception($"HandleWashingRequested - TrayId is not right!!");
            }
        }
    }

    private async Task HandleWashingAcceptedAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isRequest = min.Value.GetValueOrDefault<bool>();

        if (isRequest)
        {
            //throw new NotImplementedException();
        }
    }

    private async Task HandleWashingPlannerInControlAsync(OpcUaServer server, MonitoredItemNotification min)
    {
        bool isPlannerInControl = min.Value.GetValueOrDefault<bool>();

        GenericPlannerInControlRequest requestMessage = new(MessageType.WashingPlannerInControl)
        {
            IsSimulation = _isSimulation,
            OPCserverId = server.ServerId,
            Data = new GenericPlannerInControlRequestData()
            {
                PLannerInControl = new VarPLannerInControl
                {
                    Node = server.GetVariableNodeName(min.ClientHandle.ToString()),
                    Value = isPlannerInControl,
                    DataType = min.Value.Variant.Type.ToString(),
                    OPCDateTime = min.Value.SourceTimestamp.ToUniversalTime()
                }
            }
        };
        string response = await GetResponseFromAPI(requestMessage, 0);

        GenericResponse responseGeneric = JsonConvert.DeserializeObject<GenericResponse>(response);
    }

    private static async Task<(DataValue readItemTrayId, uint trayId, string readNode)> GetTrayId(OpcUaServer server, Component component)
    {
        string readNode = $"ns={server.NamespaceIndex};s=::ExtComm:STS.{component}.TrayId";
        ReadResponse readResultInfeed = await server.ReadNodeDataAsync([readNode]);
        DataValue readItemTrayId = readResultInfeed.Results[0];
        uint trayId = readItemTrayId.GetValueOrDefault<UInt32>();
        return (readItemTrayId, trayId, readNode);
    }

    private static async Task<uint> GetWaitingTrayIdSeeder(OpcUaServer server)
    {
        string readNode = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Seeder.DestinationCurrentTray";
        ReadResponse readResultInfeed = await server.ReadNodeDataAsync([readNode]);
        DataValue readItemJobId = readResultInfeed.Results[0];

        if (readItemJobId.StatusCode == StatusCodes.Good && readItemJobId.Value != null)
        {
            int destination;
            if (Int32.TryParse(readItemJobId.Value.ToString(), out destination))
            {
                if (destination == 0)
                {
                    var data = await GetTrayId(server, Component.Seeding);
                    return data.trayId;
                }
            }
        }

        return 0;
    }

    private static async Task<bool?> GetIsPlannerInControl(OpcUaServer server, Component component)
    {
        string readNode = $"ns={server.NamespaceIndex};s=::ExtComm:STS.{component}.PlannerInControl";
        ReadResponse readResultInfeed = await server.ReadNodeDataAsync([readNode]);

        if (readResultInfeed != null)
        {
            DataValue readItemPlannerInControl = readResultInfeed.Results[0];

            if (readItemPlannerInControl.StatusCode == StatusCodes.Good && readItemPlannerInControl.Value != null)
            {
                if (bool.TryParse(readItemPlannerInControl.Value.ToString(), out bool isPlannerInControl))
                {
                    return isPlannerInControl;
                }
            }
        }

        return null;
    }

    private static async Task<DateTime?> GetSystemDateTime(OpcUaServer server)
    {
        string readNode = $"i=2258";
        ReadResponse readResultInfeed = await server.ReadNodeDataAsync([readNode]);

        if (readResultInfeed != null)
        {
            DataValue readItemSystemDateTime = readResultInfeed.Results[0];

            if (readItemSystemDateTime.StatusCode == StatusCodes.Good && readItemSystemDateTime.Value != null)
            {
                if (DateTime.TryParse(readItemSystemDateTime.Value.ToString(), out DateTime systemDateTime))
                {
                    return systemDateTime;
                }
            }
        }

        return null;
    }

    private async Task SetGrowLineInputDestinationAsync(OpcUaServer server, Int16 rackId, ushort layer, uint trayIdOutput)
    {
        List<OpcUaNodeWriteValue> listNodes =
        [
            new OpcUaNodeWriteValue
                        {
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.GrowLineInput.Destination",
                            Value = rackId
                        },
                        new OpcUaNodeWriteValue
                        {
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.GrowLineInput.Layer",
                            Value = layer
                        },
                        new OpcUaNodeWriteValue
                        {
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.GrowLineInput.TrayIdOutputTray",
                            Value = trayIdOutput
                        }
        ];

        var resultWrite = await server.WriteNodeDataAsync(listNodes);

        CheckWriteResponse(resultWrite, listNodes);
    }

    private async Task SetHarvesterAllowedAsync(OpcUaServer server, bool harvestedAllowed)
    {
        List<OpcUaNodeWriteValue> listNodes =
        [
            new OpcUaNodeWriteValue{
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Harvester.HarvestAllowed",
                            Value = harvestedAllowed
                        },
                    ];

        var resultWrite = await server.WriteNodeDataAsync(listNodes);

        CheckWriteResponse(resultWrite, listNodes);
    }

    private async Task SetHarvestingDestinationAsync(OpcUaServer server, short destination)
    {
        List<OpcUaNodeWriteValue> listNodes =
        [
            new OpcUaNodeWriteValue{
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Harvesting.Destination",
                            Value = destination
                        },
                    ];

        var resultWrite = await server.WriteNodeDataAsync(listNodes);

        CheckWriteResponse(resultWrite, listNodes);
    }

    private async Task SetPaternosterDestinationAsync(OpcUaServer server, short destination)
    {
        List<OpcUaNodeWriteValue> listNodes =
            [
                new OpcUaNodeWriteValue
                        {
                            NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Paternoster.Destination",
                            Value = destination
                        }
                ];

        var resultWrite = await server.WriteNodeDataAsync(listNodes);

        CheckWriteResponse(resultWrite, listNodes);
    }

    private async Task SetSeedingDestinationAsync(OpcUaServer server, short destination, ushort layer, uint outputTrayId, short destinationOutput)
    {
        List<OpcUaNodeWriteValue> listNodes =
        [
            new OpcUaNodeWriteValue{ NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Seeding.DestinationCurrentTray", Value = destination }
              , new OpcUaNodeWriteValue{ NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Seeding.Layer", Value = layer }
              , new OpcUaNodeWriteValue{ NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Seeding.TrayIdOutputTray", Value = outputTrayId }
              , new OpcUaNodeWriteValue{ NodeId = $"ns={server.NamespaceIndex};s=::ExtComm:CS.Seeding.DestinationOutputTray", Value = destinationOutput }
        ];

        var resultWrite = await server.WriteNodeDataAsync(listNodes);

        CheckWriteResponse(resultWrite, listNodes);
    }

    private async Task<string> GetResponseFromAPI<T>(BaseRequest<T> requestMessage, uint trayId)
    {
        string jsonRequest = JsonConvert.SerializeObject(requestMessage, Newtonsoft.Json.Formatting.Indented);

        int messageTpe = (int)requestMessage.Type;

        var response = await PostJsonAsync($"{_apiBase}/{messageTpe}/{trayId.ToString()}", jsonRequest);

        return response;
    }

    private async Task<string> PostJsonAsync(string url, string json)
    {
        SentrySdk.AddBreadcrumb($"{url}");
        SentrySdk.AddBreadcrumb($"{json}");
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpClient client = new();

        // in case of Postman an api token is supplied for the mock
        if (!string.IsNullOrEmpty(_apiToken))
        {
            client.DefaultRequestHeaders.Add("x-api-key", _apiToken);
        }

        try
        {
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return null;
        }
    }

    private void CheckWriteResponse(WriteResponse resultWrite, List<OpcUaNodeWriteValue> listNodes)
    {
        if (resultWrite == null || resultWrite.Results == null || resultWrite.Results.Length == 0)
        {
            _logger.LogInformation("Write response is empty or null.");
            return;
        }
        //TODO: Generic check for write results on OPC server
        for (int i = 0; i < resultWrite.Results.Length; i++)
        {
            // _logger.LogInformation($"Write to OPC {listNodes[i].NodeId}: {resultR.Results[i]}");

            if (StatusCode.IsGood(resultWrite.Results[i]))
            {
                //	_logger.LogInformation("Writing Succeeded");
            }
            else
            {
                _logger.LogInformation($"Writing Error, {resultWrite.Results[i]}");
            }
        }
    }

    public TimeSyncStatus CompareOpcServerTime(DateTime opcServerTime, out TimeSpan difference)
    {
        DateTime localTime = DateTime.UtcNow;
        difference = opcServerTime - localTime;
        double secondsDiff = Math.Abs(difference.TotalSeconds);

        if (secondsDiff <= 2)
            return TimeSyncStatus.Perfect;
        else if (secondsDiff <= 10)
            return TimeSyncStatus.Ok;
        else if (secondsDiff <= 60)
            return TimeSyncStatus.Inaccurate;
        else if (secondsDiff <= 300) // 5 minutes
            return TimeSyncStatus.Poor;
        else
            return TimeSyncStatus.Bad;
    }
}
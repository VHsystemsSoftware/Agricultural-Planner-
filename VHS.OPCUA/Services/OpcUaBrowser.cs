using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VHS.OPCUA.Models;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace VHS.OPCUA.Services;

/// <summary>
/// Encapsulates the logic for browsing an OPC-UA server and extracting component/variable nodes.
/// </summary>
public class OpcUaBrowser
{
    private readonly ILogger<OpcUaBrowser> _logger;
    private readonly List<OpcUaServerConfig> _configs;

    public OpcUaBrowser(
        ILogger<OpcUaBrowser> logger,
        IOptions<List<OpcUaServerConfig>> configs)
    {
        _logger = logger;
        _configs = configs.Value;
    }

	/// <summary>
	/// Performs a full browse of the given server if needed, and returns its populated components.
	/// </summary>
	public async Task<List<OpcUaComponentNode>> BrowseAsync(OpcUaServer server)
    {
        // Ensure we have a matching configuration entry
        var cfg = _configs.Any();
        if (!cfg)
        {
            throw new InvalidOperationException(
                $"No OPC UA server config found for {server.HostnameOrIP}:{server.PortNumber}");
        }

        await CheckToBrowseServerAsync(server);
        return server.Components;
    }

    #region "Browse OPC server logic"

    private async Task CheckToBrowseServerAsync(OpcUaServer server)
    {
        try
        {
            if (server.ClientSession != null && server.ClientSession.ChannelId > 0)
            {
                if (server.IsBrowseNeeded())
                {
                    await BrowseServerAsync(server);
                    //WriteJsonServerSettings(server, cfg);
                }
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }

    private async Task BrowseServerAsync(OpcUaServer server)
    {
        try
        {
            var session = server.ClientSession!;
            var components = new List<OpcUaComponentNode>();

            // Initial browse request from RootFolder
            var browseReq = new BrowseRequest
            {
                NodesToBrowse = new[] {
                    new BrowseDescription
                    {
                        NodeId = NodeId.Parse(ObjectIds.RootFolder),
                        BrowseDirection = BrowseDirection.Forward,
                        ReferenceTypeId = NodeId.Parse(ReferenceTypeIds.HierarchicalReferences),
                        NodeClassMask = (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                        IncludeSubtypes = true,
                        ResultMask = (uint)BrowseResultMask.All
                    }
                }
            };

            var browseResp = await session.BrowseAsync(browseReq);

            // Recursive browsing up to depth 3 for components
            foreach (var rd1 in browseResp.Results[0].References ?? Array.Empty<ReferenceDescription>())
            {
                LogNodeInformation(rd1, 1);
                (browseReq, browseResp) = await GetBrowseDescriptionAsync(session, rd1);
                foreach (var rd2 in browseResp.Results[0].References ?? Array.Empty<ReferenceDescription>())
                {
                    LogNodeInformation(rd2, 2);
                    (browseReq, browseResp) = await GetBrowseDescriptionAsync(session, rd2);
                    foreach (var rd3 in browseResp.Results[0].References ?? Array.Empty<ReferenceDescription>())
                    {
                        LogNodeInformation(rd3, 3);
                        (browseReq, browseResp) = await GetComponentsAsync(components, session, rd3, 3, server.ServerId);
                        (browseReq, browseResp) = await GetBrowseDescriptionAsync(session, rd3);
                    }
                }
            }

            server.LastBrowseDate = DateTime.Now;
            server.Components = components;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }

    private async Task<(BrowseRequest, BrowseResponse)> GetComponentsAsync(
        List<OpcUaComponentNode> components,
        ClientSessionChannel session,
        ReferenceDescription rdNode,
        int level,
        int serverId)
    {
        var browseReq = new BrowseRequest();
        var browseResp = new BrowseResponse();

        if (rdNode.NodeId != null)
        {
            var nodeIdStr = rdNode.NodeId.ToString()!;
            if (nodeIdStr.Contains("ExtComm"))
            {
                int compId = components.Count + 1;
                var component = new OpcUaComponentNode
                {
                    Name = rdNode.DisplayName.Text,
                    Node = nodeIdStr,
                    Id = compId
                };

                (browseReq, browseResp) = await GetBrowseDescriptionAsync(session, rdNode);
                var refs = browseResp.Results[0].References ?? Array.Empty<ReferenceDescription>();
                int varId = 1;
                foreach (var child in refs)
                {
                    LogNodeInformation(child, level + 1);
                    var sType = SignalType.None;
                    if (child.NodeId.ToString()!.Contains(":CS.")) sType = SignalType.ControlSignal;
                    if (child.NodeId.ToString()!.Contains(":STS.")) sType = SignalType.Status;
                    bool subscribe = child.DisplayName.Text.Contains("Request") || child.DisplayName.Text.Contains("Accepted");

                    var variable = new OpcUaVariableNodeItem(
                        child.DisplayName.Text,
                        child.NodeId.ToString()!,
                        sType,
                        subscribe,
                        serverId,
                        component.Id,
                        varId++);

                    component.Variables.Add(variable);
                }

                components.Add(component);
            }
        }

        return (browseReq, browseResp);
    }

    private async Task<(BrowseRequest, BrowseResponse)> GetBrowseDescriptionAsync(
        ClientSessionChannel session,
        ReferenceDescription rd)
    {
        var req = new BrowseRequest
        {
            NodesToBrowse = new[] {
                new BrowseDescription
                {
                    NodeId = ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris),
                    BrowseDirection = BrowseDirection.Forward,
                    ReferenceTypeId = NodeId.Parse(ReferenceTypeIds.HierarchicalReferences),
                    NodeClassMask = (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    IncludeSubtypes = true,
                    ResultMask = (uint)BrowseResultMask.All
                }
            }
        };
        var resp = await session.BrowseAsync(req);
        return (req, resp);
    }

    private void LogNodeInformation(ReferenceDescription rd, int level)
    {
        _logger.LogInformation("{Indent}[Level {Level}] {Display} ({NodeId})",
            new string(' ', level * 2), level, rd.DisplayName, rd.NodeId);
    }

    //private void WriteJsonServerSettings(OpcUaServer server, OpcUaServerConfig cfg)
    //{
    //    try
    //    {
    //        var path = string.Format(cfg.JsonSettingsPattern);
    //        File.WriteAllText(path, JsonConvert.SerializeObject(server.Components, Formatting.Indented));
    //        _logger.LogInformation("Wrote server settings to {Path}", path);
    //    }
    //    catch (Exception ex)
    //    {
    //        SentrySdk.CaptureException(ex);
    //    }
    //}

    #endregion
}

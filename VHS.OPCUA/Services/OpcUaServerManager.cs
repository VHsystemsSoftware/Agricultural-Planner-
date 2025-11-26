using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Opc.Ua.Client;
using System.Reactive.Linq;
using VHS.OPCUA.Configuration;
using VHS.OPCUA.Models;
using Workstation.ServiceModel.Ua;

namespace VHS.OPCUA.Services;

/// <summary>
/// Orchestrates loading, session-management, browsing, polling, subscriptions, and teardown
/// for one or more OPC-UA servers.
/// </summary>
public class OpcUaServerManager
{
	private readonly ILogger<OpcUaServerManager> _logger;
	private readonly List<OpcUaServerConfig> _configs;
	private readonly List<OpcUaServer> _servers = new();
	private readonly OpcUaBrowser _browser;
	private readonly OpcUaVariableHandler _variableHandler;
	private readonly OpcUaConfiguration _config;

	public OpcUaServerManager(
		ILogger<OpcUaServerManager> logger,
		OpcUaBrowser browser,
		OpcUaVariableHandler variableHandler,
		IOptions<List<OpcUaServerConfig>> configs,
		IOptions<OpcUaConfiguration> options)
	{
		_logger = logger;
		_configs = configs.Value;
		_browser = browser;
		_variableHandler = variableHandler;
		_config = options.Value;
	}

	/// <summary>Step 1: Load OPC-UA server objects from configuration.</summary>
	public async Task LoadServers()
	{
		foreach (var cfg in _configs)
		{
			var jsonServer = OpcUaFileHelper.ReadJsonServerSettings(_logger, cfg.JsonSettingsPattern);
			if (jsonServer != null)
			{
				_servers.Add(jsonServer);

				_logger.LogInformation("Loaded OPC UA server config: {Url}", jsonServer.DiscoveryUrl);
			}
			else
			{
				_logger.LogError($"Cannot load file with path {cfg.JsonSettingsPattern}");
			}
		}
	}

	public async Task BrowseServersAsync()
	{
		foreach (var server in _servers)
		{
			var models = await _browser.BrowseAsync(server);
		}
	}

	/// <summary>Step 2: Create and open client sessions.</summary>
	public async Task OpenClientSessionsAsync()
	{


		foreach (var server in _servers)
		{
			server.CreateSession("VHS.Client", "Planner", "TonkaMinds");
			if (server.ClientSession != null)
			{
				while (server.ClientSession.State != CommunicationState.Opened)
				{
					_logger.LogInformation("Waiting for session to open: {Url}", server.DiscoveryUrl);
					try
					{
						using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
						await server.ClientSession.OpenAsync(cts.Token);
					}
					catch (Exception)
					{
						server.RecreateSession();
						_logger.LogInformation("Session connect error on: {Url}", server.DiscoveryUrl);
						await Task.Delay(3000); // Wait until session is ready
					}
				}

				_logger.LogInformation("Session opened: {Url}", server.DiscoveryUrl);
			}
		}

	}



	/// <summary>Step 3: Browse each server’s namespace if needed.</summary>
	public async Task BrowseNamespacesAsync()
	{
		foreach (var server in _servers)
		{
			if (server.IsBrowseNeeded())
			{
				// TODO: implement browse logic
				_logger.LogInformation("Browsing namespace for: {Url}", server.DiscoveryUrl);



			}
		}
	}

	/// <summary>Step 4: Read all current node values.</summary>
	public async Task GetCurrentValuesAsync()
	{
		_logger.LogInformation("Get variables");
		foreach (var server in _servers)
		{
			_logger.LogInformation($"Read server {server.HostnameOrIP}");
			foreach (var comp in server.Components)
			{
				
				_logger.LogInformation($"Read component {comp.Name}");
				foreach (var variable in comp.Variables)
				{
					_logger.LogInformation("Try reading {Node}", variable.Node);

					var resp = await server.ReadNodeDataAsync(new List<string> { variable.Node });
					var val = resp.Results?.FirstOrDefault()?.Value;

					_logger.LogInformation("Read {Node} = {Value}", variable.Node, val);
				}
			}
		}
	}

	public async Task SubscribeVariableNodesAsync()
	{

		foreach (var server in _servers)
		{
			if (server.ClientSession != null && server.ClientSession.ChannelId > 0)
			{
				await server.CreateSubscriptionAsync();

				_logger.LogInformation($"SubscribeVariableNodes - Add items to the subscription. {server.HostnameOrIP}");

				List<OpcUaVariableNodeItem> variablesToSubscribe = server.GetVariablesToSubscribe().ToList();
				int subs = variablesToSubscribe.Count;
				int count = 0;

				var itemsToCreateNew = new Workstation.ServiceModel.Ua.MonitoredItemCreateRequest[subs];
				
				if (variablesToSubscribe.Count > 0)
				{
					foreach (OpcUaVariableNodeItem item in variablesToSubscribe)
					{
						itemsToCreateNew[count] = new Workstation.ServiceModel.Ua.MonitoredItemCreateRequest
						{
							ItemToMonitor = new Workstation.ServiceModel.Ua.ReadValueId
							{
								NodeId = Workstation.ServiceModel.Ua.NodeId.Parse(item.Node),
								AttributeId = AttributeIds.Value
							},
							MonitoringMode = Workstation.ServiceModel.Ua.MonitoringMode.Reporting,
							RequestedParameters = new Workstation.ServiceModel.Ua.MonitoringParameters
							{
								ClientHandle = (uint)item.HandleId,
								SamplingInterval = 10,
								QueueSize = 100,
								DiscardOldest = true
							}
						};

						count++;
					}

					var itemsRequest = new Workstation.ServiceModel.Ua.CreateMonitoredItemsRequest
					{
						SubscriptionId = server.SubscriptionId,
						ItemsToCreate = itemsToCreateNew,
						RequestHeader = new Workstation.ServiceModel.Ua.RequestHeader
						{
							Timestamp = DateTime.UtcNow,
							TimeoutHint = 10000, // 10 seconds timeout
							AuthenticationToken = server.ClientSession.AuthenticationToken
						}
					};

					var itemsResponse = await server.ClientSession.CreateMonitoredItemsAsync(itemsRequest);

					//Server status waarden :
					//Running				: 0x00000000
					//Failed                : 0x80000000
					//No Configuration      : 0x00000001
					//Shutdown              : 0x00000002
					//Test                  : 0x00000003
					//Communication Fault   : 0x800100001

					_logger.LogInformation($"SubscribeVariableNodes - Subscribe to PublishResponse stream. server: {server.HostnameOrIP}, id: {server.ServerId}");

					var session = server.ClientSession.Where(pr => pr.SubscriptionId == server.SubscriptionId);

					var token = session.Subscribe(onNext: async pr =>
					{
						//_logger.LogInformation(DateTime.UtcNow.ToString());
						// loop thru all the data change notifications
						var dcns = pr.NotificationMessage.NotificationData.OfType<Workstation.ServiceModel.Ua.DataChangeNotification>();
						foreach (var dcn in dcns)
						{

							var monitoreditemnames = string.Join(",", dcn.MonitoredItems.Select(x => x.ClientHandle));

							//_logger.LogInformation($"MonitoredItems found: {dcn.MonitoredItems.Length}");
							foreach (var min in dcn.MonitoredItems)
							{
								OpcUaVariableNodeItem varItem;
								OpcUaComponentNode comItem;
								(comItem, varItem) = server.GetVariableByHandleId((int)min.ClientHandle);

								var componentName = server.GetComponentVarName(min.ClientHandle);

								if (componentName != "General.HeartBeat")
								{
									_logger.LogInformation($"MonitoredItem: {min.ClientHandle} - {componentName} with value: {min.Value.Value}, total items:  {dcn.MonitoredItems.Length}={monitoreditemnames}");
									_logger.LogInformation($"SubscribeVariable: {pr.SubscriptionId}; {min.ClientHandle}; {comItem.Name}; {varItem.Name}; {min.Value}");
								}

								try
								{
									try
									{
										var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
										var jsonMonitoredItem = JsonConvert.SerializeObject(min);
										var folder = _config.LogFolder;

										if (componentName != "General.HeartBeat")
										{
											await File.WriteAllTextAsync($"{folder}/{server.HostnameOrIP}_{timestamp}_{min.ClientHandle}_{componentName.Replace(".", "_")}.json", jsonMonitoredItem);
										}
									}
									catch (Exception)
									{


									}
									await _variableHandler.HandleVariablesAsync(server, min);
								}
								catch (Exception ex)
								{
									SentrySdk.CaptureException(ex);
								}
							}
						}
					},  onError: async error =>
					{
						_logger.LogError($"Error in subscription stream for {server.DiscoveryUrl}: {error.Message}");

						await OpenClientSessionsAsync();
						await SubscribeVariableNodesAsync();
					});

					server.Token = token;
				}
			}
		}
	}

	/// <summary>Step 5: Subscribe to variables and handle notifications.</summary>
	// public async Task SubscribeVariableNodesAsync()
	// {
	//     foreach (var server in _servers)
	//     {
	//         await server.CreateSubscriptionAsync();
	//         var toSub = server.GetVariablesToSubscribe();

	//         foreach (var variable in toSub)
	//         {
	//	var monitoredItem = new MonitoredItem(server.SubscriptionId)
	//	{
	//		DisplayName = "MonitoredVariable",
	//		StartNodeId = variable.Node,
	//		AttributeId = Attributes.Value,
	//		SamplingInterval = 1000,
	//	};

	//	// Define what happens on value change
	//	monitoredItem.Notification += OnVariableChanged;
	//}			

	//_logger.LogInformation("Subscribed {Count} items on {Url}", toSub.Count, server.DiscoveryUrl);
	//     }
	// }

	private void OnVariableChanged(MonitoredItem item, MonitoredItemNotificationEventArgs e)
	{
		foreach (var value in item.DequeueValues())
		{
			_logger.LogInformation($"[{DateTime.Now}] Value changed: {value.Value} (Status: {value.StatusCode})");
		}
	}

	/// <summary>Step 6: Delete subscriptions.</summary>
	public async Task DeleteSubscriptionsAsync()
	{
		foreach (var server in _servers)
		{
			var ok = await server.DeleteSubscriptionAsync();
			_logger.LogInformation("Deleted subscription for {Url}: {Result}", server.DiscoveryUrl, ok);
		}
	}

	/// <summary>Step 7: Read server status info (state, time, etc.).</summary>
	public async Task ReadServerInfoAsync()
	{
		foreach (var server in _servers)
		{
			var resp = await server.ReadNodeDataAsync(new List<string> { Workstation.ServiceModel.Ua.VariableIds.Server_ServerStatus.ToString() });
			var status = resp.Results?[0].GetValueOrDefault<Workstation.ServiceModel.Ua.ServerStatusDataType>();
			if (status != null)
			{
				_logger.LogInformation("Server {Url} status: {State}, time {Time}", server.DiscoveryUrl, status.State, status.CurrentTime);
			}
		}
	}

	/// <summary>Step 8: Close sessions.</summary>
	public async Task CloseSessionsAsync()
	{
		foreach (var server in _servers)
		{
			if (server.ClientSession != null && server.ClientSession.Completion.Status == TaskStatus.Running)
			{
				await server.ClientSession.CloseAsync();
				_logger.LogInformation("Closed session {Url}", server.DiscoveryUrl);
			}
		}
	}

	public async Task BackgroundProcessHeartBeatAsync()
	{
		bool heartBeat = true;

		while (true)
		{
			foreach (var server in _servers)
			{
				await _variableHandler.HandleHeartBeatResponseAsync(server, heartBeat);

				Thread.Sleep(5000);

				heartBeat = !heartBeat;
			}
		}
	}

	public async Task BackgroundFarmStateHeartBeatAsync()
	{
		while (true)
		{
			_logger.LogInformation("BackgroundFarmStateHeartBeatAsync - Heartbeat for farm state started.");

			if (_servers != null && _servers.Count > 0)
			{
				await _variableHandler.HandleFarmStateHeartBeatResponseAsync(_servers);
			}

			Thread.Sleep(30000);
		}
	}
}
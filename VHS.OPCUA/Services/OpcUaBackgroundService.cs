using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VHS.OPCUA.Configuration;

namespace VHS.OPCUA.Services;

public class OpcUaBackgroundService : BackgroundService
{
    private readonly OpcUaServerManager _manager;
    private readonly ILogger<OpcUaBackgroundService> _logger;
    private readonly OpcUaConfiguration _config;


    public OpcUaBackgroundService(
        ILogger<OpcUaBackgroundService> logger,
        OpcUaServerManager manager,
        IOptions<OpcUaConfiguration> options)
    {
        _logger = logger;
        _manager = manager;
        _config = options.Value;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Prepare log folder
            Directory.CreateDirectory(_config.LogFolder);
            _logger.LogInformation($"OPC-UA service starting. Logs folder: {_config.LogFolder}, Simulation: {_config.IsSimulation}");


			// Step 1: Initialize servers from configuration
			await _manager.LoadServers();

			Thread backgroundThreadHeartBeat = new Thread(async () =>
			{
				await _manager.BackgroundProcessHeartBeatAsync();
			});
			backgroundThreadHeartBeat.IsBackground = true;
			backgroundThreadHeartBeat.Start();

			Thread backgroundThreadFarmState = new Thread(async () =>
			{
				await _manager.BackgroundFarmStateHeartBeatAsync();
			});
			backgroundThreadFarmState.IsBackground = true;
			backgroundThreadFarmState.Start();



			// Step 2: Open client sessions for all servers
			await _manager.OpenClientSessionsAsync();  

            // Step 3: Browse namespaces where needed
            //await _manager.BrowseNamespacesAsync();

            //await _manager.BrowseServersAsync();

            // Step 4: Read current values of all configured nodes
            await _manager.GetCurrentValuesAsync();

            // Step 5: Subscribe to variable nodes and handle notifications
            await _manager.SubscribeVariableNodesAsync();

            _logger.LogInformation("OPC-UA orchestrator is running. Press Ctrl+C to shut down.");

            // Keep running until cancellation
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException) { /* Graceful shutdown */ }

            _logger.LogInformation("OPC-UA service stopping. Cleaning up...");

            // Step 6: Delete subscriptions
            await _manager.DeleteSubscriptionsAsync();

            // Step 7: Read server status information
            await _manager.ReadServerInfoAsync();

            // Step 8: Close all client sessions
            await _manager.CloseSessionsAsync();

            _logger.LogInformation("OPC-UA background service stopped.");

        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            _logger.LogError(ex, "Error in OpcUaBackgroundService");
        }
    }
}

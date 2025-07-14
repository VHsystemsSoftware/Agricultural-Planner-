using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VHS.OPCUA.Configuration;
using VHS.OPCUA.Models;
using VHS.OPCUA.Services;

namespace VHS.OPCUA;

public static class OpcUaServiceRegistrar
{
    public static IServiceCollection AddOpcUa(this IServiceCollection services, IConfiguration config)
    {
				// 1) Bind configuration sections
		services.Configure<List<OpcUaServerConfig>>(config.GetSection("OpcUaServers"));
        services.Configure<OpcUaConfiguration>(config.GetSection("OpcUa"));

		// 2) Register the browser for deep namespace discovery
		services.AddSingleton<OpcUaBrowser>();
		services.AddSingleton<OpcUaVariableHandler>();
		// 3) Register the main orchestrator
		services.AddSingleton<OpcUaServerManager>();

        // 4) Register the hosted background service
        services.AddHostedService<OpcUaBackgroundService>();



        return services;
    }
}

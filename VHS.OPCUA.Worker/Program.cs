using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Configuration;
using VHS.OPCUA;

var builder = Host.CreateDefaultBuilder(args);

builder.UseWindowsService(options => options.ServiceName = "VHS_OPCUA_Worker");

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
	config.AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true);
	config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true);
});

builder.UseSerilog((context, services, logConfiguration) =>
{
	var loggingDirectory = context.Configuration.GetValue<string>("LoggingDirectory");
	var sentryEnvironment = context.Configuration.GetValue<string>("Sentry:Environment");
	var sentryDns = context.Configuration.GetValue<string>("Sentry:Dsn");
	var sentryDebug = context.Configuration.GetValue<bool>("Sentry:Debug");

	Environment.SetEnvironmentVariable("LoggingDirectory", loggingDirectory);
	Environment.SetEnvironmentVariable("EnvironmentName", context.HostingEnvironment.EnvironmentName);

	logConfiguration.ReadFrom.Configuration(context.Configuration)
					.ReadFrom.Services(services)
					.WriteTo.Sentry(options =>
					{
						options.Dsn = sentryDns;
						options.Environment = sentryEnvironment;
						options.Debug = sentryDebug;
						options.SendDefaultPii = true;
						options.AttachStacktrace = true;
					});
});

builder.ConfigureServices((context, services) =>
{
	services.AddOpcUa(context.Configuration);
	services.AddHttpClient();

	var apiBase = context.Configuration.GetValue<string>("ApiURL");
    var hubUrl = $"{apiBase?.TrimEnd('/')}/hubs/notifications";

	services.AddSingleton<HubConnection>(sp =>
                new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect()
                    .Build());

	services.AddLogging();
	services.AddSignalR(options =>
	{
		options.KeepAliveInterval = TimeSpan.FromMinutes(1);
	});
	services.AddMemoryCache();

});

var app = builder.Build();
await app.RunAsync();
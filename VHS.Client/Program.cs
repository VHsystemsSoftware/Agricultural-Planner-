using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor.Services;
using MudExtensions.Services;
using System.Globalization;
using VHS.Client.Services.Auth;

namespace VHS.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddLocalization();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddMudServices();
        builder.Services.AddMudExtensions();

		builder.Logging.SetMinimumLevel(LogLevel.Error);
		//builder.Logging.AddFilter((category, level) =>
		//{
		//	return level >= LogLevel.Warning || !(category?.Contains("System.Net.Http.HttpClient") == true);
		//});
		//builder.Logging.AddFilter("Microsoft.AspNetCore.Components.RenderTree.*", LogLevel.None);

		var apiBaseAddress = new Uri(builder.Configuration["ApiURL"]);

		builder.Services.AddTransient<AuthClientMessageService>();
        builder.Services.AddScoped<AuthClientStateService>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthClientStateService>());
        builder.Services.AddAuthorizationCore();

        builder.Services.AddHttpClient<AuthClientService>(client =>
        {
            client.BaseAddress = apiBaseAddress;
            client.Timeout = TimeSpan.FromMinutes(30);
        })
        .AddHttpMessageHandler<AuthClientMessageService>();

		builder.Services.AddSingleton(sp =>
		{
			var navigationManager = sp.GetRequiredService<NavigationManager>();
			return new HubConnectionBuilder()
				.WithUrl(navigationManager.ToAbsoluteUri($"{apiBaseAddress}hubs/notifications"))
				.WithAutomaticReconnect()
				.Build();
		});


		ClientServiceInitialization.Initialize(builder.Services, apiBaseAddress);

		CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
		CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

		var host = builder.Build();
        await host.RunAsync();
    }
}

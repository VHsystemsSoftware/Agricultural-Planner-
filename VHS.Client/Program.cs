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
using System.Text.Json;
using VHS.Client.Services.Auth;
using VHS.Client.Services;
using Radzen;
using VHS.Services.Auth.DTO;

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
        builder.Services.AddRadzenComponents();

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
            client.Timeout = TimeSpan.FromMinutes(5);
        })
        .AddHttpMessageHandler<AuthClientMessageService>();

		builder.Services.AddSingleton(sp =>
		{
			var navigationManager = sp.GetRequiredService<NavigationManager>();
			return new HubConnectionBuilder()
				.WithUrl(navigationManager.ToAbsoluteUri($"{apiBaseAddress}hubs/notifications"))
                 //.ConfigureLogging(logging =>
                 //{
                 //    logging.AddDebug();
                 //    logging.SetMinimumLevel(LogLevel.Trace);
                 //})
                .WithAutomaticReconnect()
				.Build();
		});


		ClientServiceInitialization.Initialize(builder.Services, apiBaseAddress);


		
		
		var host = builder.Build();
        await SetCultureAsync(host.Services);
        await host.RunAsync();
    }

    private static async Task SetCultureAsync(IServiceProvider services)
    {
        var localStorage = services.GetRequiredService<ILocalStorageService>();
        string language = "en-US";

        try
        {
            var settingsJson = await localStorage.GetItemAsStringAsync("VHS_USER_SETTINGS");
            if (!string.IsNullOrWhiteSpace(settingsJson))
            {
                UserSettingDTO settings = null;
                try
                {
                    settings = JsonSerializer.Deserialize<UserSettingDTO>(settingsJson);
                }
                catch (JsonException)
                {
                    var cleanJson = JsonSerializer.Deserialize<string>(settingsJson);
                    if (cleanJson != null)
                    {
                        settings = JsonSerializer.Deserialize<UserSettingDTO>(cleanJson);
                    }
                }

                if (settings != null && !string.IsNullOrWhiteSpace(settings.PreferredLanguage))
                {
                    language = settings.PreferredLanguage;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading culture from storage: {ex.Message}. Defaulting to en-US.");
        }

        var culture = new CultureInfo(language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}

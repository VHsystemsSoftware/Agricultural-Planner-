using VHS.Client.Services;
using VHS.Client.Services.Auth;
using VHS.Client.Services.Farming;
using VHS.Client.Services.Produce;
using VHS.Client.Services.Growth;
using VHS.Client.Services.Batches;
using VHS.Client.Common;
using VHS.Client.Services.Audit;
using VHS.Client.Services.Results;
using VHS.Client.Services.Home;
using VHS.Client.Services.Notes;

namespace VHS.Client
{
    public static class ClientServiceInitialization
    {
        public static void Initialize(IServiceCollection services, Uri apiBaseAddress)
        {
            // General
            services.AddScoped<LocalStorage>();
            services.AddScoped<PageTitleService>();
            services.AddScoped<UserPreferencesService>();
            services.AddSingleton<FireAlarmStateService>();

			services.AddSingleton<PermissionService>();

			// Home
			services.AddHttpClient<HomeClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

            // User
            services.AddHttpClient<IAuthorizationClientService, AuthorizationClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<AuthorizationClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<RoleClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<UserClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<UserSettingClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

            // Farming
            services.AddHttpClient<FarmClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<FloorClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<RackClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<LayerClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<TrayClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
			services.AddHttpClient<TrayStateClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

			// Produce
			services.AddHttpClient<ProductClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<RecipeClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<RecipeLightScheduleClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<RecipeWaterScheduleClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

            // Growth
            services.AddHttpClient<LightZoneClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<LightZoneScheduleClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<WaterZoneClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<WaterZoneScheduleClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

            // Batches
            services.AddHttpClient<BatchClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<BatchPlanClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<JobClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();
            services.AddHttpClient<GrowPlanClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();            

            // Results
            services.AddHttpClient<ResultClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

            // Notes
            services.AddHttpClient<NoteClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

            // OPC
            services.AddHttpClient<OPCAuditClientService>(client => client.BaseAddress = apiBaseAddress).AddHttpMessageHandler<AuthClientMessageService>();

			// Admin
			services.AddHttpClient<SystemService>(client => client.BaseAddress = apiBaseAddress);
		}
    }
}

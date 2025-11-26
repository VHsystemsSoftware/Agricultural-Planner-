using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VHS.Services.Audit;
using VHS.Services.Farming.Algorithm;
using VHS.Services.Home;
using VHS.Services.Notes;
using VHS.Services.Results;
using VHS.Services.SystemMessages;
using VHS.Services.Notes;
using VHS.Services.Auth;


//using VHS.Services.Farming.Algorithm;

namespace VHS.Services;

public static class ServiceInitialization
{
	private static readonly int COMMAND_TIMEOUT = (int)TimeSpan.FromMinutes(30).TotalSeconds;

	public static void ConfigureDatabaseProvider(IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<VHSCoreDBContext>(options =>
			options
				.UseLazyLoadingProxies()
				.UseSqlServer(configuration.GetConnectionString("VHSCoreConnection"), sqlServerOptions =>
			{
				sqlServerOptions.CommandTimeout(COMMAND_TIMEOUT);
				sqlServerOptions.MigrationsAssembly(typeof(VHSCoreDBContext).Assembly.GetName().Name);
				sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
			})
		);

		services.AddDbContext<VHSAuthDBContext>(options =>
			options
				.UseLazyLoadingProxies()
				.UseSqlServer(configuration.GetConnectionString("VHSAuthConnection"), sqlServerOptions =>
				{
					sqlServerOptions.CommandTimeout(COMMAND_TIMEOUT);
					sqlServerOptions.MigrationsAssembly(typeof(VHSAuthDBContext).Assembly.GetName().Name);
					sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
				})
		);

		services.AddDbContext<VHSAuditDBContext>(options =>
			options
				.UseLazyLoadingProxies()
				.UseSqlServer(configuration.GetConnectionString("VHSAuditConnection"), sqlServerOptions =>
				{
					sqlServerOptions.CommandTimeout(COMMAND_TIMEOUT);
					sqlServerOptions.MigrationsAssembly(typeof(VHSAuditDBContext).Assembly.GetName().Name);
					sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
				})
		);
	}

	public static void Initialize(IServiceCollection services, IConfiguration configuration)
	{
		// Register repositories
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IUserSettingRepository, UserSettingRepository>();
		services.AddScoped<IFarmRepository, FarmRepository>();
		services.AddScoped<IFarmTypeRepository, FarmTypeRepository>();
		services.AddScoped<IFloorRepository, FloorRepository>();
		services.AddScoped<IRackRepository, RackRepository>();
		services.AddScoped<ILayerRepository, LayerRepository>();
		services.AddScoped<ITrayRepository, TrayRepository>();
		//services.AddScoped<ITrayCurrentStateRepository, TrayCurrentStateRepository>();
		services.AddScoped<ITrayStateRepository, TrayStateRepository>();
		services.AddScoped<IProductRepository, ProductRepository>();
		services.AddScoped<IRecipeRepository, RecipeRepository>();
		services.AddScoped<IRecipeLightScheduleRepository, RecipeLightScheduleRepository>();
		services.AddScoped<IRecipeWaterScheduleRepository, RecipeWaterScheduleRepository>();
		services.AddScoped<ILightZoneRepository, LightZoneRepository>();
		services.AddScoped<ILightZoneScheduleRepository, LightZoneScheduleRepository>();
		services.AddScoped<IWaterZoneRepository, WaterZoneRepository>();
		services.AddScoped<IWaterZoneScheduleRepository, WaterZoneScheduleRepository>();
		services.AddScoped<IBatchRepository, BatchRepository>();
		services.AddScoped<IGrowPlanRepository, GrowPlanRepository>();
		services.AddScoped<IBatchRowRepository, BatchRowRepository>();
		services.AddScoped<IJobRepository, JobRepository>();
		services.AddScoped<IJobTrayRepository, JobTrayRepository>();
		services.AddScoped<ISystemMessageRepository, SystemMessageRepository>();
		services.AddScoped<INoteRepository, NoteRepository>();

		// Home
		services.AddScoped<IHomeService, HomeService>();

		// Audit
		services.AddScoped<IAuditLogRepository, AuditLogRepository>();

		//OPC
		services.AddScoped<IOPCAuditRepository, OPCAuditRepository>();

		// Register unit of work
		services.AddScoped<IUnitOfWorkCore, UnitOfWorkCore>();
		services.AddScoped<IUnitOfWorkAudit, UnitOfWorkAudit>();
		services.AddScoped<IUnitOfWorkAuth, UnitOfWorkAuth>();

		// Register services
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
		services.AddScoped<IUserSettingService, UserSettingService>();
		services.AddScoped<IFarmService, FarmService>();
		services.AddScoped<IFloorService, FloorService>();
		services.AddScoped<IRackService, RackService>();
		services.AddScoped<ILayerService, LayerService>();
		services.AddScoped<ITrayService, TrayService>();
		services.AddScoped<IProductService, ProductService>();
		services.AddScoped<IRecipeService, RecipeService>();
		services.AddScoped<IRecipeLightScheduleService, RecipeLightScheduleService>();
		services.AddScoped<IRecipeWaterScheduleService, RecipeWaterScheduleService>();
		services.AddScoped<ILightZoneService, LightZoneService>();
		services.AddScoped<ILightZoneScheduleService, LightZoneScheduleService>();
		services.AddScoped<IWaterZoneService, WaterZoneService>();
		services.AddScoped<IWaterZoneScheduleService, WaterZoneScheduleService>();
		services.AddScoped<IBatchService, BatchService>();
		services.AddScoped<IGrowPlanService, GrowPlanService>();
		services.AddScoped<IBatchRowService, BatchRowService>();
		services.AddScoped<IJobService, JobService>();
		services.AddScoped<IJobTrayService, JobTrayService>();
		services.AddScoped<ITrayStateService, TrayStateService>();

		// Results
		services.AddScoped<IResultService, ResultService>();

		// Audit
		services.AddScoped<IAuditLogService, AuditLogService>();

		// System
		services.AddScoped<ISystemMessageService, SystemMessageService>();

		// Notes
		services.AddScoped<INoteService, NoteService>();

		//OPC
		services.AddScoped<IOPCAuditService, OPCAuditService>();
		services.AddScoped<IOPCCommuncationService, OPCCommuncationService>();

		// Farm allocation planner
		services.AddScoped<IGrowPlanAlgoritmeService, GrowPlanAlgoritmeService>();
        //services.AddScoped<BestFitTrayAllocator>();
        //services.AddScoped<FarmTrayAllocator>();
        //services.AddScoped<RackTypeTrayAllocator>();
        //services.AddScoped<IFarmPlannerService, FarmPlannerService>();

        services.AddLogging();
	}
}

using Microsoft.EntityFrameworkCore.Storage;

namespace VHS.Data.Core.Infrastructure
{
    public interface IUnitOfWorkCore : IDisposable
    {
        IFarmRepository Farm { get; }
        IFarmTypeRepository FarmType { get; }
        IFloorRepository Floor { get; }
        IRackRepository Rack { get; }
        ILayerRepository Layer { get; }
        ITrayRepository Tray { get; }
        //ITrayCurrentStateRepository TrayCurrentState { get; }
        ITrayStateRepository TrayState { get; }
		ITrayStateAuditRepository TrayStateAudit { get; }

		IProductRepository Product { get; }
        IRecipeRepository Recipe { get; }
        ILightZoneRepository LightZone { get; }
        ILightZoneScheduleRepository LightZoneSchedule { get; }
        IWaterZoneRepository WaterZone { get; }
        IWaterZoneScheduleRepository WaterZoneSchedule { get; }
        IRecipeLightScheduleRepository RecipeLightSchedule { get; }
        IRecipeWaterScheduleRepository RecipeWaterSchedule { get; }

        IBatchRepository Batch { get; }
        IBatchPlanRepository BatchPlan { get; }
        IBatchRowRepository BatchRow { get; }
        IJobRepository Job { get; }
        IJobTrayRepository JobTray { get; }

		Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<IDbContextTransaction> EnsureTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

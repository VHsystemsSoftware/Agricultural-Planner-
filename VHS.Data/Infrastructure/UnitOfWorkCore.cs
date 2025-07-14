using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace VHS.Data.Core.Infrastructure
{
    public class UnitOfWorkCore : IUnitOfWorkCore, IDisposable
    {
        private readonly VHSCoreDBContext _contextCore;
		private readonly ILogger<UnitOfWorkCore> _logger;
        private IDbContextTransaction? _transaction;

        public IFarmRepository Farm { get; }
        public IFarmTypeRepository FarmType { get; }
        public IFloorRepository Floor { get; }
        public IRackRepository Rack { get; }
        public ILayerRepository Layer { get; }
        public ITrayRepository Tray { get; }
        public ITrayStateAuditRepository TrayStateAudit { get; }
		//public ITrayCurrentStateRepository TrayCurrentState { get; }
		public ITrayStateRepository TrayState { get; }
		public IProductRepository Product { get; }
        public IRecipeRepository Recipe { get; }
        public ILightZoneRepository LightZone { get; }
        public ILightZoneScheduleRepository LightZoneSchedule { get; }
        public IWaterZoneRepository WaterZone { get; }
        public IWaterZoneScheduleRepository WaterZoneSchedule { get; }
        public IRecipeLightScheduleRepository RecipeLightSchedule { get; }
        public IRecipeWaterScheduleRepository RecipeWaterSchedule { get; }
        public IBatchRepository Batch { get; }
        public IBatchPlanRepository BatchPlan { get; }
        public IBatchRowRepository BatchRow { get; }
        public IJobRepository Job { get; }
        public IJobTrayRepository JobTray { get; }

		public UnitOfWorkCore(
            VHSCoreDBContext contextCore,
			ILogger<UnitOfWorkCore> logger,
            IFarmRepository farmRepository,
            IFarmTypeRepository farmTypeRepository,
            IFloorRepository floorRepository,
            IRackRepository rackRepository,
            ILayerRepository layerRepository,
            ITrayRepository trayRepository,
            //ITrayCurrentStateRepository trayCurrentStateRepository,
            ITrayStateRepository trayStateRepository,
			IProductRepository productRepository,
            IRecipeRepository recipeRepository,
            ILightZoneRepository lightZoneRepository,
            ILightZoneScheduleRepository lightZoneScheduleRepository,
            IWaterZoneRepository waterZoneRepository,
            IWaterZoneScheduleRepository waterZoneScheduleRepository,
            IRecipeLightScheduleRepository recipeLightScheduleRepository,
            IRecipeWaterScheduleRepository recipeWaterScheduleRepository,
            IBatchRepository batchRepository,
            IBatchPlanRepository batchPlanRepository,
            IBatchRowRepository batchRowRepository,
            IJobRepository jobRepository,
            IJobTrayRepository jobTrayRepository,
            ITrayStateAuditRepository trayStateAuditRepository
			)
        {
            _contextCore = contextCore;
			_logger = logger;
            Farm = farmRepository;
            FarmType = farmTypeRepository;
            Floor = floorRepository;
            Rack = rackRepository;
            Layer = layerRepository;
            Tray = trayRepository;
            //TrayCurrentState = trayCurrentStateRepository;
            TrayState = trayStateRepository;
			Product = productRepository;
            Recipe = recipeRepository;
            LightZone = lightZoneRepository;
            LightZoneSchedule = lightZoneScheduleRepository;
            WaterZone = waterZoneRepository;
            WaterZoneSchedule = waterZoneScheduleRepository;
            RecipeLightSchedule = recipeLightScheduleRepository;
            RecipeWaterSchedule = recipeWaterScheduleRepository;
            Batch = batchRepository;
            BatchPlan = batchPlanRepository;
            BatchRow = batchRowRepository;
            Job = jobRepository;
            JobTray = jobTrayRepository;
            TrayStateAudit = trayStateAuditRepository;
		}

        public async Task<int> SaveChangesAsync() => await _contextCore.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _logger.LogInformation("Starting transaction...");
                _transaction = await _contextCore.Database.BeginTransactionAsync();
            }
            return _transaction;
        }

        public async Task<IDbContextTransaction> EnsureTransactionAsync()
        {
            return _transaction ?? await BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction commit failed: {ex.Message}");
                    throw;
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync();
                    _logger.LogWarning("Transaction rolled back.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction rollback failed: {ex.Message}");
                    throw;
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
			_contextCore.Dispose();
        }
    }
}

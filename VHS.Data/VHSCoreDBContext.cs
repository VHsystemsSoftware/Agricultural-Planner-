using Microsoft.EntityFrameworkCore;


namespace VHS.Data.Core
{
    public class VHSCoreDBContext : DbContext
    {
        private static readonly int COMMAND_TIMEOUT = (int)TimeSpan.FromMinutes(60).TotalSeconds;

        public VHSCoreDBContext(DbContextOptions<VHSCoreDBContext> options) : base(options)
        {
            Database.SetCommandTimeout(COMMAND_TIMEOUT);
        }

        // Farming
        public DbSet<Farm> Farms { get; set; }
        public DbSet<FarmType> FarmTypes { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Rack> Racks { get; set; }
        public DbSet<Layer> Layers { get; set; }
        public DbSet<Tray> Trays { get; set; }
		public DbSet<TrayState> TrayStates { get; set; }
        public DbSet<TrayStateAudit> TrayStateAudits { get; set; }

		// Produce
		public DbSet<Product> Products { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeLightSchedule> RecipeLightSchedules { get; set; }
        public DbSet<RecipeWaterSchedule> RecipeWaterSchedules { get; set; }

        // Growth
        public DbSet<LightZone> LightZones { get; set; }
        public DbSet<LightZoneSchedule> LightZoneSchedules { get; set; }
        public DbSet<WaterZone> WaterZones { get; set; }
        public DbSet<WaterZoneSchedule> WaterZoneSchedules { get; set; }

        // Batches
        public DbSet<Batch> Batches { get; set; }
        public DbSet<BatchPlan> BatchPlans { get; set; }
        public DbSet<BatchRow> BatchRows { get; set; }
        public DbSet<Job> Jobs { get; set; }
		public DbSet<JobTray> JobTrays { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Farming
            modelBuilder.ApplyConfiguration(new FarmMap());
            modelBuilder.ApplyConfiguration(new FarmTypeMap());
            modelBuilder.ApplyConfiguration(new FloorMap());
            modelBuilder.ApplyConfiguration(new RackMap());
            modelBuilder.ApplyConfiguration(new LayerMap());
            modelBuilder.ApplyConfiguration(new TrayMap());
			modelBuilder.ApplyConfiguration(new TrayStateMap());
			modelBuilder.ApplyConfiguration(new TrayStateAuditMap());

			// Produce
			modelBuilder.ApplyConfiguration(new ProductMap());
            modelBuilder.ApplyConfiguration(new RecipeMap());
            modelBuilder.ApplyConfiguration(new RecipeLightScheduleMap());
            modelBuilder.ApplyConfiguration(new RecipeWaterScheduleMap());

            // Growth
            modelBuilder.ApplyConfiguration(new LightZoneMap());
            modelBuilder.ApplyConfiguration(new LightZoneScheduleMap());
            modelBuilder.ApplyConfiguration(new WaterZoneMap());
            modelBuilder.ApplyConfiguration(new WaterZoneScheduleMap());

            // Batches
            modelBuilder.ApplyConfiguration(new BatchMap());
            modelBuilder.ApplyConfiguration(new BatchPlanMap());
            modelBuilder.ApplyConfiguration(new BatchRowMap());
            
            modelBuilder.ApplyConfiguration(new JobMap());
            modelBuilder.ApplyConfiguration(new JobTrayMap());


			modelBuilder.Entity<Farm>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<FarmType>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<Floor>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<Rack>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<Layer>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<Tray>().HasQueryFilter(x => x.DeletedDateTime == null);

            modelBuilder.Entity<Product>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<Recipe>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<RecipeLightSchedule>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<RecipeWaterSchedule>().HasQueryFilter(x => x.DeletedDateTime == null);

            modelBuilder.Entity<LightZone>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<LightZoneSchedule>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<WaterZone>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<WaterZoneSchedule>().HasQueryFilter(x => x.DeletedDateTime == null);

            modelBuilder.Entity<Batch>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<BatchRow>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<BatchPlan>().HasQueryFilter(x => x.DeletedDateTime == null);
            modelBuilder.Entity<Job>().HasQueryFilter(x => x.DeletedDateTime == null);
		}
    }
}

namespace VHS.Data.Core.Repositories;
public interface IWaterZoneScheduleRepository : IRepository<WaterZoneSchedule>
{
}
public class WaterZoneScheduleRepository : Repository<WaterZoneSchedule>, IWaterZoneScheduleRepository
{
    private readonly VHSCoreDBContext _context;

    public WaterZoneScheduleRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

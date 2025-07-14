namespace VHS.Data.Core.Repositories;

public interface ILightZoneScheduleRepository : IRepository<LightZoneSchedule>
{
}
public class LightZoneScheduleRepository : Repository<LightZoneSchedule>, ILightZoneScheduleRepository
{
    private readonly VHSCoreDBContext _context;

    public LightZoneScheduleRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

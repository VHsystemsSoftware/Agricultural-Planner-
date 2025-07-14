namespace VHS.Data.Core.Repositories;

public interface IWaterZoneRepository : IRepository<WaterZone>
{
}
public class WaterZoneRepository : Repository<WaterZone>, IWaterZoneRepository
{
    private readonly VHSCoreDBContext _context;

    public WaterZoneRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

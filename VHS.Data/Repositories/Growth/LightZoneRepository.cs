namespace VHS.Data.Core.Repositories;

public interface ILightZoneRepository : IRepository<LightZone>
{
}
public class LightZoneRepository : Repository<LightZone>, ILightZoneRepository
{
    private readonly VHSCoreDBContext _context;

    public LightZoneRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

namespace VHS.Data.Core.Repositories;

public interface IFarmTypeRepository : IRepository<FarmType> {}

public class FarmTypeRepository : Repository<FarmType>, IFarmTypeRepository
{
    private readonly VHSCoreDBContext _context;

    public FarmTypeRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

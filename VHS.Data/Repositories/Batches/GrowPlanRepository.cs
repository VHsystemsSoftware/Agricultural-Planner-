namespace VHS.Data.Core.Repositories;

public interface IGrowPlanRepository : IRepository<GrowPlan>
{
}

public class GrowPlanRepository : Repository<GrowPlan>, IGrowPlanRepository
{
    private readonly VHSCoreDBContext _context;

    public GrowPlanRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

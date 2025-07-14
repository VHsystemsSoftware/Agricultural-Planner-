namespace VHS.Data.Core.Repositories;

public interface IBatchPlanRepository : IRepository<BatchPlan>
{
}

public class BatchPlanRepository : Repository<BatchPlan>, IBatchPlanRepository
{
    private readonly VHSCoreDBContext _context;

    public BatchPlanRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

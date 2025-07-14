namespace VHS.Data.Core.Repositories;

public interface IBatchRepository : IRepository<Batch>
{
}
public class BatchRepository : Repository<Batch>, IBatchRepository
{
    private readonly VHSCoreDBContext _context;

    public BatchRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

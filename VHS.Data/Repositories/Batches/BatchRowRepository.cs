namespace VHS.Data.Core.Repositories;

public interface IBatchRowRepository : IRepository<BatchRow>
{
}

public class BatchRowRepository : Repository<BatchRow>, IBatchRowRepository
{
    private readonly VHSCoreDBContext _context;

    public BatchRowRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

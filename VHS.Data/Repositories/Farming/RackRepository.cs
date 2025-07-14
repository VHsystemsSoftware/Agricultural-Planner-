using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace VHS.Data.Core.Repositories;

public interface IRackRepository : IRepository<Rack> {}

public class RackRepository : Repository<Rack>, IRackRepository
{
    private readonly VHSCoreDBContext _context;

    public RackRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Rack>> GetAllAsync(Expression<Func<Rack, bool>>? filter = null, params string[] includeProperties)
    {
        IQueryable<Rack> query = _context.Racks
                                         .Include(r => r.Floor.Farm);
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync();
    }

    public override async Task<Rack?> GetByIdAsync(object id)
    {
        return await _context.Racks
                            .Include(r => r.Floor.Farm)
                             .FirstOrDefaultAsync(r => r.Id == (Guid)id);
    }
}

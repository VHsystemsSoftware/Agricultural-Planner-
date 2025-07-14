using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace VHS.Data.Core.Repositories;

public interface IFloorRepository : IRepository<Floor> {}

public class FloorRepository : Repository<Floor>, IFloorRepository
{
    private readonly VHSCoreDBContext _context;

    public FloorRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Floor>> GetAllAsync(Expression<Func<Floor, bool>>? filter = null, params string[] includeProperties)
    {
        IQueryable<Floor> query = _context.Floors
                                          .Include(f => f.Farm)
                                          .Include(f => f.Racks);
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync();
    }

    public override async Task<Floor?> GetByIdAsync(object id)
    {
        return await _context.Floors
                             .Include(f => f.Farm)
                             .Include(f => f.Racks)
                             .FirstOrDefaultAsync(f => f.Id == (Guid)id);
    }
}

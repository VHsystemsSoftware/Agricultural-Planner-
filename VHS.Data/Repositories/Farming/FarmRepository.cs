using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace VHS.Data.Core.Repositories;

public interface IFarmRepository : IRepository<Farm>
{
	Task<Guid> GetFirstFarmId();
}

public class FarmRepository : Repository<Farm>, IFarmRepository
{
	private readonly VHSCoreDBContext _context;

	public FarmRepository(VHSCoreDBContext context) : base(context)
	{
		_context = context;
	}

	public override async Task<IEnumerable<Farm>> GetAllAsync(Expression<Func<Farm, bool>>? filter = null, params string[] includeProperties)
	{
		IQueryable<Farm> query = _context.Farms
										 .Include(f => f.FarmType)
										 .Include(f => f.Floors)
											 .ThenInclude(fl => fl.Racks).ThenInclude(r => r.Layers);
		if (filter != null)
		{
			query = query.Where(filter);
		}
		return await query.ToListAsync();
	}

	public override async Task<Farm?> GetByIdAsync(object id)
	{
		return await _context.Farms
							 .Include(f => f.FarmType)
							 .Include(f => f.Floors)
								 .ThenInclude(fl => fl.Racks)
									.ThenInclude(r => r.Layers)
							 .FirstOrDefaultAsync(f => f.Id == (Guid)id);
	}

	public async Task<Guid> GetFirstFarmId()
	{
		return await _context.Farms.Select(x => x.Id).FirstAsync();
	}
}

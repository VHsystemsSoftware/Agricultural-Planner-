using VHS.Data.Common.Infrastructure;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit.Repositories;

public interface ISystemMessageRepository : IRepository<SystemMessage>
{
}

public class SystemMessageRepository : Repository<SystemMessage>, ISystemMessageRepository
{
	private readonly VHSAuditDBContext _context;

	public SystemMessageRepository(VHSAuditDBContext context) : base(context)
    {
        _context = context;
    }
}

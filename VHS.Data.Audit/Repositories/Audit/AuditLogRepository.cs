using VHS.Data.Common.Infrastructure;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
}
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    private readonly VHSAuditDBContext _context;

    public AuditLogRepository(VHSAuditDBContext context) : base(context)
    {
        _context = context;
    }
}

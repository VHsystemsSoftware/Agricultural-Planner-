using VHS.Data.Common.Infrastructure;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit.Repositories;

public interface IOPCAuditRepository : IRepository<OPCAudit>
{
    Task Delete(Guid id);
}
public class OPCAuditRepository : Repository<OPCAudit>, IOPCAuditRepository
	{
    private readonly VHSAuditDBContext _context;

    public OPCAuditRepository(VHSAuditDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task Delete(Guid id)
    {
        var audit = await _context.OPCAudits.FindAsync(id);
        if (audit != null)
        {
            _context.OPCAudits.Remove(audit);
            await _context.SaveChangesAsync();
        }
	}
}

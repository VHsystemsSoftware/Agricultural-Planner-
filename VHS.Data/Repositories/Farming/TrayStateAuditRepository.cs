namespace VHS.Data.Core.Repositories;

public interface ITrayStateAuditRepository : IRepository<TrayStateAudit>
{
	
}

public class TrayStateAuditRepository : Repository<TrayStateAudit>, ITrayStateAuditRepository
{
	private readonly VHSCoreDBContext _context;

	public TrayStateAuditRepository(VHSCoreDBContext context) : base(context)
	{
		_context = context;
	}




}

using Microsoft.EntityFrameworkCore.Storage;
using VHS.Data.Audit.Repositories;

namespace VHS.Data.Audit.Infrastructure;

public interface IUnitOfWorkAudit : IDisposable
{
    IAuditLogRepository AuditLog { get; }
    IOPCAuditRepository OPCAudit { get; }

	Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<IDbContextTransaction> EnsureTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

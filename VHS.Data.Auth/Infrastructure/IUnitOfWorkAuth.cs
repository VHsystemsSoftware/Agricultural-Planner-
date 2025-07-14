using Microsoft.EntityFrameworkCore.Storage;
using VHS.Data.Auth.Repositories;

namespace VHS.Data.Auth.Infrastructure;

public interface IUnitOfWorkAuth : IDisposable
{
    IUserRepository User { get; }
    IUserSettingRepository UserSetting { get; }  

		Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<IDbContextTransaction> EnsureTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

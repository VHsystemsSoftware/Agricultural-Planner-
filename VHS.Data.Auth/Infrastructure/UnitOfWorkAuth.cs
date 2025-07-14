using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using VHS.Data.Auth.Repositories;

namespace VHS.Data.Auth.Infrastructure;

public class UnitOfWorkAuth : IUnitOfWorkAuth, IDisposable
{
    private readonly VHSAuthDBContext _contextCore;
		private readonly ILogger<UnitOfWorkAuth> _logger;
    private IDbContextTransaction? _transaction;

    public IUserRepository User { get; }
    public IUserSettingRepository UserSetting { get; }

    public UnitOfWorkAuth(
        VHSAuthDBContext contextCore,
        ILogger<UnitOfWorkAuth> logger,
        IUserRepository userRepository,
        IUserSettingRepository userSettingRepository

        )
    {
        _contextCore = contextCore;
        _logger = logger;
        User = userRepository;
        UserSetting = userSettingRepository;
    }

    public async Task<int> SaveChangesAsync() => await _contextCore.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _logger.LogInformation("Starting transaction...");
            _transaction = await _contextCore.Database.BeginTransactionAsync();
        }
        return _transaction;
    }

    public async Task<IDbContextTransaction> EnsureTransactionAsync()
    {
        return _transaction ?? await BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            try
            {
                await _transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Transaction commit failed: {ex.Message}");
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            try
            {
                await _transaction.RollbackAsync();
                _logger.LogWarning("Transaction rolled back.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Transaction rollback failed: {ex.Message}");
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
			_contextCore.Dispose();
    }
}

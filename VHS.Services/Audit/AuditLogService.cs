using VHS.Data.Models.Audit;
using VHS.Services.Audit.DTO;

namespace VHS.Services.Audit;

public interface IAuditLogService
{
    Task CreateAuditLogAsync(AuditLogDTO auditLogDto);
}

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWorkAudit _unitOfWork;

    public AuditLogService(IUnitOfWorkAudit unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAuditLogAsync(AuditLogDTO auditLogDto)
    {
        var auditLog = new AuditLog
        {
            UserId = auditLogDto.UserId,
            EntityName = auditLogDto.EntityName,
            Action = auditLogDto.Action,
            KeyValues = auditLogDto.KeyValues,
            OldValues = auditLogDto.OldValues,
            NewValues = auditLogDto.NewValues,
            Timestamp = auditLogDto.Timestamp
        };

        await _unitOfWork.AuditLog.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }
}

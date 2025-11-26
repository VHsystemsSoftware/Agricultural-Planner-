using Microsoft.EntityFrameworkCore;
using VHS.Data.Models.Audit;
using VHS.Services.SystemMessages.DTO;

namespace VHS.Services.SystemMessages;

public interface ISystemMessageService
{
    Task<IEnumerable<SystemMessageDTO>> GetSystemMessagesAsync(Guid? severity = null, Guid? category = null);
    Task<SystemMessageDTO> CreateMessageAsync(Guid severity, Guid category, string message);
}

public class SystemMessageService : ISystemMessageService
{
    private readonly IUnitOfWorkAudit _unitOfWork;

    public SystemMessageService(IUnitOfWorkAudit unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private IQueryable<SystemMessageDTO> MapToDTO(IQueryable<SystemMessage> data)
    {
        return data.Select(sm => new SystemMessageDTO
        {
            Severity = sm.Severity,
            Category = sm.Category,
            Message = sm.Message,
            AddedDateTime = sm.AddedDateTime
        });
    }

    public async Task<IEnumerable<SystemMessageDTO>> GetSystemMessagesAsync(Guid? severity = null, Guid? category = null)
    {
        var query = _unitOfWork.SystemMessage
            .Query(sm =>
                (!severity.HasValue || sm.Severity == severity.Value) &&
                (!category.HasValue || sm.Category == category.Value));

        return await MapToDTO(query)
            .AsNoTracking()
            .OrderByDescending(sm => sm.AddedDateTime)
            .ToListAsync();
    }

    public async Task<SystemMessageDTO> CreateMessageAsync(Guid severity, Guid category, string message)
    {
        var newMessage = new SystemMessage
        {
            Id = Guid.NewGuid(),
            Severity = severity,
            Category = category,
            Message = message,
            AddedDateTime = DateTime.UtcNow
        };

        await _unitOfWork.SystemMessage.AddAsync(newMessage);
        await _unitOfWork.SaveChangesAsync();

        return new SystemMessageDTO
        {
            Severity = newMessage.Severity,
            Category = newMessage.Category,
            Message = newMessage.Message,
            AddedDateTime = newMessage.AddedDateTime
        };
    }
    
}

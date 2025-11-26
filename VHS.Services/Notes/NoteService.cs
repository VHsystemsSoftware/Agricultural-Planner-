using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHS.Services.Audit;
using VHS.Services.Audit.DTO;
using VHS.Services.Notes.DTO;

namespace VHS.Services.Notes;

public interface INoteService
{
    Task<IEnumerable<NoteDTO>> GetNotesForEntityAsync(Guid entityId);
    Task<NoteDTO?> GetNoteByIdAsync(Guid id);
    Task<NoteDTO> CreateNoteAsync(NoteDTO noteDto, string userId);
    Task UpdateNoteAsync(NoteDTO noteDto, string userId);
}

public static class NoteDTOSelect
{
    public static IQueryable<NoteDTO> MapNoteToDTO(this IQueryable<Note> data)
    {
        var method = System.Reflection.MethodBase.GetCurrentMethod();
        return data.TagWith(method.Name)
            .Select(n => new NoteDTO
            {
                Id = n.Id,
                EntityId = n.EntityId,
                Text = n.Text,
                AddedDateTime = n.AddedDateTime
            });
    }
}

public class NoteService : INoteService
{
    private readonly IUnitOfWorkCore _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public NoteService(IUnitOfWorkCore unitOfWork, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    private static NoteDTO SelectNoteToDTO(Note n) => new NoteDTO
    {
        Id = n.Id,
        EntityId = n.EntityId,
        Text = n.Text,
        AddedDateTime = n.AddedDateTime
    };

    public async Task<IEnumerable<NoteDTO>> GetNotesForEntityAsync(Guid entityId)
    {
        return await _unitOfWork.Note
            .Query(n => n.EntityId == entityId)
            .MapNoteToDTO()
            .AsNoTracking()
            .OrderByDescending(n => n.AddedDateTime)
            .ToListAsync();
    }

    public async Task<NoteDTO?> GetNoteByIdAsync(Guid id)
    {
        var note = await _unitOfWork.Note.GetByIdAsync(id);
        if (note == null)
            return null;

        return SelectNoteToDTO(note);
    }

    public async Task<NoteDTO> CreateNoteAsync(NoteDTO noteDto, string userId)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            EntityId = noteDto.EntityId,
            Text = noteDto.Text,
            AddedDateTime = DateTime.UtcNow
        };

        await _unitOfWork.Note.AddAsync(note);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result > 0)
        {
            var newNoteDto = SelectNoteToDTO(note);
            await CreateAuditLogAsync("Added", userId, note.Id, null, newNoteDto);
            return newNoteDto;
        }

        throw new Exception("Note creation failed.");
    }

    public async Task UpdateNoteAsync(NoteDTO noteDto, string userId)
    {
        var note = await _unitOfWork.Note.GetByIdAsync(noteDto.Id);
        if (note == null)
            throw new Exception("Note not found");

        var oldNoteDto = SelectNoteToDTO(note);

        note.Text = noteDto.Text; // Only text is updatable

        _unitOfWork.Note.Update(note);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result > 0)
        {
            var newNoteDto = SelectNoteToDTO(note);
            await CreateAuditLogAsync("Modified", userId, note.Id, oldNoteDto, newNoteDto);
        }
    }

    private async Task CreateAuditLogAsync(string action, string userId, Guid entityId, NoteDTO? oldDto, NoteDTO? newDto)
    {
        var auditLog = new AuditLogDTO
        {
            UserId = string.IsNullOrEmpty(userId) ? "SYSTEM" : userId,
            EntityName = nameof(Note),
            Action = action,
            Timestamp = DateTime.UtcNow,
            KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
            OldValues = oldDto == null ? null : JsonSerializer.Serialize(oldDto),
            NewValues = newDto == null ? null : JsonSerializer.Serialize(newDto)
        };

        await _auditLogService.CreateAuditLogAsync(auditLog);
    }
}

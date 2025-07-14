namespace VHS.Services.Audit.DTO;

public class AuditLogDTO
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string KeyValues { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
}

namespace VHS.Services.SystemMessages.DTO;

public class SystemMessageDTO
{
    public Guid Severity { get; set; }
    public Guid Category { get; set; }
    public string Message { get; set; }
    public DateTime AddedDateTime { get; set; }
}

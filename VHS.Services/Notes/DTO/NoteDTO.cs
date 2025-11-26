namespace VHS.Services.Notes.DTO;

public class NoteDTO
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string Text { get; set; }
    public DateTime AddedDateTime { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class Note
{
    public Guid Id { get; set; }
    [Required]
    public Guid EntityId { get; set; }
    [Required]
    public string Text { get; set; }
    public DateTime AddedDateTime { get; set; }

    public Note()
    {
        AddedDateTime = DateTime.UtcNow;
    }
}


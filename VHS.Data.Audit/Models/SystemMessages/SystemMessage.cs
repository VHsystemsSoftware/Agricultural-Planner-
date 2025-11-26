using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Models.Audit;

public class SystemMessage
{
    public Guid Id { get; set; }
    [Required]
    public Guid Severity { get; set; }
    [Required]
    public Guid Category { get; set; }
    [Required]
    public string Message { get; set; }
    public DateTime AddedDateTime { get; set; }

    public SystemMessage()
    {
        AddedDateTime = DateTime.UtcNow;
    }
}

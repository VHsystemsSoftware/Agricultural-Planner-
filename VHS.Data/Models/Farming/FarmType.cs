using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class FarmType
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();

    public FarmType()
    {
        AddedDateTime = DateTime.UtcNow;
    }
}

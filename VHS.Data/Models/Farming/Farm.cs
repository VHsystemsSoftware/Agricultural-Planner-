using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;  

public partial class Farm
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid FarmTypeId { get; set; }
    public virtual FarmType FarmType { get; set; }

    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

    public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public Farm()
    {
        AddedDateTime = DateTime.UtcNow;
    }
}

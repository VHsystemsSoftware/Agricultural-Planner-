using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class Tray
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid FarmId { get; set; }
    public virtual Farm Farm { get; set; }

    [Required]
    [MaxLength(255)]
    public string Tag { get; set; } = string.Empty;

    public Guid StatusId { get; set; } //in-use, broken, removed

    public virtual ICollection<TrayState> TrayStates { get; set; }

    public TrayState? CurrentState
    {
        get
        {
            return TrayStates.Where(x => x.FinishedDateTime == null).SingleOrDefault();
		}
	}

	public DateTime AddedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public Tray()
    {
        AddedDateTime = DateTime.UtcNow;
    }
}

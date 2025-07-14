using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VHS.Data.Auth.Models.Auth;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;

    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    [InverseProperty("User")]
    public virtual UserSetting? UserSetting { get; set; }

    public User()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }
}

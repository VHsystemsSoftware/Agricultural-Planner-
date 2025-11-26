namespace VHS.Services.Auth.DTO;

public class UserAuthorizationDTO
{
    public bool IsGlobalAdmin { get; set; }
    public bool CanViewUsers { get; set; }
    public bool CanCreateUsers { get; set; }
    public bool CanEditUsers { get; set; }
    public bool CanDeleteUsers { get; set; }
    public bool CanChangeUserRole { get; set; }
    public string CurrentUserRole { get; set; } = string.Empty;
    public Guid? CurrentUserId { get; set; }
}

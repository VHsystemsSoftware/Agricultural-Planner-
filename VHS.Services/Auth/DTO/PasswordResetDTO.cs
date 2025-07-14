namespace VHS.Services.Auth.DTO;

public class PasswordResetDTO
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

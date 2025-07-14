namespace VHS.Data.Auth.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; } = false;
}

namespace VHS.Services.Auth.DTO;

public class LoginResultDTO
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public UserDTO? User { get; set; }

    public static LoginResultDTO Success(string token, UserDTO user)
    {
        return new()
        {
            Succeeded = true,
            Token = token,
            User = user
        };
    }

    public static LoginResultDTO Failed() => new() { Succeeded = false };
}

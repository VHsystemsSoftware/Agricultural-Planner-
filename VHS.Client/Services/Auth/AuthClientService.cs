using System.Net.Http.Json;
using VHS.Services.Auth.DTO;

namespace VHS.Client.Services.Auth
{
    public class AuthClientService
    {
        private readonly HttpClient _httpClient;

        public AuthClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResultDTO> LoginAsync(LoginDTO loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResultDTO>()
                   ?? throw new InvalidOperationException("No content returned from login.");
        }

        public async Task ResetPasswordAsync(PasswordResetDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}

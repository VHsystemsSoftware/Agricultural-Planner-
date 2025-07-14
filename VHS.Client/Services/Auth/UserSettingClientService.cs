using System.Net;
using System.Net.Http.Json;
using VHS.Services.Auth.DTO;

namespace VHS.Client.Services.Auth
{
    public class UserSettingClientService
    {
        private readonly HttpClient _httpClient;

        public UserSettingClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserSettingDTO?> GetUserSettingsByUserIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"api/user/settings/{userId}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserSettingDTO>();
        }

        public async Task<UserSettingDTO?> UpdateUserSettingsAsync(UserSettingDTO settingsDto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/user/settings/update", settingsDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserSettingDTO>();
        }
    }
}

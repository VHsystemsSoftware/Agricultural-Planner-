using System.Net;
using System.Net.Http.Json;
using VHS.Services.Auth.DTO;

namespace VHS.Client.Services.Auth
{
    public class UserClientService
    {
        private readonly HttpClient _httpClient;

        public UserClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserDTO>?> GetAllUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserDTO>>("api/user");
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/user/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<UserDTO?> CreateUserAsync(UserDTO userDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/user/create", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<UserDTO?> UpdateUserAsync(UserDTO userDto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/user/update", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/user/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
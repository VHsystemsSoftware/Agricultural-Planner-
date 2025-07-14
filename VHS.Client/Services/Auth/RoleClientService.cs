using System.Net.Http.Json;

namespace VHS.Client.Services.Auth
{
    public class RoleClientService
    {
        private readonly HttpClient _httpClient;

        public RoleClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>?> GetRolesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<string>>("api/roles");
        }
    }
}

using System.Net.Http.Json;
using VHS.Services.Farming.DTO;

namespace VHS.Client.Services.Farming
{
    public class RackClientService
    {
        private readonly HttpClient _httpClient;

        public RackClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RackDTO>?> GetAllRacksAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RackDTO>>("api/rack");
        }

        public async Task<IEnumerable<RackDTO>?> GetAllRacksByTypeAsync(string typeName)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RackDTO>>($"api/rack/type/{typeName}");
        }

        public async Task<RackDTO?> GetRackByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<RackDTO>($"api/rack/{id}");
        }

        public async Task<RackDTO?> CreateRackAsync(RackDTO rackDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/rack", rackDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RackDTO>();
        }

        public async Task UpdateRackAsync(RackDTO rackDto)
        {
            await _httpClient.PutAsJsonAsync($"api/rack/{rackDto.Id}", rackDto);
        }

        public async Task DeleteRackAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/rack/{id}");
        }

        public async Task EnableRackAsync(EnabledDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/rack/enable/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}

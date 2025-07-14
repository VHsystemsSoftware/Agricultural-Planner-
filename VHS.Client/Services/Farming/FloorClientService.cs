using System.Net.Http.Json;
using VHS.Services.Farming.DTO;

namespace VHS.Client.Services.Farming
{
    public class FloorClientService
    {
        private readonly HttpClient _httpClient;

        public FloorClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<FloorDTO>?> GetAllFloorsAsync(bool enabledOnly = false)
        {
            var url = $"api/floor?enabledOnly={enabledOnly.ToString().ToLower()}";
            return await _httpClient.GetFromJsonAsync<IEnumerable<FloorDTO>>(url);
        }

        public async Task<FloorDTO?> GetFloorByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<FloorDTO>($"api/floor/{id}");
        }

        public async Task<FloorDTO?> CreateFloorAsync(FloorDTO floorDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/floor", floorDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FloorDTO>();
        }

        public async Task UpdateFloorAsync(FloorDTO floorDto)
        {
            await _httpClient.PutAsJsonAsync($"api/floor/{floorDto.Id}", floorDto);
        }

        public async Task EnableFloorAsync(EnabledDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/floor/enable/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteFloorAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/floor/{id}");
        }
    }
}

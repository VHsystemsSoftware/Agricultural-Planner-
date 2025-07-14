using System.Net.Http.Json;
using VHS.Services.Growth.DTO;

namespace VHS.Client.Services.Growth
{
    public class WaterZoneClientService
    {
        private readonly HttpClient _httpClient;

        public WaterZoneClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WaterZoneDTO>?> GetAllWaterZonesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WaterZoneDTO>>("api/waterzone");
        }

        public async Task<WaterZoneDTO?> GetWaterZoneByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<WaterZoneDTO>($"api/waterzone/{id}");
        }

        public async Task<WaterZoneDTO?> CreateWaterZoneAsync(WaterZoneDTO waterZoneDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/waterzone", waterZoneDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WaterZoneDTO>();
        }

        public async Task UpdateWaterZoneAsync(WaterZoneDTO waterZoneDto)
        {
            await _httpClient.PutAsJsonAsync($"api/waterzone/{waterZoneDto.Id}", waterZoneDto);
        }

        public async Task DeleteWaterZoneAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/waterzone/{id}");
        }
    }
}

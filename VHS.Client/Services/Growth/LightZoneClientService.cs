using System.Net.Http.Json;
using VHS.Services.Growth.DTO;

namespace VHS.Client.Services.Growth
{
    public class LightZoneClientService
    {
        private readonly HttpClient _httpClient;

        public LightZoneClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<LightZoneDTO>?> GetAllLightZonesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LightZoneDTO>>("api/lightzone");
        }

        public async Task<LightZoneDTO?> GetLightZoneByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<LightZoneDTO>($"api/lightzone/{id}");
        }

        public async Task<LightZoneDTO?> CreateLightZoneAsync(LightZoneDTO lightZoneDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/lightzone", lightZoneDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LightZoneDTO>();
        }

        public async Task UpdateLightZoneAsync(LightZoneDTO lightZoneDto)
        {
            await _httpClient.PutAsJsonAsync($"api/lightzone/{lightZoneDto.Id}", lightZoneDto);
        }

        public async Task DeleteLightZoneAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/lightzone/{id}");
        }
    }
}

using System.Net.Http.Json;
using VHS.Services.Farming.DTO;

namespace VHS.Client.Services.Batches
{
    public class TrayStateClientService
	{
        private readonly HttpClient _httpClient;

        public TrayStateClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TrayStateDTO>?> GetCurrentStates()
        {
            return await _httpClient.GetFromJsonAsync<List<TrayStateDTO>>("api/traystate/current");
        }

        public async Task<List<TrayStateDTO>?> GetCurrentStates(Guid batchId)
        {
            return await _httpClient.GetFromJsonAsync<List<TrayStateDTO>>($"api/traystate/{batchId}");
        }

        public async Task UpdateTrayStateAsync(TrayStateDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/traystate/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }

    }
}

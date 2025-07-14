using System.Net.Http.Json;
using VHS.Services.Growth.DTO;
using VHS.Services.Growth.Helpers;

namespace VHS.Client.Services.Growth
{
    public class WaterZoneScheduleClientService
    {
        private readonly HttpClient _httpClient;

        public WaterZoneScheduleClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WaterZoneScheduleDTO>?> GetAllSchedulesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WaterZoneScheduleDTO>>("api/waterzoneschedule");
        }

        public async Task<WaterZoneScheduleDTO?> GetScheduleByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<WaterZoneScheduleDTO>($"api/waterzoneschedule/{id}");
        }

        public async Task<IEnumerable<WaterZoneScheduleDTO>> GetSchedulesByZoneAsync(Guid zoneId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WaterZoneScheduleDTO>>($"api/waterzoneschedule/zone/{zoneId}") ?? Enumerable.Empty<WaterZoneScheduleDTO>();
        }

        public async Task<WaterZoneScheduleDTO?> CreateScheduleAsync(WaterZoneScheduleDTO scheduleDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/waterzoneschedule", scheduleDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WaterZoneScheduleDTO>();
        }

        public async Task UpdateScheduleAsync(WaterZoneScheduleDTO scheduleDto)
        {
            await _httpClient.PutAsJsonAsync($"api/waterzoneschedule/{scheduleDto.Id}", scheduleDto);
        }

        public async Task DeleteScheduleAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/waterzoneschedule/{id}");
        }

        public Task<double> CalculateDWR(WaterZoneScheduleDTO schedule)
        {
            double newDWR = WaterZoneScheduleDWRHelper.CalculateDWR(schedule.Volume, schedule.StartTime, schedule.EndTime);
            return Task.FromResult(newDWR);
        }
    }
}

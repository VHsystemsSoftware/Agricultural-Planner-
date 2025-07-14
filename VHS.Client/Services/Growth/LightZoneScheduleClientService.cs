using System.Net.Http.Json;
using VHS.Services.Growth.DTO;
using VHS.Services.Growth.Helpers;

namespace VHS.Client.Services.Growth
{
    public class LightZoneScheduleClientService
    {
        private readonly HttpClient _httpClient;

        public LightZoneScheduleClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<LightZoneScheduleDTO>?> GetAllLightZoneSchedulesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LightZoneScheduleDTO>>("api/lightzoneschedule");
        }

        public async Task<LightZoneScheduleDTO?> GetLightZoneScheduleByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<LightZoneScheduleDTO>($"api/lightzoneschedule/{id}");
        }

        public async Task<IEnumerable<LightZoneScheduleDTO>> GetSchedulesByZoneAsync(Guid zoneId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LightZoneScheduleDTO>>($"api/lightzoneschedule/zone/{zoneId}") ?? Enumerable.Empty<LightZoneScheduleDTO>();
        }

        public async Task<LightZoneScheduleDTO?> CreateLightZoneScheduleAsync(LightZoneScheduleDTO scheduleDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/lightzoneschedule", scheduleDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LightZoneScheduleDTO>();
        }

        public async Task UpdateLightZoneScheduleAsync(LightZoneScheduleDTO scheduleDto)
        {
            await _httpClient.PutAsJsonAsync($"api/lightzoneschedule/{scheduleDto.Id}", scheduleDto);
        }

        public async Task DeleteLightZoneScheduleAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/lightzoneschedule/{id}");
        }

        public Task<double> CalculateDLI(LightZoneScheduleDTO schedule)
        {
            double newDLI = LightZoneScheduleDLIHelper.CalculateDLI(schedule.Intensity, schedule.StartTime, schedule.EndTime);
            return Task.FromResult(newDLI);
        }
    }
}

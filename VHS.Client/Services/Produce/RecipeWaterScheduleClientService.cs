using System.Net.Http.Json;
using VHS.Services.Produce.DTO;

namespace VHS.Client.Services.Produce
{
    public class RecipeWaterScheduleClientService
    {
        private readonly HttpClient _httpClient;

        public RecipeWaterScheduleClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RecipeWaterScheduleDTO>?> GetAllRecipeWaterSchedulesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RecipeWaterScheduleDTO>>("api/recipewaterschedule");
        }

        public async Task<RecipeWaterScheduleDTO?> GetRecipeWaterScheduleByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeWaterScheduleDTO>($"api/recipewaterschedule/{id}");
        }

        public async Task<RecipeWaterScheduleDTO?> CreateRecipeWaterScheduleAsync(RecipeWaterScheduleDTO scheduleDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/recipewaterschedule", scheduleDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RecipeWaterScheduleDTO>();
        }

        public async Task UpdateRecipeWaterScheduleAsync(RecipeWaterScheduleDTO scheduleDto)
        {
            await _httpClient.PutAsJsonAsync($"api/recipewaterschedule/{scheduleDto.Id}", scheduleDto);
        }

        public async Task DeleteRecipeWaterScheduleAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/recipewaterschedule/{id}");
        }
    }
}

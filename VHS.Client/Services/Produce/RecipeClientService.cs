using System.Net.Http.Json;
using VHS.Services.Produce.DTO;

namespace VHS.Client.Services.Produce
{
    public class RecipeClientService
    {
        private readonly HttpClient _httpClient;

        public RecipeClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RecipeDTO>?> GetAllRecipesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RecipeDTO>>("api/recipe");
        }

        public async Task<RecipeDTO?> GetRecipeByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeDTO>($"api/recipe/{id}");
        }

        public async Task<List<RecipeDTO>> GetRecipesByFarmAsync(Guid farmId)
        {
            return await _httpClient.GetFromJsonAsync<List<RecipeDTO>>($"api/recipe/byfarm/{farmId}");
        }

        public async Task<RecipeDTO?> CreateRecipeAsync(RecipeDTO recipeDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/recipe", recipeDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RecipeDTO>();
        }

        public async Task<RecipeDTO?> UpdateRecipeAsync(RecipeDTO recipeDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/recipe/{recipeDto.Id}", recipeDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RecipeDTO>();
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/recipe/{id}");
        }
    }
}

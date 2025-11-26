using System.Net.Http;
using System.Net.Http.Json;
using VHS.Services.Results.DTO;

namespace VHS.Client.Services.Results;

public class ResultClientService
{
    private readonly HttpClient _httpClient;

    public ResultClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ResultItemDTO>> GetResultsAsync(ResultFilterDTO filter)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/result/filter", filter);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ResultItemDTO>>() ?? new();
        }
        catch (Exception ex)
        {
            return new List<ResultItemDTO>();
        }
    }
}

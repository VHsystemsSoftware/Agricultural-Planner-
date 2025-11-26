using System.Net.Http.Json;
using VHS.Services.Batches.DTO;
using VHS.Client.Services.Batches;

namespace VHS.Client.Services;

public class GrowPlanAlgoritmeService
{
    private readonly HttpClient _httpClient;

    public GrowPlanAlgoritmeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GrowDemandResultPerDay>?> GetGrowPlanCalculationOutputAsync(Guid farmId, DateOnly startDate, List<GrowPlanDTO> demands, bool cleanGrowRack = false)
    {
        var url = $"api/GrowPlanAlgoritme?farmId={farmId}&startDate={startDate:yyyy-MM-dd}&cleanGrowRack={cleanGrowRack}";
        var response = await _httpClient.PostAsJsonAsync(url, demands);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<GrowDemandResultPerDay>>();
        }
        return null;
    }
}

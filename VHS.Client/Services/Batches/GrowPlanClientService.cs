using System.Net.Http.Json;
using VHS.Services.Batches.DTO;

namespace VHS.Client.Services.Batches;

public class GrowPlanClientService
{
    private readonly HttpClient _httpClient;

    public GrowPlanClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GrowDemandResultPerDay>?> GetGrowPlanCalculationOutputAsync(Guid farmId, GrowPlanDTO growPlan, int rackSize, int layers, bool cleanGrowRack = false)
    {
        var url = $"api/GrowPlanAlgoritme/{farmId}/{rackSize}/{layers}/{cleanGrowRack}";
        var response = await _httpClient.PostAsJsonAsync(url, growPlan);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<GrowDemandResultPerDay>>();
        }
        return null;
    }

    public async Task CreateGrowPlanMultipleAsync(GrowPlanDTO configDto)
    {
        await _httpClient.PostAsJsonAsync("api/growplan/multiple", configDto);
    }

    public async Task<IEnumerable<GrowPlanDTO>?> GetAllGrowPlansAsync(
          Guid? productId = null,
          string? name = null,
          Guid? recipeId = null,
          DateTime? startDateFrom = null,
          DateTime? startDateTo = null)
    {
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
        {
            Path = "api/growplan"
        };
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

        if (productId.HasValue)
        {
            query["productId"] = productId.Value.ToString();
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            query["name"] = name;
        }
        if (recipeId.HasValue)
        {
            query["recipeId"] = recipeId.Value.ToString();
        }
        if (startDateFrom.HasValue)
        {
            query["startDateFrom"] = startDateFrom.Value.ToString("o");
        }
        if (startDateTo.HasValue)
        {
            query["startDateTo"] = startDateTo.Value.ToString("o");
        }

        uriBuilder.Query = query.ToString();
        return await _httpClient.GetFromJsonAsync<IEnumerable<GrowPlanDTO>>(uriBuilder.Uri.ToString());
    }

}

using VHS.Services.Home.DTO;
using VHS.Services.SystemMessages.DTO;
using System.Net.Http.Json;

namespace VHS.Client.Services.Home;

public class HomeClientService
{
    private readonly HttpClient _httpClient;

    public HomeClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PlanningStatsDTO> GetPlanningStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<PlanningStatsDTO>("api/home/planning");
    }

    public async Task<OperationalStatsDTO> GetOperationalStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<OperationalStatsDTO>("api/home/operational");
    }

    public async Task<ResultsStatsDTO> GetResultsStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<ResultsStatsDTO>($"api/home/results");
    }

    public async Task<List<SystemMessageDTO>> GetSystemMessagesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<SystemMessageDTO>>("api/home/messages");
    }
}

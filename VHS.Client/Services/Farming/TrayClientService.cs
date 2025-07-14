using System.Net.Http.Json;
using VHS.Services.Common.DataGrid;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.Client.Services.Farming
{
    public class TrayClientService
    {
        private readonly HttpClient _httpClient;

        public TrayClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedResult<TrayDTO>?> GetTraysByStatusAsync(
            Guid trayStatus,
            int pageIndex,
            int pageSize,
            string sortColumn,
            SortDirectionEnum sortDirection)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "trayStatus", trayStatus.ToString() },
                { "pageIndex", pageIndex.ToString() },
                { "pageSize", pageSize.ToString() },
                { "sortColumn", sortColumn },
                { "sortDirection", sortDirection.ToString() }
            };

            var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            return await _httpClient.GetFromJsonAsync<PaginatedResult<TrayDTO>>($"api/tray/status/?{query}");
        }
    }
}

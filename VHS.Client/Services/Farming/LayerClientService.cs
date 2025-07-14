using System.Net.Http.Json;
using VHS.Services.Common.DataGrid;
using VHS.Services.Common.DataGrid.Enums;
using VHS.Services.Farming.DTO;

namespace VHS.Client.Services.Farming
{
    public class LayerClientService
    {
        private readonly HttpClient _httpClient;

        public LayerClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<LayerDTO>?> GetAllLayersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LayerDTO>>("api/layer");
        }

        public async Task<PaginatedResult<LayerDTO>?> GetLayersByRackAsync(
            Guid rackId,
            int pageIndex,
            int pageSize,
            string sortColumn,
            SortDirectionEnum sortDirection)
        {
            var queryParams = new Dictionary<string, string>
        {
            { "rackId", rackId.ToString() },
            { "pageIndex", pageIndex.ToString() },
            { "pageSize", pageSize.ToString() },
            { "sortColumn", sortColumn },
            { "sortDirection", sortDirection.ToString() }
        };

            var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            return await _httpClient.GetFromJsonAsync<PaginatedResult<LayerDTO>>($"api/layer/rack/?{query}");
        }

        public async Task UpdateLayerAsync(LayerDTO layerDto)
        {
            await _httpClient.PutAsJsonAsync($"api/layer/{layerDto.Id}", layerDto);
        }

        public async Task EnableLayerAsync(EnabledDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/layer/enable/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}

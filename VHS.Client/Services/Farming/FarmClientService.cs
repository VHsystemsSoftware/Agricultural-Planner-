using System.Net.Http.Json;
using VHS.Services.Farming.DTO;

namespace VHS.Client.Services.Farming
{
    public class FarmClientService
    {
        private readonly HttpClient _httpClient;

        public FarmClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<FarmDTO>?> GetAllFarmsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<FarmDTO>>("api/farm");
        }
		public async Task<IEnumerable<FarmDTO>?> GetAllFarmsSimpleAsync()
		{
			return await _httpClient.GetFromJsonAsync<IEnumerable<FarmDTO>>("api/farm/simple");
		}
		public async Task<FarmDTO?> GetFarmByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<FarmDTO>($"api/farm/{id}");
        }

        public async Task<IEnumerable<FarmTypeDTO>?> GetAllFarmTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<FarmTypeDTO>>("api/farm/types");
        }

        public async Task<FarmDTO?> CreateFarmAsync(FarmDTO farmDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/farm", farmDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FarmDTO>();
        }

        public async Task<FarmDTO?> UpdateFarmAsync(FarmDTO farmDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/farm/{farmDto.Id}", farmDto);
            return await response.Content.ReadFromJsonAsync<FarmDTO>();
        }

        public async Task DeleteFarmAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/farm/{id}");
        }

        public async Task<List<LayerOccupancyDTO>?> GetLayerOccupancyAsync(Guid farmId, DateTime? asOf)
        {
            var date = asOf ?? DateTime.Now.Date;
            var url = $"api/farm/{farmId}/occupancy?asOf={date:yyyy-MM-dd}";
            return await _httpClient.GetFromJsonAsync<List<LayerOccupancyDTO>>(url);
        }
		public async Task<List<LayerOccupancyDTO>?> GetRackOccupancyAsync(Guid farmId, Guid rackId)
		{
			var url = $"api/farm/{farmId}/rackoccupancy/{rackId}";
			return await _httpClient.GetFromJsonAsync<List<LayerOccupancyDTO>>(url);
		}
		//public async Task<FarmAllocationPlan?> RunFarmAllocationAsync(
		//    Guid farmId,
		//    List<ProductCategoryBatchSizeDTO> batchSizes,
		//    int totalDays,
		//    int totalTraysAvailable,
		//    DateTime startDate)
		//{
		//    var request = new
		//    {
		//        BatchSizes = batchSizes,
		//        TotalDays = totalDays,
		//        TotalTraysAvailable = totalTraysAvailable,
		//        StartDate = startDate
		//    };

		//    var response = await _httpClient.PostAsJsonAsync($"api/farm/plan/{farmId}", request);
		//    response.EnsureSuccessStatusCode();
		//    return await response.Content.ReadFromJsonAsync<FarmAllocationPlan>();
		//}
	}
}

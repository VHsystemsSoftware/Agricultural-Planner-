using System.Net.Http.Json;
using VHS.Services.Batches.DTO;

namespace VHS.Client.Services.Batches
{
    public class BatchPlanClientService
    {
        private readonly HttpClient _httpClient;

        public BatchPlanClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<GrowPlanDTO>?> GetAllBatchPlansAsync(
            Guid? productId = null,
            string? name = null,
            Guid? recipeId = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            IEnumerable<Guid>? statusIds = null)
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

            if (statusIds != null && statusIds.Any())
            {
                foreach (var statusId in statusIds)
                {
                    query.Add("statusIds", statusId.ToString());
                }
            }

            uriBuilder.Query = query.ToString();
            return await _httpClient.GetFromJsonAsync<IEnumerable<GrowPlanDTO>>(uriBuilder.Uri.ToString());
        }

        public async Task<GrowPlanDTO?> GetBatchPlanByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<GrowPlanDTO>($"api/growplan/{id}");
        }

        public async Task<GrowPlanDTO?> CreateBatchPlanAsync(GrowPlanDTO configDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/growplan", configDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GrowPlanDTO>();
        }


        public async Task UpdateBatchPlanAsync(GrowPlanDTO configDto)
        {
            await _httpClient.PutAsJsonAsync($"api/growplan/{configDto.Id}", configDto);
        }

        public async Task DeleteBatchPlanAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/growplan/{id}");
        }

        public async Task<List<BatchRowDTO>> CalculateAssignmentsAsync(Guid rackTypeId, int traysPerDay, int days, bool includeTransportLayer = true)
        {
            var url = $"api/growplan/calculate-assignments?rackTypeId={rackTypeId}&traysPerDay={traysPerDay}&days={days}&includeTransportLayer={includeTransportLayer.ToString().ToLower()}";
            var response = await _httpClient.GetFromJsonAsync<List<BatchRowDTO>>(url);
            return response ?? new List<BatchRowDTO>();
        }

        public async Task StartBatchPlanAsync(GrowPlanDTO batchPlan)
        {
            var response = await _httpClient.PostAsJsonAsync("api/growplan/startplan", batchPlan);
            response.EnsureSuccessStatusCode();
        }

		public async Task StartBatchPlanAsync(Guid batchPlanId)
		{
			var response = await _httpClient.PostAsync($"api/growplan/startplan/{batchPlanId}", null);
			response.EnsureSuccessStatusCode();
		}

        public async Task<GrowPlanDTO?> DuplicateBatchPlanAsync(Guid id)
        {
            var response = await _httpClient.PostAsync($"api/growplan/{id}/duplicate", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GrowPlanDTO>();
        }

        public async Task StopBatchPlanAsync(Guid id, DateTime endDate)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/batchplan/{id}/stop", endDate);
            response.EnsureSuccessStatusCode();
        }

        public async Task CancelBatchPlanAsync(Guid id)
        {
            var response = await _httpClient.PostAsync($"api/batchplan/{id}/cancel", null);
            response.EnsureSuccessStatusCode();
        }
    }
}

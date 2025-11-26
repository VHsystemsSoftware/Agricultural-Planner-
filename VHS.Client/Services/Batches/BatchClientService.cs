using System.Net.Http.Json;
using VHS.Services.Batches.DTO;

namespace VHS.Client.Services.Batches
{
    public class BatchClientService
    {
        private readonly HttpClient _httpClient;

        public BatchClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<BatchDTO>?> GetAllBatchesAsync(
            string? batchName = null,
            string? planName = null,
            DateTime? seedDateFrom = null,
            DateTime? seedDateTo = null,
            DateTime? harvestDateFrom = null,
            DateTime? harvestDateTo = null)
        {
            var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
            {
                Path = "api/batch"
            };
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(batchName))
                query["batchName"] = batchName;
            if (!string.IsNullOrWhiteSpace(planName))
                query["planName"] = planName;
            if (seedDateFrom.HasValue)
                query["seedDateFrom"] = seedDateFrom.Value.ToString("o");
            if (seedDateTo.HasValue)
                query["seedDateTo"] = seedDateTo.Value.ToString("o");
            if (harvestDateFrom.HasValue)
                query["harvestDateFrom"] = harvestDateFrom.Value.ToString("o");
            if (harvestDateTo.HasValue)
                query["harvestDateTo"] = harvestDateTo.Value.ToString("o");

            uriBuilder.Query = query.ToString();
            return await _httpClient.GetFromJsonAsync<IEnumerable<BatchDTO>>(uriBuilder.Uri.ToString());
        }

        public async Task<BatchDTO?> GetBatchByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<BatchDTO>($"api/batch/{id}");
        }

        public async Task<BatchDTO?> CreateBatchAsync(BatchDTO batchDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/batch", batchDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BatchDTO>();
        }

        public async Task UpdateBatchAsync(BatchDTO batchDto)
        {
            await _httpClient.PutAsJsonAsync($"api/batch/{batchDto.Id}", batchDto);
        }

        public async Task DeleteBatchAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/batch/{id}");
        }

		public async Task UpdateBatchAsync(Guid id, Guid jobId, string lotReference)
		{
			await _httpClient.PutAsJsonAsync($"api/batch/lofreference/{id}/{jobId}", lotReference);
		}
	}
}

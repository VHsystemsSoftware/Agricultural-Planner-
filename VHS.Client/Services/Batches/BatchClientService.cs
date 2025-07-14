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

        public async Task<IEnumerable<BatchDTO>?> GetAllBatchesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<BatchDTO>>("api/batch");
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

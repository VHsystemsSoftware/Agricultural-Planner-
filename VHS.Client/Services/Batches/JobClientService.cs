using System.Net.Http.Json;
using VHS.Services.Batches.DTO;

namespace VHS.Client.Services.Batches
{
    public class JobClientService
    {
        private readonly HttpClient _httpClient;

        public JobClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<JobDTO>?> GetAllJobsAsync(
            string? name = null,
            string? batchName = null,
            Guid? jobLocationTypeId = null,
            DateTime? scheduledDateFrom = null,
            DateTime? scheduledDateTo = null,
            IEnumerable<Guid>? statusIds = null)
        {
            var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
            {
                Path = "api/job"
            };
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query["name"] = name;
            }
            if (!string.IsNullOrWhiteSpace(batchName))
            {
                query["batchName"] = batchName;
            }
            if (jobLocationTypeId.HasValue)
            {
                query["jobLocationTypeId"] = jobLocationTypeId.Value.ToString();
            }
            if (scheduledDateFrom.HasValue)
            {
                query["scheduledDateFrom"] = scheduledDateFrom.Value.ToString("o");
            }
            if (scheduledDateTo.HasValue)
            {
                query["scheduledDateTo"] = scheduledDateTo.Value.ToString("o");
            }
            if (statusIds != null && statusIds.Any())
            {
                foreach (var statusId in statusIds)
                {
                    query.Add("statusIds", statusId.ToString());
                }
            }

            uriBuilder.Query = query.ToString();
            return await _httpClient.GetFromJsonAsync<IEnumerable<JobDTO>>(uriBuilder.Uri.ToString());
        }

		public async Task<IEnumerable<JobDTO>?> GetAllSeedingJobsAsync()
		{
			return await _httpClient.GetFromJsonAsync<IEnumerable<JobDTO>>("api/job/seeding");
		}
		public async Task<IEnumerable<JobDTO>?> GetAllTransplantingJobsAsync()
		{
			return await _httpClient.GetFromJsonAsync<IEnumerable<JobDTO>>("api/job/transplant");
		}
		public async Task<IEnumerable<JobDTO>?> GetAlHarvestingJobsAsync()
		{
			return await _httpClient.GetFromJsonAsync<IEnumerable<JobDTO>>("api/job/harvesting");
		}
		public async Task<IEnumerable<JobDTO>?> GetAlTransportJobsAsync()
		{
			return await _httpClient.GetFromJsonAsync<IEnumerable<JobDTO>>("api/job/transport");
		}
		public async Task<JobDTO?> GetJobByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<JobDTO>($"api/job/{id}");
        }

        public async Task<JobDTO?> CreateJobAsync(JobDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/job", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JobDTO>();
        }

        public async Task<bool> UpdateJobAsync(JobDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/job/{dto.Id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteJobAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/job/{id}");
        }

        //public async Task<IEnumerable<JobAssignmentDTO>?> GetAllUsersAsync()
        //{
        //    return await _httpClient.GetFromJsonAsync<List<JobAssignmentDTO>>("api/user");
        //}

        //public async Task<IEnumerable<JobBatchPhaseDTO>?> GetAllPhasesAsync(Guid jobId)
        //{
        //    return await _httpClient.GetFromJsonAsync<List<JobBatchPhaseDTO>>($"api/jobbatchphase/job/{jobId}");
        //}

        public async Task<IEnumerable<BatchDTO>?> GetAllBatchesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<BatchDTO>>("api/batch");
        }

        public async Task<IEnumerable<JobDTO>?> GetSuggestedJobsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<JobDTO>>($"api/job/suggestions");
        }
		public async Task<IEnumerable<JobTrayDTO>?> GetJobTrays(Guid jobId)
		{
			return await _httpClient.GetFromJsonAsync<IEnumerable<JobTrayDTO>>($"api/job/trays/{jobId}");
		}


		public async Task SetPausedAsync(Guid id)
		{
			await _httpClient.PutAsJsonAsync($"api/job/pause/{id}", id);
		}
		public async Task SetUnPausedAsync(Guid id)
		{
			await _httpClient.PutAsJsonAsync($"api/job/unpause/{id}", id);
		}
	}
}

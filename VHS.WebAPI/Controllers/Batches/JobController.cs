using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using VHS.Services.Batches.DTO;
using VHS.WebAPI.Hubs;

namespace VHS.WebAPI.Controllers.Batches
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Temporary allowed
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IJobTrayService _jobTrayService;
		private readonly IHubContext<VHSNotificationHub, IHubCommunicator> _hubContext;

		public JobController(IJobService jobService, IJobTrayService jobTrayService, IHubContext<VHSNotificationHub, IHubCommunicator> hubContext)
        {
            _jobService = jobService;
            _jobTrayService = jobTrayService;
            _hubContext = hubContext;
		}

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobs(
        [FromQuery] string? name = null,
        [FromQuery] string? batchName = null,
        [FromQuery] Guid? jobLocationTypeId = null,
        [FromQuery] DateTime? scheduledDateFrom = null,
        [FromQuery] DateTime? scheduledDateTo = null,
        [FromQuery] IEnumerable<Guid>? statusIds = null)
        {
            var jobs = await _jobService.GetAllJobsAsync(
                name,
                batchName,
                jobLocationTypeId,
                scheduledDateFrom,
                scheduledDateTo,
                statusIds);
            return Ok(jobs);
        }

		[HttpGet("seeding")]
		public async Task<IActionResult> GetAllSeedingJobs()
		{
			var jobs = await _jobService.GetAllSeedingJobsAsync();
			return Ok(jobs);
		}
		[HttpGet("transplant")]
		public async Task<IActionResult> GetAllTransplantJobs()
		{
			var jobs = await _jobService.GetAllTransplantJobsAsync();
			return Ok(jobs);
		}
		[HttpGet("harvesting")]
		public async Task<IActionResult> GetAllHarvestingJobs()
		{
			var jobs = await _jobService.GetAllHarvestingJobsAsync();
			return Ok(jobs);
		}

		[HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(Guid id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound();
            return Ok(job);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobDTO jobDto)
        {
            var createdJob = await _jobService.CreateJobAsync(jobDto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetJobById), new { id = createdJob.Id }, createdJob);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(Guid id, [FromBody] JobDTO jobDto)
        {
            if (id != jobDto.Id)
                return BadRequest("ID mismatch");
            await _jobService.UpdateJobAsync(jobDto, GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(Guid id)
        {
            await _jobService.DeleteJobAsync(id, GetCurrentUserId());
			
            await _hubContext.Clients.All.RefreshDashboardSeeder();
			
            return NoContent();
        }

        //[HttpGet("suggestions")]
        //public async Task<IActionResult> GetSuggestedJobs()
        //{
        //    var list = await _jobService.GetSuggestedJobsAsync();
        //    return Ok(list);
        //}

		[HttpGet("trays/{jobId}")]
		public async Task<IActionResult> GetJobTraysByJobId(Guid jobId)
		{
			var job = await _jobTrayService.GetAllByJobTrayIdAsync(jobId);
			if (job == null)
				return NotFound();
			return Ok(job);
		}

		[HttpPut("Pause/{id}")]
		public async Task<IActionResult> SetPause(Guid id)
		{			
			await _jobService.ChangePaused(id,true, GetCurrentUserId());
			return NoContent();
		}
		[HttpPut("UnPause/{id}")]
		public async Task<IActionResult> SetUnPause(Guid id)
		{
			await _jobService.ChangePaused(id, false, GetCurrentUserId());
			return NoContent();
		}
	}
}

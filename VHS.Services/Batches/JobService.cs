using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using VHS.Data.Core.Models;
using VHS.Services.Audit;
using VHS.Services.Audit.DTO;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IJobService
{
	Task<IEnumerable<JobDTO>> GetAllHarvestingJobsAsync();
	Task<IEnumerable<JobDTO>> GetAllSeedingJobsAsync();
	Task<IEnumerable<JobDTO>> GetAllTransplantJobsAsync();
	Task<IEnumerable<JobDTO>> GetAllJobsAsync(
		string? name = null,
		string? batchName = null,
		Guid? jobLocationTypeId = null,
		DateTime? scheduledDateFrom = null,
		DateTime? scheduledDateTo = null,
		IEnumerable<Guid>? statusIds = null);
	Task<JobDTO?> GetJobByIdAsync(Guid id);
	Task<JobDTO> CreateJobAsync(JobDTO dto, string userId);
	Task UpdateJobAsync(JobDTO dto, string userId);
	Task DeleteJobAsync(Guid id, string userId);
	//Task<List<JobDTO>> GetSuggestedJobsAsync();

	Task ChangePaused(Guid jobId, bool pause, string userId);
}

public static class JobDTOSelect
{
	public static IQueryable<JobDTO> MapJobToDTO(this IQueryable<Job> data)
	{
		var method = System.Reflection.MethodBase.GetCurrentMethod();
		return data.TagWith(method.Name)
			.Include(x => x.Batch.BatchPlan.Recipe.Product)
			.Select(x => new JobDTO
			{
				Id = x.Id,
				Paused = x.Paused,
				OrderOnDay = x.OrderOnDay,
				Name = x.Name,
				TrayCount = x.TrayCount,
				JobLocationTypeId = x.JobLocationTypeId,
				StatusId = x.StatusId,
				BatchId = x.BatchId,
				ScheduledDate = x.ScheduledDate,
				BatchName = x.Batch != null ? x.Batch.BatchName : string.Empty,
				LotReference = x.Batch != null ? x.Batch.LotReference : string.Empty,
				AddedDateTime = x.AddedDateTime,
				JobTypeId = x.JobTypeId,
				JobTrays = x.JobTrays.Select(jt => new JobTrayDTO
				{
					Id = jt.Id,
					TrayId = jt.TrayId,
					DestinationLayerId = jt.DestinationLayerId,
					DestinationLocation = jt.DestinationLocation,
					OrderInJob = jt.OrderInJob,
					RecipeId = jt.RecipeId,
					TransportLayerId = jt.TransportLayerId,
					AddedDateTime = jt.AddedDateTime,
					RecipeName = jt.RecipeId.HasValue ? jt.Recipe.Name : string.Empty,
					SeedIdentifier = jt.RecipeId.HasValue ? jt.Recipe.Product.SeedIdentifier : string.Empty,
					SeedSupplier = jt.RecipeId.HasValue ? jt.Recipe.Product.SeedSupplier : string.Empty,
					DestinationLayer = jt.DestinationLayerId.HasValue ? new LayerDTO()
					{
						Id = jt.DestinationLayer.Id,
						RackTypeId = jt.DestinationLayer.Rack.TypeId,
						TrayCountPerLayer = jt.DestinationLayer.Rack.TrayCountPerLayer,
						RackId = jt.DestinationLayer.RackId
					} : null
				}).ToList(),
				Batch = x.Batch != null ? new BatchDTO
				{
					Id = x.Batch.Id,
					BatchName = x.Batch.BatchName,
					LotReference = x.Batch.LotReference,
					HarvestDate = x.Batch.HarvestDate,
					SeedDate = x.Batch.SeedDate,
					FarmId = x.Batch.FarmId,
					Recipe = x.Batch.BatchPlan != null && x.Batch.BatchPlan.Recipe != null ? new RecipeDTO
					{
						Id = x.Batch.BatchPlan.Recipe.Id,
						Name = x.Batch.BatchPlan.Recipe.Name,
						Product = new ProductDTO
						{
							Id = x.Batch.BatchPlan.Recipe.Product.Id,
							Name = x.Batch.BatchPlan.Recipe.Product.Name,
							ProductCategoryId = x.Batch.BatchPlan.Recipe.Product.ProductCategoryId
						}
					} : null
				} : null
			});
	}
}


public class JobService : IJobService
{
	private readonly IUnitOfWorkCore _unitOfWork;
	private readonly IAuditLogService _auditLogService;

	public JobService(IUnitOfWorkCore unitOfWork, IAuditLogService auditLogService)
	{
		_unitOfWork = unitOfWork;
		_auditLogService = auditLogService;
	}

	public async Task<IEnumerable<JobDTO>> GetAllJobsAsync(
		string? name = null,
		string? batchName = null,
		Guid? jobLocationTypeId = null,
		DateTime? scheduledDateFrom = null,
		DateTime? scheduledDateTo = null,
		IEnumerable<Guid>? statusIds = null)
	{
		IQueryable<Job> query = _unitOfWork.Job.Query();

		if (!string.IsNullOrWhiteSpace(name))
		{
			query = query.Where(x => x.Name.Contains(name));
		}

		if (!string.IsNullOrWhiteSpace(batchName))
		{
			query = query.Where(x => x.Batch != null && x.Batch.BatchName.Contains(batchName));
		}

		if (jobLocationTypeId.HasValue && jobLocationTypeId.Value != Guid.Empty)
		{
			query = query.Where(x => x.JobLocationTypeId == jobLocationTypeId.Value);
		}

		if (scheduledDateFrom.HasValue)
		{
			query = query.Where(x => x.ScheduledDate >= scheduledDateFrom.Value);
		}

		if (scheduledDateTo.HasValue)
		{
			query = query.Where(x => x.ScheduledDate <= scheduledDateTo.Value.Date.AddDays(1).AddTicks(-1));
		}

		if (statusIds != null && statusIds.Any())
		{
			query = query.Where(x => statusIds.Contains(x.StatusId));
		}

		var jobs = await query
			.MapJobToDTO()
			.AsNoTracking()
			.OrderBy(x => x.ScheduledDate)
			.ThenBy(x => x.OrderOnDay)
			.ThenBy(x => x.AddedDateTime)
			.ToListAsync();

		return jobs;
	}

	public async Task<IEnumerable<JobDTO>> GetAllSeedingJobsAsync()
	{
		var jobs = await _unitOfWork.Job
			.Query(x => x.JobLocationTypeId == GlobalConstants.JOBLOCATION_SEEDER)
			.MapJobToDTO()
			.AsNoTracking()
			.OrderBy(x => x.ScheduledDate)
			.ToListAsync();

		return jobs;
	}
	public async Task<IEnumerable<JobDTO>> GetAllTransplantJobsAsync()
	{
		var jobs = await _unitOfWork.Job
			.Query(x => x.JobLocationTypeId == GlobalConstants.JOBLOCATION_TRANSPLANTER)
			.MapJobToDTO()
			.AsNoTracking()
			.OrderBy(x => x.ScheduledDate)
			.ToListAsync();

		return jobs;
	}
	public async Task<IEnumerable<JobDTO>> GetAllHarvestingJobsAsync()
	{
		var jobs = await _unitOfWork.Job
			.Query(x => x.JobLocationTypeId == GlobalConstants.JOBLOCATION_HARVESTER)
			.MapJobToDTO()
			.AsNoTracking()
			.OrderBy(x => x.ScheduledDate)
			.ToListAsync();
		return jobs;
	}

	public async Task<JobDTO?> GetJobByIdAsync(Guid id)
	{
		var job = await _unitOfWork.Job
			.Query(x => x.Id == id)
			.MapJobToDTO()
			.AsNoTracking()
			.SingleOrDefaultAsync();
		return job;
	}

	public async Task<JobDTO> CreateJobAsync(JobDTO dto, string userId)
	{
		var job = new Job
		{
			Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
			OrderOnDay = dto.OrderOnDay,
			Name = dto.Name,
			TrayCount = dto.TrayCount,
			ScheduledDate = dto.ScheduledDate,
			JobLocationTypeId = dto.JobLocationTypeId,
			StatusId = dto.StatusId,
			BatchId = dto.BatchId,
			AddedDateTime = DateTime.UtcNow,
			ModifiedDateTime = DateTime.UtcNow
		};

		await _unitOfWork.Job.AddAsync(job);
		var result = await _unitOfWork.SaveChangesAsync();

		if (result > 0)
		{
			var oldJobDto = await GetJobByIdAsync(job.Id);
			await CreateAuditLogAsync("Added", userId, job.Id, null, oldJobDto);
		}

		return await GetJobByIdAsync(job.Id);
	}

	public async Task UpdateJobAsync(JobDTO dto, string userId)
	{
		var job = await _unitOfWork.Job.GetByIdAsync(dto.Id)
				  ?? throw new Exception("Job not found");

		var oldJobDto = await GetJobByIdAsync(job.Id);

		job.OrderOnDay = dto.OrderOnDay;
		job.Name = dto.Name;
		job.TrayCount = dto.TrayCount;
		job.ScheduledDate = dto.ScheduledDate;
		job.ModifiedDateTime = DateTime.UtcNow;
		job.JobLocationTypeId = dto.JobLocationTypeId;
		job.StatusId = dto.StatusId;
		job.JobTypeId = dto.JobTypeId;

		_unitOfWork.Job.Update(job);
		var result = await _unitOfWork.SaveChangesAsync();

		if (result > 0)
		{
			var newJobDto = await GetJobByIdAsync(job.Id);
			await CreateAuditLogAsync("Modified", userId, job.Id, oldJobDto, newJobDto);
		}
	}

	public async Task DeleteJobAsync(Guid id, string userId)
	{
		var job = await _unitOfWork.Job.GetByIdAsync(id)
				  ?? throw new Exception("Job not found");

		var oldJobDto = await GetJobByIdAsync(id);

		job.DeletedDateTime = DateTime.UtcNow;
		_unitOfWork.Job.Update(job);
		var result = await _unitOfWork.SaveChangesAsync();

		if (result > 0)
		{
			await CreateAuditLogAsync("Deleted", userId, job.Id, oldJobDto, null);
		}
	}

	private async Task CreateAuditLogAsync(string action, string userId, Guid entityId, JobDTO? oldDto, JobDTO? newDto)
	{
		var auditLog = new AuditLogDTO
		{
			UserId = string.IsNullOrEmpty(userId) ? "SYSTEM" : userId,
			EntityName = nameof(Job),
			Action = action,
			Timestamp = DateTime.UtcNow,
			KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
			OldValues = oldDto == null ? null : JsonSerializer.Serialize(oldDto),
			NewValues = newDto == null ? null : JsonSerializer.Serialize(newDto)
		};

		await _auditLogService.CreateAuditLogAsync(auditLog);
	}

	public async Task ChangePaused(Guid jobId, bool pause, string userId)
	{
		var job = await _unitOfWork.Job.GetByIdAsync(jobId)
				  ?? throw new Exception("Job not found");

		job.Paused = pause;
		job.ModifiedDateTime = DateTime.UtcNow;
		_unitOfWork.Job.Update(job);
		await _unitOfWork.SaveChangesAsync();

		await CreateAuditLogAsync("PauseChanged", userId, job.Id, null, null);
	}

	//public async Task<List<JobDTO>> GetSuggestedJobsAsync()
	//{
	//	// 1) Load all new or started plans, including their recipes & existing jobs
	//	var plans = await _unitOfWork.BatchPlan.GetAllAsync(
	//		bp => bp.StatusId == GlobalConstants.BATCHPLANSTATUS_STARTED,
	//		includeProperties: new[] { "Recipe", "Batches.Jobs" }
	//	);

	//	var today = DateTime.Today;
	//	var suggestions = new List<JobDTO>();

	//	foreach (var plan in plans.OrderBy(p => p.Name))
	//	{
	//		// gather already‐scheduled jobs to avoid dupes
	//		var existing = plan.Batches
	//			.SelectMany(b => b.Jobs)
	//			.Select(j => (Date: j.ScheduledDate.Date, Loc: j.JobLocationTypeId))
	//			.ToHashSet();

	//		void MaybeAdd(DateTime dt, Guid loc, string name)
	//		{
	//			var key = (Date: dt.Date, Loc: loc);
	//			if (!existing.Contains(key))
	//			{
	//				suggestions.Add(new JobDTO
	//				{
	//					Id = Guid.Empty,
	//					BatchId = plan.Batches.FirstOrDefault()?.Id,
	//					Name = name,
	//					OrderOnDay = 1,
	//					TrayCount = plan.TraysPerDay,
	//					ScheduledDate = dt,
	//					JobLocationTypeId = loc,
	//					StatusId = GlobalConstants.JOBSTATUS_NOTSTARTED
	//				});
	//				existing.Add(key);
	//			}
	//		}

	//		// Recipe plans
	//		if (plan.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_RECIPE && plan.StartDate.HasValue)
	//		{
	//			var sd = plan.StartDate.Value.Date;
	//			MaybeAdd(sd, GlobalConstants.JOBLOCATION_SEEDER, $"Seeding Job ({plan.Name})");

	//			//germination
	//			//if (plan.Recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_MICROGREENS
	//			//	|| plan.Recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_PETITEGREENS)
	//			//{
	//			//	MaybeAdd(sd.AddDays(plan.Recipe.GerminationDays),
	//			//			 GlobalConstants.JOBLOCATION_TRANSPORT,
	//			//			 $"Germination → Growing Job ({plan.Name})");

	//			//	if (plan.Recipe.GrowDays > 0)
	//			//	{
	//			//		var dt = sd.AddDays(plan.Recipe.GerminationDays
	//			//						   + plan.Recipe.GrowDays);
	//			//		MaybeAdd(dt, GlobalConstants.JOBLOCATION_TRANSPORT, $"Growing → Harvesting Job ({plan.Name})");
	//			//		MaybeAdd(dt, GlobalConstants.JOBLOCATION_TRANSPORT, $"Empty Trays → Washing Job ({plan.Name})");
	//			//	}
	//			//}
	//			//propagation
	//			//if (plan.Recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_LETTUCE)
	//			//{
	//			//	MaybeAdd(sd.AddDays(plan.Recipe.GerminationDays + plan.Recipe.PropagationDays),
	//			//			 GlobalConstants.JOBLOCATION_TRANSPORT,
	//			//			 $"Propagation → Growing Job ({plan.Name})");

	//			//	if (plan.Recipe.GrowDays > 0)
	//			//	{
	//			//		var dt = sd.AddDays(plan.Recipe.PropagationDays
	//			//						   + plan.Recipe.GrowDays);
	//			//		MaybeAdd(dt, GlobalConstants.JOBLOCATION_TRANSPORT, $"Growing → Harvesting Job ({plan.Name})");
	//			//		MaybeAdd(dt, GlobalConstants.JOBLOCATION_TRANSPORT, $"Empty Trays → Washing Job ({plan.Name})");
	//			//		MaybeAdd(dt, GlobalConstants.JOBLOCATION_TRANSPORT, $"Empty Trays → Transplanting Job ({plan.Name})");
	//			//	}
	//			//}

	//		}
	//		// Washing/transplant plans
	//		else if ((plan.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_WASHER
	//			   || plan.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_TRANSPLANTER)
	//			   && plan.StartDate.HasValue)
	//		{
	//			var dt = plan.StartDate.Value.Date;
	//			var loc = plan.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_WASHER
	//						? GlobalConstants.JOBLOCATION_SEEDER
	//						: GlobalConstants.JOBLOCATION_SEEDER;
	//			MaybeAdd(dt, loc,
	//					 $"Empty Trays → → {(plan.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_WASHER ? "Washing Job" : "Transplanting Job")} ({plan.Name})");
	//		}
	//		// Manual‐rack plans
	//		//else if (plan.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_RACK
	//		//	  && plan.StartDate.HasValue)
	//		//{
	//		//	var dt = plan.StartDate.Value.Date;
	//		//	MaybeAdd(dt, GlobalConstants.JOBLOCATION_TRANSPORT,
	//		//			 $"Manual Rack Placement Job ({plan.Name})");
	//		//}
	//	}

	//	return suggestions;
	//}
}
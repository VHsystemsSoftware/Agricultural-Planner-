using Microsoft.EntityFrameworkCore;
using VHS.Data.Core.Models;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IBatchPlanService
{
	Task<IEnumerable<BatchPlanDTO>> GetAllBatchPlansAsync(
		Guid? productId = null,
		string? name = null,
		Guid? recipeId = null,
		DateTime? startDateFrom = null,
		DateTime? startDateTo = null,
		IEnumerable<Guid>? statusIds = null);
	Task<BatchPlanDTO?> GetBatchPlanByIdAsync(Guid id);
	Task<BatchPlanDTO> CreateBatchPlanAsync(BatchPlanDTO configDto);
	Task UpdateBatchPlanAsync(BatchPlanDTO configDto);
	Task DeleteBatchPlanAsync(Guid id);
	Task<List<BatchRowDTO>> CalculateAssignmentsAsync(Guid rackTypeId, int traysPerDay, int days, bool includeTransportLayer);
	Task StartBatchPlanAsync(BatchPlanDTO batchPlanDto);
	Task StartBatchPlanAsync(Guid batchPlanId);
}

public static class BatchPlanDTOSelect
{
	public static IQueryable<BatchPlanDTO> MapBatchPlanToDTO(this IQueryable<BatchPlan> data)
	{
		var method = System.Reflection.MethodBase.GetCurrentMethod();
		return data.TagWith(method.Name)
			.Select(c => new BatchPlanDTO
			{
				Id = c.Id,
				Name = c.Name,
				FarmId = c.FarmId,
				BatchPlanTypeId = c.BatchPlanTypeId,
				Recipe = c.RecipeId.HasValue ? new RecipeDTO
				{
					Id = c.RecipeId.Value,
					Name = c.Recipe.Name,
					GerminationDays = c.Recipe.GerminationDays,
					PropagationDays = c.Recipe.PropagationDays,
					GrowDays = c.Recipe.GrowDays
				} : null,
				StartDate = c.StartDate,
				TraysPerDay = c.TraysPerDay,
				DaysForPlan = c.DaysForPlan,
				StatusId = c.StatusId,
				AddedDateTime = c.AddedDateTime,
				Batches = c.Batches.Select(x => new BatchDTO
				{
					Id = x.Id,
					BatchName = x.BatchName,
					FarmId = x.FarmId,
					BatchRows = x.BatchRows.Select(row => new BatchRowDTO
					{
						Id = row.Id,
						BatchId = row.BatchId,
						FloorId = row.FloorId,
						RackId = row.RackId,
						LayerId = row.LayerId,
						LayerRackTypeId = row.Rack.TypeId
					}).ToList() ?? new List<BatchRowDTO>(),
					HarvestDate = x.HarvestDate,
					SeedDate = x.SeedDate,
					TrayCount = x.TrayCount,
					StatusId = x.StatusId
				}).ToList()
			});
	}
}

public class BatchPlanService : IBatchPlanService
{
	private readonly IUnitOfWorkCore _unitOfWork;
	private readonly IRackService _rackService;
	private readonly IBatchService _batchService;
	private readonly IRecipeService _recipeService;

	public BatchPlanService(
		IUnitOfWorkCore unitOfWork,
		IRackService rackService,
		IBatchService batchService,
		IRecipeService recipeService)
	{
		_unitOfWork = unitOfWork;
		_rackService = rackService;
		_batchService = batchService;
		_recipeService = recipeService;
	}
	
	public async Task<IEnumerable<BatchPlanDTO>> GetAllBatchPlansAsync(
		Guid? productId = null,
		string? name = null,
		Guid? recipeId = null,
		DateTime? startDateFrom = null,
		DateTime? startDateTo = null,
		IEnumerable<Guid>? statusIds = null)
	{
		IQueryable<BatchPlan> query = _unitOfWork.BatchPlan.Query();

		if (productId.HasValue && productId.Value != Guid.Empty)
		{
			query = query.Where(x => x.Recipe != null && x.Recipe.ProductId == productId.Value);
		}

		if (!string.IsNullOrWhiteSpace(name))
		{
			query = query.Where(x => x.Name.Contains(name));
		}

		if (recipeId.HasValue && recipeId.Value != Guid.Empty)
		{
			query = query.Where(x => x.RecipeId == recipeId.Value);
		}

		if (startDateFrom.HasValue)
		{
			query = query.Where(x => x.StartDate >= startDateFrom.Value);
		}

		if (startDateTo.HasValue)
		{
			query = query.Where(x => x.StartDate <= startDateTo.Value.Date.AddDays(1).AddTicks(-1));
		}

		if (statusIds != null && statusIds.Any())
		{
			query = query.Where(x => statusIds.Contains(x.StatusId));
		}

		var data = await query
			.OrderByDescending(x => x.StartDate)
			.ThenByDescending(x => x.AddedDateTime)
			.MapBatchPlanToDTO()
			.AsNoTracking()
			.ToListAsync();

		return data;
	}

	public async Task<BatchPlanDTO?> GetBatchPlanByIdAsync(Guid id)
	{
		var data = await _unitOfWork.BatchPlan.Query(x => x.Id == id)
			.MapBatchPlanToDTO()
			.AsNoTracking()
			.SingleOrDefaultAsync();

		return data;
	}

	public async Task<BatchPlanDTO> CreateBatchPlanAsync(BatchPlanDTO batchPlanDto)
	{
		if (batchPlanDto.Recipe?.Id != null && batchPlanDto.Recipe.Id != Guid.Empty)
		{
			var recipe = await _unitOfWork.Recipe.GetByIdAsync(batchPlanDto.Recipe.Id);
			if (recipe == null)
				throw new Exception("Recipe not found.");
		}

		var batchPlan = new BatchPlan
		{
			Id = batchPlanDto.Id == Guid.Empty ? Guid.NewGuid() : batchPlanDto.Id,
			Name = batchPlanDto.Name,
			FarmId = batchPlanDto.FarmId,
			RecipeId = batchPlanDto.Recipe?.Id == Guid.Empty ? null : batchPlanDto.Recipe?.Id,
			DaysForPlan = batchPlanDto.DaysForPlan,
			TraysPerDay = batchPlanDto.TraysPerDay,
			StartDate = batchPlanDto.StartDate,
			StatusId = batchPlanDto.StatusId,
			BatchPlanTypeId = batchPlanDto.BatchPlanTypeId,
		};
		await _unitOfWork.BatchPlan.AddAsync(batchPlan);

		var batch = new Batch
		{
			Id = Guid.NewGuid(),
			BatchName = batchPlan.Name,
			BatchPlan = batchPlan,
			FarmId = batchPlanDto.FarmId,
			TrayCount = batchPlanDto.TraysPerDay * batchPlanDto.DaysForPlan,
			ScheduledDateTime = batchPlanDto.StartDate ?? DateTime.UtcNow,
			SeedDate = batchPlanDto.StartDate,
			StatusId = GlobalConstants.BATCHSTATUS_PLANNED,
			AddedDateTime = DateTime.UtcNow
		};
		await _unitOfWork.Batch.AddAsync(batch);
		batchPlan.Batches.Add(batch);

		foreach (var rowDto in batchPlanDto.Batches.First().BatchRows.Where(r => r.FloorId.HasValue && r.RackId.HasValue && r.LayerId.HasValue))
		{
			var newRow = new BatchRow
			{
				Id = Guid.NewGuid(),
				BatchId = batch.Id,
				FloorId = rowDto.FloorId,
				RackId = rowDto.RackId,
				LayerId = rowDto.LayerId,
				EmptyCount = rowDto.EmptyCount
			};
			await _unitOfWork.BatchRow.AddAsync(newRow);
			batch.BatchRows.Add(newRow);
		}

		await _unitOfWork.SaveChangesAsync();

		return await GetBatchPlanByIdAsync(batchPlan.Id);
	}

	public async Task UpdateBatchPlanAsync(BatchPlanDTO batchPlanDto)
	{
		var includes = new string[] { "Recipe.Product", "Batches.BatchRows" };
		var plan = await _unitOfWork.BatchPlan.GetByIdWithIncludesAsync(batchPlanDto.Id, includeProperties: includes);
		if (plan == null)
			throw new Exception("Batch plan not found");

		plan.Name = batchPlanDto.Name;
		plan.FarmId = batchPlanDto.FarmId;
		plan.RecipeId = batchPlanDto.Recipe?.Id == Guid.Empty ? null : batchPlanDto.Recipe?.Id;
		plan.StartDate = batchPlanDto.StartDate;
		plan.DaysForPlan = batchPlanDto.DaysForPlan;
		plan.TraysPerDay = batchPlanDto.TraysPerDay;
		plan.BatchPlanTypeId = batchPlanDto.BatchPlanTypeId;
		plan.ModifiedDateTime = DateTime.UtcNow;

		_unitOfWork.BatchPlan.Update(plan);

		if (batchPlanDto.Batches.Any() != null)
		{
			await _batchService.UpdateBatchAsync(batchPlanDto.Batches.First());
		}

		await _unitOfWork.SaveChangesAsync();
	}

	public async Task DeleteBatchPlanAsync(Guid id)
	{
		var config = await _unitOfWork.BatchPlan.GetByIdAsync(id);
		if (config == null)
			throw new Exception("Batch configuration not found");

		config.DeletedDateTime = DateTime.UtcNow;
		_unitOfWork.BatchPlan.Update(config);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task<List<BatchRowDTO>> CalculateAssignmentsAsync(Guid rackTypeId, int traysPerDay, int days, bool includeTransportLayer)
	{
		var allRacks = await _rackService.GetRacksByTypeIdAsync(rackTypeId);
		return CalculateAssignments(traysPerDay, days, allRacks, includeTransportLayer);
	}

	private static List<BatchRowDTO> CalculateAssignments(
		int traysPerDay,
		int days,
		List<RackDTO> allRacks,
		bool includeTransportLayer)
	{
		var assignments = new List<BatchRowDTO>();

		if (allRacks == null || allRacks.Count == 0)
			return assignments;

		int traysAssigned = 0;
		int layerCounter = 1;

		var sortedRacks = allRacks
			.Where(x => x.Enabled)
			.Where(x => x.Floor.Enabled)
			.OrderByDescending(r => r.TrayCountPerLayer)
			.ThenBy(r => r.Number)
			.ToList();

		bool allTraysAssigned = false;

		foreach (var rack in sortedRacks)
		{
			if (rack.TypeId == GlobalConstants.RACKTYPE_PROPAGATION)
			{
				traysPerDay = (int)Math.Ceiling((double)traysPerDay / (double)24);
			}
			int totalTraysNeeded = traysPerDay * days;
			foreach (var layer in rack.Layers.Where(x => x.Enabled).OrderBy(l => l.Number))
			{
				traysAssigned += rack.TrayCountPerLayer;

				assignments.Add(new BatchRowDTO
				{
					Number = layerCounter++,
					FloorId = rack.Floor.Id,
					RackId = rack.Id,
					LayerId = layer.Id,
					TrayCount = rack.TrayCountPerLayer,
					EmptyCount = traysAssigned >= totalTraysNeeded ? traysAssigned - totalTraysNeeded : 0,
					LayerRackTypeId = rack.TypeId
				});

				if (traysAssigned >= totalTraysNeeded)
				{
					allTraysAssigned = true;
					break;
				}
			}

			if (allTraysAssigned)
				break;
		}

		return assignments;
	}

	public async Task StartBatchPlanAsync(Guid batchPlanId)
	{
		var dto = await GetBatchPlanByIdAsync(batchPlanId)
			?? throw new Exception("Plan not found");
		await StartBatchPlanAsync(dto);
	}

	public async Task StartBatchPlanAsync(BatchPlanDTO dto)
	{
		if (dto == null) throw new ArgumentNullException(nameof(dto));

		var plan = await _unitOfWork.BatchPlan.GetByIdAsync(dto.Id)
			?? throw new Exception("Plan not found");

		var batch = plan.Batches.FirstOrDefault()
			?? throw new Exception($"No batch found for plan {plan.Name}. Cannot start.");

		plan.StatusId = GlobalConstants.BATCHPLANSTATUS_PLANNED;
		plan.ModifiedDateTime = DateTime.UtcNow;
		_unitOfWork.BatchPlan.Update(plan);

		var seedDate = dto.StartDate!.Value;
		int trayCount = dto.TraysPerDay;

		RecipeDTO recipe = null;
		
		if (dto.Recipe != null)
			recipe = await _recipeService.GetRecipeByIdAsync(dto.Recipe.Id)
				?? throw new Exception("Recipe not found for batch plan.");

		batch.ModifiedDateTime = DateTime.UtcNow;
		_unitOfWork.Batch.Update(batch);

		var allBatchRows = batch.BatchRows?.ToList() ?? new List<BatchRow>();
		var germinationLayers = allBatchRows.Where(r => r.Rack?.TypeId == GlobalConstants.RACKTYPE_GERMINATION).OrderBy(x => x.Layer.Number).ToList();
		var propagationLayers = allBatchRows.Where(r => r.Rack?.TypeId == GlobalConstants.RACKTYPE_PROPAGATION).OrderBy(x => x.Layer.Number).ToList();
		var growLayers = allBatchRows.Where(r => r.Rack?.TypeId == GlobalConstants.RACKTYPE_GROWING).OrderBy(x => x.Layer.Number).ToList();
		var totalCount = 0;

		if (dto.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_RECIPE)
		{
			var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(seedDate);

			// Seeding Job
			var seedingJob = await AddJobAsync(
				batch.Id, newOrderOnDay, $"Seeding Job ({dto.Name})",
				dt: seedDate,
				loc: GlobalConstants.JOBLOCATION_SEEDER,
				jobTypeId: recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_LETTUCE
					? GlobalConstants.JOBTYPE_SEEDING_PROPAGATION
					: GlobalConstants.JOBTYPE_SEEDING_GERMINATION,
				status: GlobalConstants.JOBSTATUS_NOTSTARTED,
				recipeId: dto.Recipe.Id,
				trayCount: trayCount,
				paused: true
			);

			// Propagation Job
			if (recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_LETTUCE)
			{
				totalCount = 0;

				for (int x = 0; x < propagationLayers.Count; x++)
				{
					var propagationLayer = propagationLayers[x];
					var transportLayer = propagationLayer.Rack.Layers.OrderBy(l => l.Number).Last();

					for (int i = 1; i <= propagationLayer.Rack.TrayCountPerLayer; i++)
					{
						var isEmpty = i > propagationLayer.Rack.TrayCountPerLayer - propagationLayer.EmptyCount;
						totalCount++;
						var jtPropagation = new JobTray
						{
							Id = Guid.NewGuid(),
							JobId = seedingJob.Id,
							TrayId = null,
							ParentJobTrayId = null,
							OrderInJob = totalCount,
							DestinationLocation = GlobalConstants.JOBLOCATION_SEEDER,
							DestinationLayerId = propagationLayer.LayerId,
							OrderOnLayer = i,
							RecipeId = isEmpty ? null : dto.Recipe.Id,
							AddedDateTime = seedingJob.AddedDateTime,
							TransportLayerId = transportLayer.Id
						};
						await _unitOfWork.JobTray.AddAsync(jtPropagation);
					}
				}
				seedingJob.TrayCount = totalCount;

				var propagationDate = seedDate.AddDays(recipe.PropagationDays);
				newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(propagationDate);
				var emptyForTransplant = await AddJobAsync(
					batch.Id, newOrderOnDay, $"Empty Trays to Transplant Job ({dto.Name})",
					dt: propagationDate,
					loc: GlobalConstants.JOBLOCATION_SEEDER,
					jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT,
					status: GlobalConstants.JOBSTATUS_NOTSTARTED,
					trayCount: trayCount,
					recipeId: null,
					paused: true
				);
				newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(propagationDate);
				var emptyForTransplanter = await AddJobAsync(
					batch.Id, newOrderOnDay, $"Transplant Transfering Job ({dto.Name})",
					dt: propagationDate,
					loc: GlobalConstants.JOBLOCATION_TRANSPLANTER,
					jobTypeId: GlobalConstants.JOBTYPE_TRANSPLANTING,
					status: GlobalConstants.JOBSTATUS_NOTSTARTED,
					trayCount: trayCount,
					recipeId: dto.Recipe.Id,
					paused: false
				);
				var harvestDate = seedDate.AddDays(recipe.PropagationDays).AddDays(recipe.GrowDays);
				newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(harvestDate);
				var harvestingJob = await AddJobAsync(
					batch.Id, newOrderOnDay, $"Harvesting Job ({dto.Name})", 
					dt: harvestDate,
					loc: GlobalConstants.JOBLOCATION_HARVESTER,
					jobTypeId: GlobalConstants.JOBTYPE_HARVESTING,
					status: GlobalConstants.JOBSTATUS_NOTSTARTED,
					trayCount: trayCount,
					recipeId: dto.Recipe.Id,
					paused: false
				);
				totalCount = 0;

				for (int x = 0; x < growLayers.Count; x++)
				{
					var growLayer = growLayers[x];
					var transportLayer = growLayer.Rack.Layers.OrderBy(l => l.Number).Last();

					for (int i = 1; i <= growLayer.Rack.TrayCountPerLayer; i++)
					{
						totalCount++;
						var isEmpty = i > growLayer.Rack.TrayCountPerLayer - growLayer.EmptyCount;
						var jtemptyTransplant = new JobTray
						{
							Id = Guid.NewGuid(),
							JobId = emptyForTransplant.Id,
							TrayId = null,
							ParentJobTrayId = null,
							OrderInJob = totalCount,
							DestinationLocation = GlobalConstants.JOBLOCATION_SEEDER,
							DestinationLayerId = null,
							OrderOnLayer = i,
							RecipeId = null,
							AddedDateTime = emptyForTransplant.AddedDateTime,
							TransportLayerId = null
						};
						var jtTransplanter = new JobTray
						{
							Id = Guid.NewGuid(),
							JobId = emptyForTransplanter.Id,
							TrayId = null,
							ParentJobTrayId = jtemptyTransplant.Id,
							OrderInJob = totalCount,
							DestinationLocation = GlobalConstants.JOBLOCATION_TRANSPLANTER,
							DestinationLayerId = growLayer.LayerId,
							OrderOnLayer = i,
							RecipeId = isEmpty ? null : dto.Recipe.Id,
							AddedDateTime = emptyForTransplant.AddedDateTime,
							TransportLayerId = transportLayer.Id
						};
						var jtHarvest = new JobTray
						{
							Id = Guid.NewGuid(),
							JobId = harvestingJob.Id,
							TrayId = null,
							ParentJobTrayId = jtemptyTransplant.Id,
							OrderInJob = totalCount,
							DestinationLocation = GlobalConstants.JOBLOCATION_HARVESTER,
							DestinationLayerId = growLayer.LayerId,
							OrderOnLayer = i,
							RecipeId = isEmpty ? null : dto.Recipe.Id,
							AddedDateTime = harvestingJob.AddedDateTime,
							TransportLayerId = null
						};
						await _unitOfWork.JobTray.AddAsync(jtHarvest);
						await _unitOfWork.JobTray.AddAsync(jtTransplanter);
						await _unitOfWork.JobTray.AddAsync(jtemptyTransplant);
					}
				}
			}
			else if (recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_MICROGREENS || recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_PETITEGREENS)
			{
				Dictionary<int, Guid> germinationJobTrays = new Dictionary<int, Guid>();
				var harvestDate = seedDate.AddDays(recipe.GerminationDays).AddDays(recipe.PropagationDays).AddDays(recipe.GrowDays);
				newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(harvestDate);
				var harvestingJob = await AddJobAsync(
					batch.Id, newOrderOnDay, $"Harvesting Job ({dto.Name})", 
					dt: harvestDate,
					loc: GlobalConstants.JOBLOCATION_HARVESTER,
					jobTypeId: GlobalConstants.JOBTYPE_HARVESTING,
					status: GlobalConstants.JOBSTATUS_NOTSTARTED,
					trayCount: trayCount,
					recipeId: dto.Recipe.Id,
					paused: false
				);

				totalCount = 0;
				for (int x = 0; x < germinationLayers.Count; x++)
				{
					var batchRow = germinationLayers[x];
					var transportLayer = batchRow.Rack.Layers.OrderBy(l => l.Number).Last();

					for (int j = 1; j <= batchRow.Rack.TrayCountPerLayer; j++)
					{
						totalCount++;
						var isEmpty = j > batchRow.Rack.TrayCountPerLayer - batchRow.EmptyCount;
						var jtGermination = new JobTray
						{
							Id = Guid.NewGuid(),
							JobId = seedingJob.Id,
							TrayId = null,
							ParentJobTrayId = null,
							OrderInJob = totalCount,
							DestinationLocation = GlobalConstants.JOBLOCATION_SEEDER,
							DestinationLayerId = batchRow.LayerId,
							OrderOnLayer = j,
							RecipeId = isEmpty ? null : dto.Recipe.Id,
							AddedDateTime = seedingJob.AddedDateTime,
							TransportLayerId = transportLayer.Id
						};
						germinationJobTrays.Add(totalCount, jtGermination.Id);
						await _unitOfWork.JobTray.AddAsync(jtGermination);
					}
				}

				seedingJob.TrayCount = totalCount;

				totalCount = 0;
				for (int x = 0; x < growLayers.Count; x++)
				{
					var growLayer = growLayers[x];
					var transportLayer = growLayer.Rack.Layers.OrderBy(l => l.Number).Last();

					for (int i = 1; i <= growLayer.Rack.TrayCountPerLayer; i++)
					{
						totalCount++;
						var isEmpty = i > growLayer.Rack.TrayCountPerLayer - growLayer.EmptyCount;
						var jtHarvest = new JobTray
						{
							Id = Guid.NewGuid(),
							JobId = harvestingJob.Id,
							TrayId = null,
							ParentJobTrayId = totalCount > germinationJobTrays.Count ? null : germinationJobTrays[totalCount],
							OrderInJob = totalCount,
							DestinationLocation = GlobalConstants.JOBLOCATION_HARVESTER,
							DestinationLayerId = growLayer.LayerId,
							OrderOnLayer = i,
							RecipeId = isEmpty ? null : dto.Recipe.Id,
							AddedDateTime = harvestingJob.AddedDateTime,
							TransportLayerId = transportLayer.Id
						};
						await _unitOfWork.JobTray.AddAsync(jtHarvest);
					}
				}
			}
			// Empty Trays to Washing or Transplanting Job
		}
		else if (dto.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_WASHER)
		{
			var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(seedDate);
			var job = await AddJobAsync(
				batch.Id,
				newOrderOnDay,
				$"Empty Trays to Washing Job ({dto.Name})",
				dt: seedDate,
				loc: GlobalConstants.JOBLOCATION_SEEDER,
				jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TOWASHER,
				status: GlobalConstants.JOBSTATUS_NOTSTARTED,
				trayCount: trayCount,
				recipeId: null,
				paused: true
			);

			for (int i = 1; i <= trayCount; i++)
			{
				var jt = new JobTray
				{
					Id = Guid.NewGuid(),
					JobId = job.Id,
					TrayId = null,
					ParentJobTrayId = null,
					OrderInJob = i,
					DestinationLocation = GlobalConstants.JOBLOCATION_SEEDER,
					DestinationLayerId = null,
					OrderOnLayer = null,
					RecipeId = null,
					AddedDateTime = job.AddedDateTime
				};
				await _unitOfWork.JobTray.AddAsync(jt);
			}
		}
		else if (dto.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_TRANSPLANTER)
		{
			var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(seedDate);
			var job = await AddJobAsync(
				batch.Id,
				newOrderOnDay,
				$"Empty Trays to transplant Job ({dto.Name})",
				dt: seedDate,
				loc: GlobalConstants.JOBLOCATION_SEEDER,
				jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT,
				status: GlobalConstants.JOBSTATUS_NOTSTARTED,
				trayCount: trayCount,
				recipeId: null,
				paused: true
			);

			for (int i = 1; i <= trayCount; i++)
			{
				var jt = new JobTray
				{
					Id = Guid.NewGuid(),
					JobId = job.Id,
					TrayId = null,
					ParentJobTrayId = null,
					OrderInJob = i,
					DestinationLocation = GlobalConstants.JOBLOCATION_TRANSPLANTER,
					DestinationLayerId = null,
					OrderOnLayer = null,
					RecipeId = null,
					AddedDateTime = job.AddedDateTime
				};
				await _unitOfWork.JobTray.AddAsync(jt);
			}
		}
		else if (dto.BatchPlanTypeId == GlobalConstants.BATCHPLANTYPE_RACK)
		{
			var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(seedDate);
			var job = await AddJobAsync(
				batch.Id, newOrderOnDay, $"Manual Rack Transport Job ({dto.Name})", 
				dt: seedDate,
				loc: GlobalConstants.JOBLOCATION_SEEDER,
				jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TORACK,
				status: GlobalConstants.JOBSTATUS_NOTSTARTED,
				trayCount: trayCount,
				recipeId: null,
				paused: true
			);

			totalCount = 0;

			for (int x = 0; x < allBatchRows.Count; x++)
			{
				var layer = allBatchRows[x];
				var transportLayer = layer.Rack.Layers.OrderBy(l => l.Number).Last();

				//var rack = layer.Rack;
				for (int i = 1; i <= dto.TotalTrays; i++)
				{
					totalCount++;
					var jt = new JobTray
					{
						Id = Guid.NewGuid(),
						JobId = job.Id,
						TrayId = null,
						ParentJobTrayId = null,
						OrderInJob = totalCount,
						DestinationLocation = GlobalConstants.JOBLOCATION_SEEDER,
						DestinationLayerId = layer.LayerId,
						OrderOnLayer = i,
						RecipeId = null,
						AddedDateTime = job.AddedDateTime,
						TransportLayerId = transportLayer.Id
					};
					await _unitOfWork.JobTray.AddAsync(jt);
				}
			}
			job.TrayCount = totalCount;
		}

		await _unitOfWork.SaveChangesAsync();

	}

	private async Task<Job> AddJobAsync(
		Guid batchId,
		int orderOnDay,
		string name,
		DateTime dt,
		Guid loc,
		Guid status,
		Guid jobTypeId,
		int trayCount,		
		Guid? recipeId,
		bool paused
	)
	{
		var job = new Job
		{
			Id = Guid.NewGuid(),
			BatchId = batchId,
			OrderOnDay = orderOnDay,
			Name = name,
			TrayCount = trayCount,
			ScheduledDate = dt,
			JobLocationTypeId = loc,
			StatusId = status,
			JobTypeId= jobTypeId,
			AddedDateTime = DateTime.UtcNow,
			ModifiedDateTime = DateTime.UtcNow,
			Paused= paused,
		};
		await _unitOfWork.Job.AddAsync(job);

		return job;
	}
}

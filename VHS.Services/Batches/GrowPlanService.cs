using Microsoft.EntityFrameworkCore;
using VHS.Data.Core.Models;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IGrowPlanService
{
    Task<IEnumerable<GrowPlanDTO>> GetAllGrowPlansAsync(
        Guid? productId = null,
        string? name = null,
        Guid? recipeId = null,
        DateOnly? startDateFrom = null,
        DateOnly? startDateTo = null,
        IEnumerable<Guid>? statusIds = null);
    Task<GrowPlanDTO?> GetGrowPlanByIdAsync(Guid id);
    Task<GrowPlanDTO> CreateGrowPlanAsync(GrowPlanDTO configDto);
    Task CreateGrowPlanMultipleAsync(GrowPlanDTO growPlanDTO);
    
    Task UpdateGrowPlanAsync(GrowPlanDTO configDto);
    Task DeleteGrowPlanAsync(Guid id);
    Task<List<BatchRowDTO>> CalculateAssignmentsAsync(Guid rackTypeId, int traysPerDay, int days, bool includeTransportLayer);
    Task StartGrowPlanAsync(GrowPlanDTO GrowPlanDTO);
    Task StartGrowPlanAsync(Guid batchPlanId);
    Task<GrowPlanDTO> DuplicateGrowPlanAsync(Guid id);

    Task StopGrowPlanAsync(Guid batchPlanId, DateOnly endDate);
    Task CancelGrowPlanAsync(Guid batchPlanId);

}

public static class GrowPlanDTOSelect
{
    public static IQueryable<GrowPlanDTO> MapGrowPlanToDTO(this IQueryable<GrowPlan> data)
    {
        var method = System.Reflection.MethodBase.GetCurrentMethod();
        return data.TagWith(method.Name)
            .Select(c => new GrowPlanDTO
            {
                Id = c.Id,
                SetId = c.SetId,
                Name = c.Name,
                FarmId = c.FarmId,
                GrowPlanTypeId = c.GrowPlanTypeId,
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
                    Name = x.Name,
                    FarmId = x.FarmId,
                    GrowPlan = new GrowPlanDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        FarmId = c.FarmId,
                        GrowPlanTypeId = c.GrowPlanTypeId,
                        StartDate = c.StartDate,
                        TraysPerDay = c.TraysPerDay,
                        DaysForPlan = c.DaysForPlan,
                        StatusId = c.StatusId,
                        AddedDateTime = c.AddedDateTime,
                    },
                    Recipe = c.RecipeId.HasValue ? new RecipeDTO
                    {
                        Id = c.RecipeId.Value,
                        Name = c.Recipe.Name,
                        GerminationDays = c.Recipe.GerminationDays,
                        PropagationDays = c.Recipe.PropagationDays,
                        GrowDays = c.Recipe.GrowDays
                    } : null,
                    BatchRows = x.BatchRows.Select(row => new BatchRowDTO
                    {
                        Id = row.Id,
                        BatchId = row.BatchId,
                        FloorId = row.FloorId,
                        RackId = row.RackId,
                        LayerId = row.LayerId,
                        LayerRackTypeId = row.Rack.TypeId,
                        EmptyCount = row.EmptyCount,
                        TrayCount = row.Rack.TrayCountPerLayer,
                    }).ToList() ?? new List<BatchRowDTO>(),
                    HarvestDate = x.HarvestDate,
                    SeedDate = x.SeedDate,
                    TrayCount = x.TrayCount,
                    StatusId = x.StatusId
                }).ToList()
            });
    }
}

public class GrowPlanService : IGrowPlanService
{
    private readonly IUnitOfWorkCore _unitOfWork;
    private readonly IRackService _rackService;
    private readonly IBatchService _batchService;
    private readonly IRecipeService _recipeService;

    public GrowPlanService(
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

    public async Task<IEnumerable<GrowPlanDTO>> GetAllGrowPlansAsync(
        Guid? productId = null,
        string? name = null,
        Guid? recipeId = null,
        DateOnly? startDateFrom = null,
        DateOnly? startDateTo = null,
        IEnumerable<Guid>? statusIds = null)
    {
        IQueryable<GrowPlan> query = _unitOfWork.GrowPlan.Query();

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
            query = query.Where(x => x.StartDate <= startDateTo.Value.AddDays(1));
        }

        if (statusIds != null && statusIds.Any())
        {
            query = query.Where(x => statusIds.Contains(x.StatusId));
        }

        var data = await query
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.AddedDateTime)
            .MapGrowPlanToDTO()
            .AsNoTracking()
            .ToListAsync();

        return data;
    }

    public async Task<GrowPlanDTO?> GetGrowPlanByIdAsync(Guid id)
    {
        var data = await _unitOfWork.GrowPlan.Query(x => x.Id == id)
            .MapGrowPlanToDTO()
            .AsNoTracking()
            .SingleOrDefaultAsync();

        return data;
    }

    public async Task<GrowPlanDTO> CreateGrowPlanAsync(GrowPlanDTO growPlanDTO)
    {
        if (growPlanDTO.Recipe?.Id != null && growPlanDTO.Recipe.Id != Guid.Empty)
        {
            var recipe = await _unitOfWork.Recipe.GetByIdAsync(growPlanDTO.Recipe.Id);
            if (recipe == null)
                throw new Exception("Recipe not found.");
        }
        TimeOnly timeOnly = new TimeOnly(0, 0, 0);

        var growPlan = new GrowPlan
        {
            Id = growPlanDTO.Id == Guid.Empty ? Guid.NewGuid() : growPlanDTO.Id,
            SetId = Guid.NewGuid(),
            Name = growPlanDTO.Name,
            FarmId = growPlanDTO.FarmId,
            RecipeId = growPlanDTO.Recipe?.Id == Guid.Empty ? null : growPlanDTO.Recipe?.Id,
            DaysForPlan = growPlanDTO.DaysForPlan,
            TraysPerDay = growPlanDTO.TraysPerDay,
            StartDate = growPlanDTO.StartDate,
            StatusId = growPlanDTO.StatusId,
            GrowPlanTypeId = growPlanDTO.GrowPlanTypeId,
        };
        await _unitOfWork.GrowPlan.AddAsync(growPlan);

        for (int i = 0; i < growPlan.DaysForPlan; i++)
        {
            var batch = new Batch
            {
                Id = Guid.NewGuid(),
                Name = growPlan.Name,
                GrowPlan = growPlan,
                FarmId = growPlanDTO.FarmId,
                TrayCount = growPlanDTO.TraysPerDay,
                ScheduledDateTime = growPlanDTO.StartDate?.AddDays(i).ToDateTime(timeOnly) ?? DateTime.UtcNow,
                SeedDate = growPlanDTO.StartDate?.AddDays(i),
                HarvestDate = growPlanDTO.Recipe != null ? growPlanDTO.StartDate?.AddDays(growPlanDTO.Recipe.TotalDays + i) : null,
                StatusId = GlobalConstants.BATCHSTATUS_PLANNED,
                AddedDateTime = DateTime.UtcNow
            };
            await _unitOfWork.Batch.AddAsync(batch);
            growPlan.Batches.Add(batch);

            foreach (var rowDto in growPlanDTO.Batches.First().BatchRows.Where(r => r.FloorId.HasValue && r.RackId.HasValue && r.LayerId.HasValue))
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
        }


        await _unitOfWork.SaveChangesAsync();

        return await GetGrowPlanByIdAsync(growPlan.Id);
    }

    public async Task CreateGrowPlanMultipleAsync(GrowPlanDTO growPlanDTO)
    {
        if (growPlanDTO.Recipe?.Id != null && growPlanDTO.Recipe.Id != Guid.Empty)
        {
            var recipe = await _unitOfWork.Recipe.GetByIdAsync(growPlanDTO.Recipe.Id);
            if (recipe == null)
                throw new Exception("Recipe not found.");
        }
        TimeOnly timeOnly = new TimeOnly(0, 0, 0);
        Guid setId = Guid.NewGuid();

        foreach (var batchDTO in growPlanDTO.Batches.OrderBy(x => x.SeedDate))
        {
            var growPlan = new GrowPlan
            {
                Id = growPlanDTO.Id == Guid.Empty ? Guid.NewGuid() : growPlanDTO.Id,
                SetId = setId,
                Name = growPlanDTO.Name,
                FarmId = growPlanDTO.FarmId,
                RecipeId = growPlanDTO.Recipe?.Id == Guid.Empty ? null : (batchDTO.IsEmpty ? null : growPlanDTO.Recipe?.Id),
                DaysForPlan = 1,
                TraysPerDay = growPlanDTO.TraysPerDay,
                StartDate = batchDTO.SeedDate,
                StatusId = growPlanDTO.StatusId,
                GrowPlanTypeId = batchDTO.IsEmpty ? GlobalConstants.BATCHPLANTYPE_RACK : growPlanDTO.GrowPlanTypeId,
            };
            await _unitOfWork.GrowPlan.AddAsync(growPlan);

            var batch = new Batch
            {
                Id = Guid.NewGuid(),
                Name = batchDTO.Name,
                GrowPlanId = growPlan.Id,
                FarmId = batchDTO.FarmId,
                TrayCount = batchDTO.TrayCount,
                ScheduledDateTime = batchDTO.SeedDate.Value.ToDateTime(new TimeOnly(0, 0)),
                SeedDate = batchDTO.SeedDate,
                HarvestDate = batchDTO.HarvestDate,
                StatusId = GlobalConstants.BATCHSTATUS_PLANNED,
                AddedDateTime = DateTime.UtcNow
            };
            await _unitOfWork.Batch.AddAsync(batch);

            foreach (var rowDto in batchDTO.BatchRows.Where(r => r.FloorId.HasValue && r.RackId.HasValue && r.LayerId.HasValue).OrderBy(x => x.Number))
            {
                var newRow = new BatchRow
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    FloorId = rowDto.FloorId,
                    RackId = rowDto.RackId,
                    LayerId = rowDto.LayerId,
                    EmptyCount = rowDto.EmptyCount,
                };
                await _unitOfWork.BatchRow.AddAsync(newRow);
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateGrowPlanAsync(GrowPlanDTO GrowPlanDTO)
    {
        var includes = new string[] { "Recipe.Product", "Batches.BatchRows" };
        var plan = await _unitOfWork.GrowPlan.GetByIdWithIncludesAsync(GrowPlanDTO.Id, includeProperties: includes);
        if (plan == null)
            throw new Exception("Batch plan not found");

        plan.Name = GrowPlanDTO.Name;
        plan.FarmId = GrowPlanDTO.FarmId;
        plan.RecipeId = GrowPlanDTO.Recipe?.Id == Guid.Empty ? null : GrowPlanDTO.Recipe?.Id;
        plan.StartDate = GrowPlanDTO.StartDate;
        plan.DaysForPlan = GrowPlanDTO.DaysForPlan;
        plan.TraysPerDay = GrowPlanDTO.TraysPerDay;
        plan.GrowPlanTypeId = GrowPlanDTO.GrowPlanTypeId;
        plan.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.GrowPlan.Update(plan);

        if (GrowPlanDTO.Batches.Any() != null)
        {
            await _batchService.UpdateBatchAsync(GrowPlanDTO.Batches.First());
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteGrowPlanAsync(Guid id)
    {
        var config = await _unitOfWork.GrowPlan.GetByIdAsync(id);
        if (config == null)
            throw new Exception("Batch configuration not found");

        var dt = DateTime.UtcNow;

        config.DeletedDateTime = dt;

        foreach (var batch in config.Batches)
        {
            batch.DeletedDateTime = dt;
            foreach (var job in batch.Jobs)
            {
                job.DeletedDateTime = dt;
            }
        }

        _unitOfWork.GrowPlan.Update(config);
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

    public async Task StartGrowPlanAsync(Guid batchPlanId)
    {
        var dto = await GetGrowPlanByIdAsync(batchPlanId)
            ?? throw new Exception("Plan not found");
        await StartGrowPlanAsync(dto);
    }

    public async Task StartGrowPlanAsync(GrowPlanDTO growPlan)
    {
        if (growPlan == null) throw new ArgumentNullException(nameof(growPlan));

        var plan = await _unitOfWork.GrowPlan.GetByIdAsync(growPlan.Id)
            ?? throw new Exception("Plan not found");

        plan.StatusId = GlobalConstants.BATCHPLANSTATUS_PLANNED;
        plan.ModifiedDateTime = DateTime.UtcNow;
        _unitOfWork.GrowPlan.Update(plan);

        RecipeDTO recipe = null;

        if (growPlan.Recipe != null)
            recipe = await _recipeService.GetRecipeByIdAsync(growPlan.Recipe.Id)
                ?? throw new Exception("Recipe not found for batch plan.");

        foreach (var batch in plan.Batches)
        {
            var seedDate = batch.SeedDate!.Value;
            batch.ModifiedDateTime = DateTime.UtcNow;
            _unitOfWork.Batch.Update(batch);

            var allBatchRows = batch.BatchRows?.ToList() ?? new List<BatchRow>();
            var germinationLayers = allBatchRows.Where(r => r.Rack?.TypeId == GlobalConstants.RACKTYPE_GERMINATION).OrderBy(x => x.Rack.Number).ThenBy(x => x.Layer.Number).ToList();
            var propagationLayers = allBatchRows.Where(r => r.Rack?.TypeId == GlobalConstants.RACKTYPE_PROPAGATION).OrderBy(x => x.Rack.Number).ThenBy(x => x.Layer.Number).ToList();
            var growLayers = allBatchRows.Where(r => r.Rack?.TypeId == GlobalConstants.RACKTYPE_GROWING).OrderBy(x => x.Rack.Number).ThenBy(x => x.Layer.Number).ToList();
            var totalCount = 0;


			if (growPlan.GrowPlanTypeId == GlobalConstants.BATCHPLANTYPE_RACK)
			{
				var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(GlobalConstants.JOBLOCATION_SEEDER, seedDate);

				var job = await AddJobAsync(
					batch.Id,
					newOrderOnDay,
					$"Manual Rack Transport Job ({growPlan.Name})",
					dt: seedDate,
					loc: GlobalConstants.JOBLOCATION_SEEDER,
					jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TORACK,
					status: GlobalConstants.JOBSTATUS_NOTSTARTED,
					trayCount: growPlan.TraysPerDay,
					recipeId: null,
					paused: true
				);

				totalCount = 0;
				int trayAssigned = 0;

				foreach (var layer in allBatchRows.OrderBy(r => r.Rack.Number).ThenBy(r => r.Layer.Number))
				{
					var transportLayer = layer.Rack?.Layers.OrderBy(l => l.Number).LastOrDefault();
					if (transportLayer == null)
						continue;

					for (int i = 1; i <= layer.Rack.TrayCountPerLayer && trayAssigned < growPlan.TotalTrays; i++)
					{
						trayAssigned++;
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
							RecipeId = null,
							AddedDateTime = job.AddedDateTime,
							TransportLayerId = transportLayer.Id
						};

						await _unitOfWork.JobTray.AddAsync(jt);
					}

					if (trayAssigned >= growPlan.TotalTrays)
						break;
				}
				job.TrayCount = totalCount;
			} else if (growPlan.GrowPlanTypeId == GlobalConstants.BATCHPLANTYPE_RECIPE)
            {
                var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(GlobalConstants.JOBLOCATION_SEEDER, seedDate);

                // Seeding Job
                var seedingJob = await AddJobAsync(
                    batch.Id,
                    newOrderOnDay, $"Seeding Job ({growPlan.Name})",
                    dt: seedDate,
                    loc: GlobalConstants.JOBLOCATION_SEEDER,
                    jobTypeId: recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_LETTUCE
                        ? GlobalConstants.JOBTYPE_SEEDING_PROPAGATION
                        : GlobalConstants.JOBTYPE_SEEDING_GERMINATION,
                    status: GlobalConstants.JOBSTATUS_NOTSTARTED,
                    recipeId: growPlan.Recipe.Id,
                    trayCount: growPlan.TraysPerDay,
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
                                RecipeId = isEmpty ? null : growPlan.Recipe.Id,
                                AddedDateTime = seedingJob.AddedDateTime,
                                TransportLayerId = transportLayer.Id
                            };
                            await _unitOfWork.JobTray.AddAsync(jtPropagation);
                        }
                    }
                    seedingJob.TrayCount = totalCount;

                    var propagationDate = seedDate.AddDays(recipe.PropagationDays);
                    newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(GlobalConstants.JOBLOCATION_SEEDER, propagationDate);
                    var emptyForTransplant = await AddJobAsync(
                        batch.Id, 
                        newOrderOnDay, 
                        $"Transplant Job ({growPlan.Name})",
                        dt: propagationDate,
                        loc: GlobalConstants.JOBLOCATION_SEEDER,
                        jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TOTRANSPLANT,
                        status: GlobalConstants.JOBSTATUS_NOTSTARTED,
                        trayCount: growPlan.TraysPerDay,
                        recipeId: growPlan.Recipe.Id,
                        paused: true
                    );
                    var harvestDate = seedDate.AddDays(recipe.PropagationDays).AddDays(recipe.GrowDays);
                    newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(GlobalConstants.JOBLOCATION_HARVESTER, harvestDate);
                    var harvestingJob = await AddJobAsync(
                        batch.Id, newOrderOnDay, $"Harvesting Job ({growPlan.Name})",
                        dt: harvestDate,
                        loc: GlobalConstants.JOBLOCATION_HARVESTER,
                        jobTypeId: GlobalConstants.JOBTYPE_HARVESTING,
                        status: GlobalConstants.JOBSTATUS_NOTSTARTED,
                        trayCount: growPlan.TraysPerDay,
                        recipeId: growPlan.Recipe.Id,
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
                            var jtTransplant = new JobTray
                            {
                                Id = Guid.NewGuid(),
                                JobId = emptyForTransplant.Id,
                                TrayId = null,
                                ParentJobTrayId = null,
                                OrderInJob = totalCount,
                                DestinationLocation = GlobalConstants.JOBLOCATION_SEEDER,
								DestinationLayerId = growLayer.LayerId,
								RecipeId = isEmpty ? null : growPlan.Recipe.Id,
								AddedDateTime = emptyForTransplant.AddedDateTime,
                                TransportLayerId = transportLayer.Id
							};
                            var jtHarvest = new JobTray
                            {
                                Id = Guid.NewGuid(),
                                JobId = harvestingJob.Id,
                                TrayId = null,
                                ParentJobTrayId = jtTransplant.Id,
                                OrderInJob = totalCount,
                                DestinationLocation = GlobalConstants.JOBLOCATION_HARVESTER,
                                DestinationLayerId = growLayer.LayerId,
                                RecipeId = isEmpty ? null : growPlan.Recipe.Id,
                                AddedDateTime = harvestingJob.AddedDateTime,
                                TransportLayerId = null
                            };
                            await _unitOfWork.JobTray.AddAsync(jtHarvest);
                            await _unitOfWork.JobTray.AddAsync(jtTransplant);
                        }
                    }
                }
                else if (recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_MICROGREENS || recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_PETITEGREENS)
                {
                    Queue<Guid> filledTrays = new Queue<Guid>();

                    var harvestDate = seedDate.AddDays(recipe.GerminationDays).AddDays(recipe.PropagationDays).AddDays(recipe.GrowDays);
                    newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(GlobalConstants.JOBLOCATION_HARVESTER, harvestDate);
                    var harvestingJob = await AddJobAsync(
                        batch.Id, newOrderOnDay, $"Harvesting Job ({growPlan.Name})",
                        dt: harvestDate,
                        loc: GlobalConstants.JOBLOCATION_HARVESTER,
                        jobTypeId: GlobalConstants.JOBTYPE_HARVESTING,
                        status: GlobalConstants.JOBSTATUS_NOTSTARTED,
                        trayCount: growPlan.TraysPerDay,
                        recipeId: growPlan.Recipe.Id,
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
                                RecipeId = isEmpty ? null : growPlan.Recipe.Id,
                                AddedDateTime = seedingJob.AddedDateTime,
                                TransportLayerId = transportLayer.Id
                            };
                            if (jtGermination.RecipeId.HasValue) filledTrays.Enqueue(jtGermination.Id);
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
                                ParentJobTrayId = isEmpty ? null : filledTrays.Dequeue(),
                                OrderInJob = totalCount,
                                DestinationLocation = GlobalConstants.JOBLOCATION_HARVESTER,
                                DestinationLayerId = growLayer.LayerId,
                                RecipeId = isEmpty ? null : growPlan.Recipe.Id,
                                AddedDateTime = harvestingJob.AddedDateTime,
                                TransportLayerId = transportLayer.Id
                            };
                            await _unitOfWork.JobTray.AddAsync(jtHarvest);
                        }
                    }
                }
                // Empty Trays to Washing or Transplanting Job
            }
            else if (growPlan.GrowPlanTypeId == GlobalConstants.BATCHPLANTYPE_WASHER)
            {
                var newOrderOnDay = await _unitOfWork.Job.GetNextJobOrderOnDay(GlobalConstants.JOBLOCATION_SEEDER, seedDate);
                var job = await AddJobAsync(
                    batch.Id,
                    newOrderOnDay,
                    $"Empty Trays to Washing Job ({growPlan.Name})",
                    dt: seedDate,
                    loc: GlobalConstants.JOBLOCATION_SEEDER,
                    jobTypeId: GlobalConstants.JOBTYPE_EMPTY_TOWASHER,
                    status: GlobalConstants.JOBSTATUS_NOTSTARTED,
                    trayCount: growPlan.TraysPerDay,
                    recipeId: null,
                    paused: true
                );

                for (int i = 1; i <= growPlan.TraysPerDay; i++)
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
                        RecipeId = null,
                        AddedDateTime = job.AddedDateTime
                    };
                    await _unitOfWork.JobTray.AddAsync(jt);
                }
            }
       
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task<Job> AddJobAsync(
        Guid batchId,
        int orderOnDay,
        string name,
        DateOnly dt,
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
            JobTypeId = jobTypeId,
            AddedDateTime = DateTime.UtcNow,
            ModifiedDateTime = DateTime.UtcNow,
            Paused = paused,
        };
        await _unitOfWork.Job.AddAsync(job);

        return job;
    }

    public async Task<GrowPlanDTO> DuplicateGrowPlanAsync(Guid id)
    {
        var originalPlanDto = await GetGrowPlanByIdAsync(id)
            ?? throw new Exception("Original batch plan not found for duplication.");

        originalPlanDto.Id = Guid.Empty;
        originalPlanDto.Name = $"{originalPlanDto.Name} (Copy)";
        originalPlanDto.StartDate = DateOnly.FromDateTime(DateTime.Today);
        originalPlanDto.StatusId = GlobalConstants.BATCHPLANSTATUS_NEW;

        if (originalPlanDto.Batches.Any())
        {
            foreach (var batch in originalPlanDto.Batches)
            {
                batch.Id = Guid.Empty;
                batch.Name = originalPlanDto.Name;
                batch.StatusId = GlobalConstants.BATCHSTATUS_PLANNED;
                foreach (var row in batch.BatchRows)
                {
                    row.Id = Guid.Empty;
                }
            }
        }

        return await CreateGrowPlanAsync(originalPlanDto);
    }

    public async Task StopGrowPlanAsync(Guid batchPlanId, DateOnly endDate)
    {
        var batchPlan = await _unitOfWork.GrowPlan
            .Query(bp => bp.Id == batchPlanId, includeProperties: new[] { "Batches.Jobs" })
            .SingleOrDefaultAsync();

        if (batchPlan == null)
            throw new Exception("Batch plan not found");

        if (batchPlan.StatusId != GlobalConstants.BATCHPLANSTATUS_ACTIVE && batchPlan.StatusId != GlobalConstants.BATCHPLANSTATUS_PLANNED)
            throw new InvalidOperationException("Can only stop active or planned batch plans");

        batchPlan.StatusId = GlobalConstants.BATCHPLANSTATUS_FINISHED;
        batchPlan.ModifiedDateTime = DateTime.UtcNow;

        var jobsToCancel = batchPlan.Batches
            .SelectMany(b => b.Jobs)
            .Where(j => j.DeletedDateTime == null &&
                       j.ScheduledDate > endDate &&
                       j.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
            .ToList();

        foreach (var job in jobsToCancel)
        {
            job.StatusId = GlobalConstants.JOBSTATUS_CANCELLED;
            job.ModifiedDateTime = DateTime.UtcNow;
            _unitOfWork.Job.Update(job);
        }

        _unitOfWork.GrowPlan.Update(batchPlan);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelGrowPlanAsync(Guid batchPlanId)
    {
        var batchPlan = await _unitOfWork.GrowPlan
            .Query(bp => bp.Id == batchPlanId, includeProperties: new[] { "Batches.Jobs" })
            .SingleOrDefaultAsync();

        if (batchPlan == null)
            throw new Exception("Batch plan not found");

        var runningJobs = batchPlan.Batches
            .SelectMany(b => b.Jobs)
            .Where(j => j.DeletedDateTime == null && j.StatusId == GlobalConstants.JOBSTATUS_INPROGRESS)
            .ToList();

        if (runningJobs.Any())
            throw new InvalidOperationException("Cannot cancel batch plan with running jobs. Please complete or pause the running jobs first.");

        batchPlan.StatusId = GlobalConstants.BATCHPLANSTATUS_FINISHED;
        batchPlan.ModifiedDateTime = DateTime.UtcNow;

        var jobsToCancel = batchPlan.Batches
            .SelectMany(b => b.Jobs)
            .Where(j => j.DeletedDateTime == null && j.StatusId != GlobalConstants.JOBSTATUS_COMPLETED)
            .ToList();

        foreach (var job in jobsToCancel)
        {
            job.StatusId = GlobalConstants.JOBSTATUS_CANCELLED;
            job.ModifiedDateTime = DateTime.UtcNow;
            _unitOfWork.Job.Update(job);
        }

        foreach (var batch in batchPlan.Batches)
        {
            batch.StatusId = GlobalConstants.BATCHSTATUS_CANCELLED;
            batch.ModifiedDateTime = DateTime.UtcNow;
            _unitOfWork.Batch.Update(batch);
        }

        _unitOfWork.GrowPlan.Update(batchPlan);
        await _unitOfWork.SaveChangesAsync();
    }
}

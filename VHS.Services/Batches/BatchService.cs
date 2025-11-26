using Microsoft.EntityFrameworkCore;
using VHS.Services.Batches.DTO;

namespace VHS.Services;

public interface IBatchService
{
    Task<IEnumerable<BatchDTO>> GetAllBatchesAsync(
        Guid? farmId = null,
        string? batchName = null,
        string? planName = null,
        DateTime? seedDateFrom = null,
        DateTime? seedDateTo = null,
        DateTime? harvestDateFrom = null,
        DateTime? harvestDateTo = null);
    Task<BatchDTO?> GetBatchByIdAsync(Guid id);
    Task<BatchDTO> CreateBatchAsync(BatchDTO batchDto);
    Task UpdateBatchAsync(BatchDTO batchDto);
    Task DeleteBatchAsync(Guid id);
    Task<string> GenerateBatchIdAsync(DateOnly seededDate);

    Task UpdateLotReference(Guid batchId, Guid jobId, string lotReference);

    Task<IEnumerable<BatchDTO>> GetActiveBatches(Guid farmId, DateOnly startDate);
}

public class BatchService : IBatchService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public BatchService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static BatchDTO SelectBatchToDTO(Batch b) => new BatchDTO
    {
        Id = b.Id,
        Name = b.Name,
        FarmId = b.FarmId,
        SeedDate = b.SeedDate,
        HarvestDate = b.HarvestDate,
        StatusId = b.StatusId,
        TrayCount = b.TrayCount,
        LotReference = b.LotReference,
        GrowPlan = b.GrowPlan != null ? new GrowPlanDTO
        {
            Id = b.GrowPlan.Id,
            Name = b.GrowPlan.Name,
            FarmId = b.GrowPlan.FarmId,
            Recipe = b.GrowPlan.Recipe != null ? new Produce.DTO.RecipeDTO
            {
                Id = b.GrowPlan.Recipe.Id,
                Name = b.GrowPlan.Recipe.Name,
                GerminationDays = b.GrowPlan.Recipe.GerminationDays,
                PropagationDays = b.GrowPlan.Recipe.PropagationDays,
                GrowDays = b.GrowPlan.Recipe.GrowDays,
            } : null,
        } : new GrowPlanDTO(),
        BatchRows = b.BatchRows?.Select(row => new BatchRowDTO
        {
            Id = row.Id,
            BatchId = row.BatchId,
            FloorId = row.FloorId,
            RackId = row.RackId,
            LayerId = row.LayerId,
            LayerRackTypeId = row.Rack?.TypeId ?? Guid.Empty,
            EmptyCount = row.EmptyCount,
            TrayCount = row.Rack.TrayCountPerLayer,
        }).ToList() ?? new List<BatchRowDTO>()
    };

    public async Task<IEnumerable<BatchDTO>> GetActiveBatches(Guid farmId, DateOnly startDate)
    {
        var includes = new string[] { "GrowPlan.Recipe.Product", "BatchRows.Rack" };
        IQueryable<Batch> query = _unitOfWork.Batch.Query(includeProperties: includes);
        query = query.Where(x => x.FarmId == farmId);
        query = query.Where(x => x.SeedDate >= startDate);
        var batches = await query.ToListAsync();
        return batches.Select(SelectBatchToDTO);
    }

    public async Task<IEnumerable<BatchDTO>> GetAllBatchesAsync(
        Guid? farmId = null,
        string? batchName = null,
        string? planName = null,
        DateTime? seedDateFrom = null,
        DateTime? seedDateTo = null,
        DateTime? harvestDateFrom = null,
        DateTime? harvestDateTo = null)
    {
        var includes = new string[] { "GrowPlan.Recipe", "BatchRows.Rack" };
        IQueryable<Batch> query = _unitOfWork.Batch.Query(includeProperties: includes);

        if (farmId.HasValue && farmId.Value != Guid.Empty)
        {
            query = query.Where(x => x.FarmId == farmId);
        }

        if (!string.IsNullOrWhiteSpace(batchName))
        {
            query = query.Where(x => x.Name.Contains(batchName));
        }

        if (!string.IsNullOrWhiteSpace(planName))
        {
            query = query.Where(x => x.GrowPlan != null && x.GrowPlan.Name.Contains(planName));
        }

        if (seedDateFrom.HasValue)
        {
            query = query.Where(x => x.SeedDate >= DateOnly.FromDateTime(seedDateFrom.Value.Date));
        }

        if (seedDateTo.HasValue)
        {
            query = query.Where(x => x.SeedDate <= DateOnly.FromDateTime(seedDateTo.Value.Date.AddDays(1)));
        }

        if (harvestDateFrom.HasValue)
        {
            query = query.Where(x => x.HarvestDate >= DateOnly.FromDateTime(harvestDateFrom.Value.Date));
        }

        if (harvestDateTo.HasValue)
        {
            query = query.Where(x => x.HarvestDate <= DateOnly.FromDateTime(harvestDateTo.Value.Date.AddDays(1)));
        }

        var batches = await query.ToListAsync();
        return batches.Select(SelectBatchToDTO);
    }

    public async Task<BatchDTO?> GetBatchByIdAsync(Guid id)
    {
        var includes = new string[] { "GrowPlan.Recipe.Product", "BatchRows.Rack" };
        var batch = await _unitOfWork.Batch.GetByIdWithIncludesAsync(id, includes);
        if (batch == null) return null;

        return SelectBatchToDTO(batch);
    }

    public async Task<BatchDTO> CreateBatchAsync(BatchDTO batchDto)
    {
        DateOnly seededDate = batchDto.SeedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        string generatedBatchId = await GenerateBatchIdAsync(seededDate);

        var batch = new Batch
        {
            Id = batchDto.Id == Guid.Empty ? Guid.NewGuid() : batchDto.Id,
            Name = batchDto.Name ?? generatedBatchId,
            FarmId = batchDto.FarmId,
            GrowPlanId = batchDto.GrowPlan.Id,
            SeedDate = batchDto.SeedDate,
            HarvestDate = batchDto.HarvestDate,
            StatusId = batchDto.StatusId,
            TrayCount = batchDto.TrayCount,
            LotReference = batchDto.LotReference,
            BatchRows = batchDto.BatchRows?.Select(rowDto => new BatchRow
            {
                Id = Guid.NewGuid(),
                FloorId = rowDto.FloorId,
                RackId = rowDto.RackId,
                LayerId = rowDto.LayerId
            }).ToList() ?? new List<BatchRow>()
        };

        await _unitOfWork.Batch.AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        return await GetBatchByIdAsync(batch.Id);
    }

    public async Task UpdateBatchAsync(BatchDTO batchDto)
    {
        var batch = await _unitOfWork.Batch
            .Query(b => b.Id == batchDto.Id, includeProperties: new[] { "BatchRows", "Jobs" })
            .SingleOrDefaultAsync();

        if (batch == null)
            throw new Exception("Batch not found");

        batch.Name = batchDto.Name;
        if (batchDto.StatusId == GlobalConstants.BATCHSTATUS_CANCELLED)
        {
            var runningJobs = batch.Jobs?.Where(j => j.DeletedDateTime == null && 
                j.StatusId == GlobalConstants.JOBSTATUS_INPROGRESS)
                .ToList();
            
            if (runningJobs?.Any() == true)
            {
                throw new InvalidOperationException("Cannot cancel batch with running jobs. Please complete or pause the running jobs first.");
            }
        }

        batch.Name = batchDto.Name;
        batch.SeedDate = batchDto.SeedDate;
        batch.HarvestDate = batchDto.HarvestDate;
        batch.StatusId = batchDto.StatusId;
        batch.TrayCount = batchDto.TrayCount;
        batch.ModifiedDateTime = DateTime.UtcNow;
        batch.LotReference = batchDto.LotReference;

        var rowsToRemove = batch.BatchRows
            .Where(dbRow => dbRow.DeletedDateTime == null)
            .Where(dbRow => !batchDto.BatchRows.Any(dtoRow => dtoRow.Id == dbRow.Id))
            .ToList();

        foreach (var row in rowsToRemove)
        {
            row.DeletedDateTime = DateTime.UtcNow;
            _unitOfWork.BatchRow.Update(row);
        }

        foreach (var rowDto in batchDto.BatchRows)
        {
            var existingRow = batch.BatchRows
                .SingleOrDefault(r => r.Id == rowDto.Id && r.DeletedDateTime == null);

            if (existingRow != null)
            {
                existingRow.FloorId = rowDto.FloorId;
                existingRow.RackId = rowDto.RackId;
                existingRow.LayerId = rowDto.LayerId;
                existingRow.EmptyCount = rowDto.EmptyCount;
                _unitOfWork.BatchRow.Update(existingRow);
            }
            else if (rowDto.Id == Guid.Empty)
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
            }
        }

        _unitOfWork.Batch.Update(batch);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteBatchAsync(Guid id)
    {
        var batch = await _unitOfWork.Batch.GetByIdAsync(id);
        if (batch == null)
            throw new Exception("Batch not found");

        batch.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.Batch.Update(batch);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a human-readable BatchId in the format "BATCH-YYYYMMDD-XXXXXX"
    /// by checking existing batches for the given seeded date and incrementing the sequence.
    /// </summary>
    public async Task<string> GenerateBatchIdAsync(DateOnly seededDate)
    {
        string prefix = "BATCH";
        string datePart = seededDate.ToString("yyyyMMdd");
        int nextSequence = 1;

        // Get batches that were seeded on the same date
        var batchesForDate = await _unitOfWork.Batch.Query()
            .Where(b => b.SeedDate.HasValue && b.SeedDate.Value == seededDate)
            .ToListAsync();

        if (batchesForDate.Any())
        {
            var sequenceNumbers = batchesForDate.Select(b =>
            {
                // Ensure that BatchId is not null and follows the expected format "BATCH-YYYYMMDD-XXXXXX"
                if (!string.IsNullOrWhiteSpace(b.Name))
                {
                    string[] parts = b.Name.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int seq))
                        return seq;
                }
                return 0;
            });

            nextSequence = sequenceNumbers.Max() + 1;
        }

        // Build the BatchId using the prefix, date part, and zero-padded sequence number (6 digits minimum)
        string generatedBatchId = $"{prefix}-{datePart}-{nextSequence:D4}";
        return generatedBatchId;
    }

    public async Task UpdateLotReference(Guid batchId, Guid jobId, string lotReference)
    {
        var batch = await _unitOfWork.Batch.Query(b => b.Id == batchId).SingleOrDefaultAsync();

        if (batch == null)
            throw new Exception("Batch not found");

        batch.LotReference = lotReference;

        var job = await _unitOfWork.Job.Query(j => j.Id == jobId).SingleOrDefaultAsync();
        if (job == null)
            throw new Exception("Job not found");
        job.Paused = false;

        await _unitOfWork.SaveChangesAsync();
    }
}

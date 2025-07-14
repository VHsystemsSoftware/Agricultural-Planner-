using VHS.Services.Batches.DTO;

namespace VHS.Services;

public interface IBatchRowService
{
    Task<IEnumerable<BatchRowDTO>> GetBatchRowsByBatchAsync(Guid batchId);
    Task CreateBatchRowAsync(BatchRowDTO dto);
}

public class BatchRowService : IBatchRowService
{
    private readonly IUnitOfWorkCore _unitOfWork;

    public BatchRowService(IUnitOfWorkCore unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<BatchRowDTO>> GetBatchRowsByBatchAsync(Guid batchId)
    {
        var entities = await _unitOfWork.BatchRow
            .GetAllAsync(r => r.BatchId == batchId, includeProperties: new[] { "Rack", "Rack.Floor" });
        return entities.Select(r => new BatchRowDTO
        {
            Id = r.Id,
            BatchId = r.BatchId,
            FloorId = r.FloorId,
            RackId = r.RackId,
            LayerId = r.LayerId,
            TrayCount = r.Rack?.TrayCountPerLayer ?? 0,
            AddedDateTime = r.AddedDateTime,
            LayerRackTypeId = r.Rack?.TypeId ?? Guid.Empty,
        });
    }

    public async Task CreateBatchRowAsync(BatchRowDTO dto)
    {
        var entity = new BatchRow
        {
            Id = Guid.NewGuid(),
            BatchId = dto.BatchId,
            FloorId = dto.FloorId,
            RackId = dto.RackId,
            LayerId = dto.LayerId,
            AddedDateTime = DateTime.UtcNow
        };
        await _unitOfWork.BatchRow.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}

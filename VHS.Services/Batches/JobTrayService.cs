using VHS.Services.Batches.DTO;

namespace VHS.Services;

public interface IJobTrayService
{
	Task<IEnumerable<JobTrayDTO>> GetAllByJobTrayIdAsync(Guid jobId);
	Task<JobTrayDTO?> GetByIdAsync(Guid id);
	Task<JobTrayDTO> CreateJobTrayAsync(Guid jobId, JobTrayDTO dto);
	//Task UpdateJobTrayAsync(JobTrayDTO dto);
}

public class JobTrayService : IJobTrayService
{
	private readonly IUnitOfWorkCore _unitOfWork;

	public JobTrayService(IUnitOfWorkCore unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	private static JobTrayDTO SelectToDTO(JobTray jt) => new JobTrayDTO
	{
		Id = jt.Id,
		TrayId = jt.TrayId,
		DestinationLocation = jt.DestinationLocation,
		DestinationLayerId = jt.DestinationLayerId,
		DestinationLayer = jt.DestinationLayerId.HasValue ? new Farming.DTO.LayerDTO()
		{
			Id = jt.DestinationLayerId.Value,
			Number = jt.DestinationLayer.Number,
  			    TrayCountPerLayer = jt.DestinationLayer.Rack.TrayCountPerLayer,
			RackId = jt.DestinationLayer.RackId,
			Name = $"{jt.DestinationLayer.Rack.Floor.Name}-{jt.DestinationLayer.Rack.Name}-{jt.DestinationLayer.Number}"
		} : null,
		OrderInJob = jt.OrderInJob,
		TransportLayerId= jt.TransportLayerId,
		RecipeId = jt.RecipeId,
		RecipeName = jt.RecipeId.HasValue ? jt.Recipe?.Name : string.Empty,
		AddedDateTime = jt.AddedDateTime,
	};

	public async Task<IEnumerable<JobTrayDTO>> GetAllByJobTrayIdAsync(Guid jobId)
	{
		var trays = await _unitOfWork.JobTray.GetAllAsync(jt =>
			jt.JobId == jobId, includeProperties: "Recipe, DestinationLayer");

		return trays
			.OrderBy(jt => jt.OrderInJob)
			.Select(SelectToDTO);
	}

	public async Task<JobTrayDTO?> GetByIdAsync(Guid id)
	{
		var tray = await _unitOfWork.JobTray.GetByIdAsync(id);
		return tray == null ? null : SelectToDTO(tray);
	}

	public async Task<JobTrayDTO> CreateJobTrayAsync(Guid jobId, JobTrayDTO dto)
	{
		var entity = new JobTray
		{
			Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
			JobId = jobId,
			TrayId = dto.TrayId,
			DestinationLocation = dto.DestinationLocation,
			DestinationLayerId = dto.DestinationLayerId,
			RecipeId = dto.RecipeId,
			OrderInJob = dto.OrderInJob,
			AddedDateTime = dto.AddedDateTime == default ? DateTime.UtcNow : dto.AddedDateTime
		};

		await _unitOfWork.JobTray.AddAsync(entity);
		await _unitOfWork.SaveChangesAsync();

		var result = SelectToDTO(entity);
		return result;
	}

	//public async Task UpdateJobTrayAsync(JobTrayDTO dto)
	//{
	//	var entity = await _unitOfWork.JobTray.GetByIdAsync(dto.Id);
	//	if (entity == null)
	//		throw new Exception("JobTray not found");

	//	entity.JobId = dto.Job.Id;
	//	entity.TrayId = dto.TrayId;
	//	entity.DestinationLocation = dto.DestinationLocation;
	//	entity.DestinationLayerId = dto.DestinationLayerId;
	//	entity.RecipeId = dto.RecipeId;
	//	entity.OrderInJob = dto.OrderInJob;
	//	entity.AddedDateTime = dto.AddedDateTime;

	//	_unitOfWork.JobTray.Update(entity);
	//	await _unitOfWork.SaveChangesAsync();
	//}
}

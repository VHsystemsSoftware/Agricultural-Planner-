using VHS.Services.Farming.DTO;

namespace VHS.Services;

public interface IFloorService
{
	Task<IEnumerable<FloorDTO>> GetAllFloorsAsync(Guid? farmId, bool enabledOnly);
	Task<FloorDTO?> GetFloorByIdAsync(Guid id);
	Task<FloorDTO> CreateFloorAsync(FloorDTO floorDto);
	Task UpdateFloorAsync(FloorDTO floorDto);
	Task UpdateFloorEnabledAsync(EnabledDTO enabledDto);
	Task DeleteFloorAsync(Guid id);
}

public class FloorService : IFloorService
{
	private readonly IUnitOfWorkCore _unitOfWork;

	public FloorService(IUnitOfWorkCore unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	private static FloorDTO SelectFloorToDTO(Floor f) => new FloorDTO
	{
		Id = f.Id,
		Name = f.Name,
		Number = f.Number,
		FarmId = f.FarmId,
		Enabled = f.Enabled,
		Racks = f.Racks.Select(x => 
			new RackDTO() 
			{
				Name = x.Name,
				TypeId = x.TypeId,
				Number=x.Number,
				Id = x.Id,
				LayerCount = x.LayerCount,
				TrayCountPerLayer = x.TrayCountPerLayer,
				Enabled = x.Enabled,
				Layers = x.Layers.Select(l => new LayerDTO
				{
					Id = l.Id,
					RackId = l.RackId,
					Number = l.Number,
					Enabled = l.Enabled
				}).ToList()
			}).ToList()
	};

	public async Task<IEnumerable<FloorDTO>> GetAllFloorsAsync(Guid? farmId = null, bool enabledOnly = false)
	{
		IEnumerable<Floor> floors;

		if (farmId.HasValue && farmId.Value != Guid.Empty)
		{
			floors = await _unitOfWork.Floor.GetAllAsync(x =>
				x.FarmId == farmId.Value &&
				(!enabledOnly || x.Enabled));
		}
		else
		{
			floors = await _unitOfWork.Floor.GetAllAsync(x =>
				!enabledOnly || x.Enabled);
		}

		return floors
			.OrderBy(f => f.Name)
			.Select(SelectFloorToDTO);
	}

	public async Task<FloorDTO?> GetFloorByIdAsync(Guid id)
	{
		var floor = await _unitOfWork.Floor.GetByIdAsync(id);
		if (floor == null)
			return null;

		return SelectFloorToDTO(floor);
	}

	public async Task<FloorDTO> CreateFloorAsync(FloorDTO floorDto)
	{
		var floor = new Floor
		{
			Id = floorDto.Id == Guid.Empty ? Guid.NewGuid() : floorDto.Id,
			FarmId = floorDto.FarmId,
			Name = floorDto.Name
		};

		await _unitOfWork.Floor.AddAsync(floor);
		await _unitOfWork.SaveChangesAsync();

		return await GetFloorByIdAsync(floor.Id);
	}

	public async Task UpdateFloorAsync(FloorDTO floorDto)
	{
		var floor = await _unitOfWork.Floor.GetByIdAsync(floorDto.Id);
		if (floor == null)
			throw new Exception("Floor not found");

		floor.Name = floorDto.Name;
		floor.FarmId = floorDto.FarmId;
		floor.Enabled = floorDto.Enabled;
		_unitOfWork.Floor.Update(floor);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task UpdateFloorEnabledAsync(EnabledDTO enabledDto)
	{
		var floor = await _unitOfWork.Floor.GetByIdAsync(enabledDto.Id);
		if (floor == null)
			throw new KeyNotFoundException("Rack not found");
		floor.Enabled = enabledDto.Enabled;
		_unitOfWork.Floor.Update(floor);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task DeleteFloorAsync(Guid id)
	{
		var floor = await _unitOfWork.Floor.GetByIdAsync(id);
		if (floor == null)
			throw new Exception("Floor not found");

		floor.DeletedDateTime = DateTime.UtcNow;
		_unitOfWork.Floor.Update(floor);
		await _unitOfWork.SaveChangesAsync();
	}
}

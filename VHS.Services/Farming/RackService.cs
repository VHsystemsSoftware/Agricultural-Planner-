using Microsoft.EntityFrameworkCore;
using VHS.Services.Farming.DTO;

namespace VHS.Services;

public interface IRackService
{
	Task<IEnumerable<RackDTO>> GetAllRacksByTypeAsync(Guid farmId, Guid typeId);
	Task<RackDTO?> GetRackByIdAsync(Guid id);
	Task<List<RackDTO>> GetRacksByTypeIdAsync(Guid typeId);
	Task<IEnumerable<RackDTO>> GetAllRacksAsync(Guid farmId);
	Task<RackDTO> CreateRackAsync(RackDTO rackDto);
	Task UpdateRackAsync(RackDTO rackDto);
	Task UpdateRackEnabledAsync(EnabledDTO enabledDto);

	Task DeleteRackAsync(Guid id);
	Task InsertRacksForFarmAsync(Guid farmId, List<Floor> floors);

	Task<Layer> GetBufferLayer(Guid rackId);
}
public class RackService : IRackService
{
	private readonly IUnitOfWorkCore _unitOfWork;

	public RackService(IUnitOfWorkCore unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	private static RackDTO SelectRackToDTO(Rack r) => new RackDTO(r.Id, r.LayerCount)
	{
		Id = r.Id,
		Name = r.Name,
		Number = r.Number,
		Enabled = r.Enabled,
		Floor = new FloorDTO()
		{
			Id = r.Floor.Id,
			Name = r.Floor.Name,
			FarmId = r.Floor.FarmId,
			Enabled = r.Floor.Enabled,
			Number = r.Floor.Number,
		},
		TypeId = r.TypeId,
		LayerCount = r.LayerCount,
		TrayCountPerLayer = r.TrayCountPerLayer,
		Layers = r.Layers.Select(x => new LayerDTO
		{
			Id = x.Id,
			RackId = x.RackId,
			Number = x.Number,
			Enabled = x.Enabled,
			//Batch = x.BatchId.HasValue ? new Batches.DTO.BatchDTO()
			//{
			//	Id=x.BatchId.Value,
			//	Name=x.Batch.Name,
			//	HarvestDate=x.Batch.HarvestDate,
			//	SeedDate=x.Batch.SeedDate,
			//}:null,
		}).ToList()
	};

	public async Task<IEnumerable<RackDTO>> GetAllRacksAsync(Guid farmId)
	{
		var racks = await _unitOfWork.Rack.GetAllAsync(x => x.Floor.FarmId == farmId && x.Floor.Enabled);			

		return racks
			.OrderBy(r => r.Name)
			.Select(SelectRackToDTO);
	}

	public async Task<IEnumerable<RackDTO>> GetAllRacksByTypeAsync(Guid farmId, Guid typeId)
	{
		var racks = await _unitOfWork.Rack.GetAllAsync(x => x.Floor.FarmId == farmId && x.TypeId == typeId);
		return racks
			.Select(SelectRackToDTO);
	}


	public async Task<RackDTO?> GetRackByIdAsync(Guid id)
	{
		var rack = await _unitOfWork.Rack.GetByIdAsync(id);
		if (rack == null)
			return null;

		return SelectRackToDTO(rack);
	}

	public async Task<List<RackDTO>> GetRacksByTypeIdAsync(Guid typeId)
	{
		var racks = await _unitOfWork.Rack.GetAllAsync(
			r => (typeId == Guid.Empty || r.TypeId == typeId),
			"Layers", "Floor"
		);

		return racks
			.OrderByDescending(r => r.TrayCountPerLayer)
			.ThenBy(r => r.Name)
			.Select(SelectRackToDTO)
			.ToList();
	}

	public async Task<RackDTO> CreateRackAsync(RackDTO rackDto)
	{
		var rack = new Rack
		{
			Id = rackDto.Id == Guid.Empty ? Guid.NewGuid() : rackDto.Id,
			Name = rackDto.Name,
			FloorId = rackDto.Floor.Id,
			Layers = new List<Layer>(),
			LayerCount = rackDto.LayerCount,
			TrayCountPerLayer = rackDto.TrayCountPerLayer
		};

		await _unitOfWork.Rack.AddAsync(rack);
		await _unitOfWork.SaveChangesAsync();

		return SelectRackToDTO(rack);
	}

	public async Task UpdateRackAsync(RackDTO rackDto)
	{
		var rack = await _unitOfWork.Rack.GetByIdAsync(rackDto.Id);
		if (rack == null)
			throw new Exception("Rack not found");

		rack.Name = rackDto.Name;
		rack.FloorId = rackDto.Floor.Id;
		rack.TrayCountPerLayer = rackDto.TrayCountPerLayer;
		rack.LayerCount = rackDto.LayerCount;
		rack.Enabled = rackDto.Enabled;
		_unitOfWork.Rack.Update(rack);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task UpdateRackEnabledAsync(EnabledDTO enabledDto)
	{
		var rack = await _unitOfWork.Rack.GetByIdAsync(enabledDto.Id);
		if (rack == null)
			throw new KeyNotFoundException("Rack not found");

		rack.Enabled = enabledDto.Enabled;

		foreach (var layer in rack.Layers)
		{
			layer.Enabled= enabledDto.Enabled;
			_unitOfWork.Layer.Update(layer);
		}

		_unitOfWork.Rack.Update(rack);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task DeleteRackAsync(Guid id)
	{
		var rack = await _unitOfWork.Rack.GetByIdAsync(id);
		if (rack == null)
			throw new Exception("Rack not found");

		rack.DeletedDateTime = DateTime.UtcNow;
		_unitOfWork.Rack.Update(rack);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task InsertRacksForFarmAsync(Guid farmId, List<Floor> floors)
	{
		if (floors == null || !floors.Any())
		{
			throw new ArgumentException("No floors available to assign racks.");
		}

		var racksToInsert = new List<Rack>();
		foreach (var floor in floors)
		{
			if (floor.Name == "SK1")
			{
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "1",
					Number = 1,
					TypeId = GlobalConstants.RACKTYPE_GERMINATION,
					FloorId = floor.Id,
					LayerCount = 76,
					TrayCountPerLayer = 27
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "2",
					Number = 2,
					TypeId = GlobalConstants.RACKTYPE_GERMINATION,
					FloorId = floor.Id,
					LayerCount = 76,
					TrayCountPerLayer = 27
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "3",
					Number = 3,
					TypeId = GlobalConstants.RACKTYPE_GERMINATION,
					FloorId = floor.Id,
					LayerCount = 76,
					TrayCountPerLayer = 27
				});
			}
			if (floor.Name == "SK2")
			{
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "1",
					Number = 1,
					TypeId = GlobalConstants.RACKTYPE_PROPAGATION,
					LayerCount = 9,
					TrayCountPerLayer = 54,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "2",
					Number = 2,
					TypeId = GlobalConstants.RACKTYPE_PROPAGATION,
					LayerCount = 9,
					TrayCountPerLayer = 54,
					FloorId = floor.Id
				});

				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "3",
					Number = 3,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 54,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "4",
					Number = 4,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 88,
					FloorId = floor.Id
				});

				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "5",
					Number = 5,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "6",
					Number = 6,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "7",
					Number = 7,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "8",
					Number = 8,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "9",
					Number = 9,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "10",
					Number = 10,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "11",
					Number = 11,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "12",
					Number = 12,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 9,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
			}

			if (floor.Name == "SK3")
			{
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "1",
					Number = 1,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 79,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "2",
					Number = 2,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 79,
					FloorId = floor.Id
				});

				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "3",
					Number = 3,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 113,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "4",
					Number = 4,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 113,
					FloorId = floor.Id
				});

				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "5",
					Number = 5,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "6",
					Number = 6,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "7",
					Number = 7,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "8",
					Number = 8,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "9",
					Number = 9,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "10",
					Number = 10,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "11",
					Number = 11,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "12",
					Number = 12,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "13",
					Number = 13,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
				racksToInsert.Add(new Rack
				{
					Id = Guid.NewGuid(),
					Name = "14",
					Number = 14,
					TypeId = GlobalConstants.RACKTYPE_GROWING,
					LayerCount = 18,
					TrayCountPerLayer = 131,
					FloorId = floor.Id
				});
			}
		}

		await _unitOfWork.Rack.AddRangeAsync(racksToInsert);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task<Layer> GetBufferLayer(Guid rackId)
	{
		//get second last layer for buffer
		var buffer = await _unitOfWork.Layer
			.Query(x => x.RackId == rackId && x.IsBufferLayer).FirstOrDefaultAsync();

		if (buffer == null)
		{
			buffer = await _unitOfWork.Layer
			.Query(x => x.RackId == rackId)
			.OrderByDescending(x => x.Number).Skip(1).FirstAsync();
		}
		return buffer;
	}
}

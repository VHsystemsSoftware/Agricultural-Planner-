namespace VHS.Services.Farming.DTO;

public class FloorDTO
{
	public Guid Id { get; set; }
	public Guid FarmId { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Number { get; set; }
	public List<RackDTO> Racks { get; set; } = new List<RackDTO>();

	public bool Enabled { get; set; }

	public bool HasGerminationRacks
	{
		get
		{
			return this.Racks.Any(x => x.TypeId == GlobalConstants.RACKTYPE_GERMINATION);
		}
	}
	public bool HasPropagationRacks
	{
		get

		{
			return this.Racks.Any(x => x.TypeId == GlobalConstants.RACKTYPE_PROPAGATION);
		}
	}
	public bool HasGrowingRacks
	{
		get
		{
			return this.Racks.Any(x => x.TypeId == GlobalConstants.RACKTYPE_GROWING);
		}
	}
}

namespace VHS.Services.Farming.DTO;

public class FarmDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public Guid FarmTypeId { get; set; }
    public string FarmTypeName { get; set; } = string.Empty;

    public ICollection<FloorDTO> Floors { get; set; } = new List<FloorDTO>();

	public ICollection<RackDTO> Racks
	{
		get
		{
            return Floors.SelectMany(x => x.Racks).ToList();
		}
	}

	public ICollection<RackDTO> GrowRacks
    {
        get
        {
            var racks = new List<RackDTO>();
            foreach (var floor in Floors)
            {
                racks.AddRange(floor.Racks.Where(x => x.TypeId == GlobalConstants.RACKTYPE_GROWING));
            }
            return racks;
        }
    }

    public ICollection<RackDTO> GerminationRacks
    {
        get
        {
            var racks = new List<RackDTO>();
            foreach (var floor in Floors)
            {
                racks.AddRange(floor.Racks.Where(x => x.TypeId == GlobalConstants.RACKTYPE_GERMINATION));
            }
            return racks;
        }
    }

    public ICollection<RackDTO> PropagationRacks
    {
        get
        {
            var racks = new List<RackDTO>();
            foreach (var floor in Floors)
            {
                racks.AddRange(floor.Racks.Where(x => x.TypeId == GlobalConstants.RACKTYPE_PROPAGATION));
            }
            return racks;
        }
    }

    //public void AddOccupiedDays(DateTime currentDateTime)
    //{
    //    foreach (var rack in GrowRacks)
    //    {
    //        foreach (var layer in rack.Layers)
    //        {
    //            foreach (var tray in layer.Trays)
    //            {
    //                tray.AddOccupiedDay(currentDateTime);
    //            }
    //        }
    //    }
    //    foreach (var rack in PropagationRacks)
    //    {
    //        foreach (var layer in rack.Layers)
    //        {
    //            foreach (var tray in layer.Trays)
    //            {
    //                tray.AddOccupiedDay(currentDateTime);
    //            }
    //        }
    //    }
    //    foreach (var rack in GerminationRacks)
    //    {
    //        foreach (var layer in rack.Layers)
    //        {
    //            foreach (var tray in layer.Trays)
    //            {
    //                tray.AddOccupiedDay(currentDateTime);
    //            }
    //        }
    //    }
    //}
}

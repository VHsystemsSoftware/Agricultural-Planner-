namespace VHS.Services.Farming.DTO;

public class TrayDestinationDTO
{
    public List<Guid> TrayStateIds { get; set; } = new List<Guid>();
    public Guid DestinationLayerId { get; set; }
    public Guid RackTypeId { get; set; }
}

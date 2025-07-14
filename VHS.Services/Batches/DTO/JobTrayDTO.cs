using System.ComponentModel.DataAnnotations;
using VHS.Services.Farming.DTO;

namespace VHS.Services.Batches.DTO;

public class JobTrayDTO
{
	public Guid Id { get; set; }

	public Guid? TrayId { get; set; }
	public virtual TrayDTO? Tray { get; set; }

	[Required]
	public Guid DestinationLocation { get; set; }
	public Guid? RecipeId { get; set; }
	public string? RecipeName { get; set; }
	public string? SeedIdentifier { get; set; }
	public string? SeedSupplier { get; set; }
	public Guid? DestinationLayerId { get; set; }
	public LayerDTO? DestinationLayer { get; set; }
	public Guid? TransportLayerId { get; set; }
	[Required]
	public int OrderInJob { get; set; }

	public Guid CurrentPhaseId { get; set; }

	public DateTime AddedDateTime { get; set; }
}

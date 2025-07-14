using VHS.Services.Produce.DTO;

namespace VHS.Services.Batches.DTO;

public class BatchDTO
{
	public Guid Id { get; set; }
	public string BatchName { get; set; }
	public Guid FarmId { get; set; }

	public RecipeDTO? Recipe { get; set; } = new RecipeDTO();

    public BatchPlanDTO? BatchPlan { get; set; }

    public int TrayCount { get; set; } = 0;

	public DateTime? SeedDate { get; set; }

	public DateTime? HarvestDate { get; set; } //default = seeddate+recipe days

	public Guid StatusId { get; set; } = GlobalConstants.BATCHSTATUS_PLANNED;
    public virtual ICollection<JobDTO> Jobs { get; set; } = new List<JobDTO>();
    public virtual ICollection<BatchRowDTO> BatchRows { get; set; } = new List<BatchRowDTO>();

	public string? LotReference { get; set; }

	public List<BatchRowDTO> GerminationLayers => BatchRows
        .Where(r => r.LayerRackTypeId == GlobalConstants.RACKTYPE_GERMINATION)
        .ToList();

    public List<BatchRowDTO> PropagationLayers => BatchRows
        .Where(r => r.LayerRackTypeId == GlobalConstants.RACKTYPE_PROPAGATION)
        .ToList();

    public List<BatchRowDTO> GrowLayers => BatchRows
        .Where(r => r.LayerRackTypeId == GlobalConstants.RACKTYPE_GROWING)
        .ToList();
}

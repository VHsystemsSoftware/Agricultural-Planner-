using System.ComponentModel.DataAnnotations.Schema;
using VHS.Services.Notes.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services.Batches.DTO;

public class BatchDTO
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public Guid FarmId { get; set; }

	public RecipeDTO? Recipe { get; set; } = new RecipeDTO();

    public GrowPlanDTO? GrowPlan { get; set; }

    public int TrayCount { get; set; } = 0;

	public DateOnly? SeedDate { get; set; }

	public DateOnly? HarvestDate { get; set; } //default = seeddate+recipe days

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

    [NotMapped]
    public bool IsEmpty
    {
        get
        {
            return this.Recipe == null;
        }
    }

    public virtual ICollection<NoteDTO> Notes { get; set; } = new List<NoteDTO>();

    public bool IsFinishedGrowing(DateOnly? now)
    {
        return !IsEmpty ? FinishedDateGrowing < now : false;
    }
    public bool IsFinishedPropagation(DateOnly? now)
    {
        return !IsEmpty ? FinishedDatePropagation < now : false;
    }
    public bool IsFinishedGermination(DateOnly? now)
    {
        return !IsEmpty ? FinishedDateGermination < now : false;
    }

    [NotMapped]
    public DateOnly? FinishedDateGrowing
    {
        get
        {
            return !IsEmpty ? SeedDate.Value.AddDays(Recipe.GrowDays + Recipe.GerminationDays + Recipe.PropagationDays) : null;
        }
    }
    [NotMapped]
    public DateOnly? FinishedDateGermination
    {
        get
        {
            return !IsEmpty? SeedDate.Value.AddDays(Recipe.GerminationDays) : null;
        }
    }
    [NotMapped]
    public DateOnly? FinishedDatePropagation
    {
        get
        {
            return !IsEmpty ? SeedDate.Value.AddDays(Recipe.PropagationDays) : null;
        }
    }
}

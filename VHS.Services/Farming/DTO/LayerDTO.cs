using Microsoft.EntityFrameworkCore.Update.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using VHS.Services.Batches.DTO;

namespace VHS.Services.Farming.DTO;

public class LayerDTO
{
	public Guid Id { get; set; }
	public int Number { get; set; }

    [JsonIgnore]
    public virtual List<TrayStateDTO> Trays { get; set; } = new List<TrayStateDTO>();

	public Guid RackId { get; set; }

    [JsonIgnore]
    public RackDTO? Rack { get; set; }
	public int TrayCountPerLayer { get; set; }
	public bool IsTransportLayer { get; set; }
	public string Name { get; set; } = string.Empty;

	public bool Enabled { get; set; }
	public bool IsBufferLayer { get; set; }

	public Guid? BatchId { get; set; }

    public virtual BatchDTO? Batch { get; set; }

	public Guid? RackTypeId { get; set; }

    [JsonIgnore]
    public virtual ICollection<TrayStateDTO> PreGrowTrayStates { get; set; } = new List<TrayStateDTO>();

    [JsonIgnore]
    public virtual ICollection<TrayStateDTO> GrowTrayStates { get; set; } = new List<TrayStateDTO>();

	public bool SimulationFull { get; set; }
	public DateOnly? SimulationGrowFinishedDate { get; set; }
	public DateOnly? SimulationPreGrowFinishedDate { get; set; }

	public int BatchRowNumber { get; set; } = 0;

    [NotMapped]
	public bool IsEmpty
	{
		get
		{
			return this.BatchId.HasValue ? this.Batch.Recipe == null : true;
		}
	}

	public bool IsFinishedGrowing(DateOnly? now)
	{
		return !IsEmpty ? FinishedDateGrowing <= now : false;
	}
	public bool IsFinishedPropagation(DateOnly? now)
	{
		return !IsEmpty ? FinishedDatePropagation <= now : false;
	}
	public bool IsFinishedGermination(DateOnly? now)
	{
		return !IsEmpty ? FinishedDateGermination <= now : false;
	}

	[NotMapped]
	public DateOnly? FinishedDateGrowing
	{
		get
		{
			return !IsEmpty ? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.GrowDays + Batch.GrowPlan.Recipe.GerminationDays + Batch.GrowPlan.Recipe.PropagationDays) : null;
		}
	}
	[NotMapped]
	public DateOnly? FinishedDateGermination
	{
		get
		{
			return !IsEmpty ? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.GerminationDays) : null;
		}
	}
	[NotMapped]
	public DateOnly? FinishedDatePropagation
	{
		get
		{
			return !IsEmpty ? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.PropagationDays) : null;
		}
	}
	public bool FinishedGrowing
	{
		get
		{
			return Trays.Any(x => x.IsFinishedGrowing);
		}
    }

  

}

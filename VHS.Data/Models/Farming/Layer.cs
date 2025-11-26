using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;

public class Layer
{
	public Guid Id { get; set; }

	[Required]
	public Guid RackId { get; set; }
	public virtual Rack Rack { get; set; }

	[Required]
	public int Number { get; set; }        //0=top, 1=next, 2=next, etc.

	public DateTime AddedDateTime { get; set; }
	public DateTime? DeletedDateTime { get; set; }

	public bool Enabled { get; set; } = true;
	public bool IsBufferLayer { get; set; } = false;

	[NotMapped]
	public Guid? BatchId { get; set; }

    [NotMapped]
    public virtual Batch Batch { get; set; }

	public bool IsTransportLayer { get; set; }

	//public Batch Batch
	//{
	//	get
	//	{
	//		var trayOnLayer = !IsEmpty ? this.TrayStates.First() : null;
	//		return trayOnLayer != null ? trayOnLayer.Batch : null;
	//	}
	//}

	public virtual ICollection<TrayState> PreGrowTrayStates { get; set; } = new List<TrayState>();
	public virtual ICollection<TrayState> GrowTrayStates { get; set; } = new List<TrayState>();

	public Layer()
	{
		AddedDateTime = DateTime.UtcNow;
	}

	[NotMapped]
	public virtual ICollection<TrayState> TrayStates
	{
		get
		{
			return PreGrowTrayStates.Any() ? PreGrowTrayStates : GrowTrayStates;
		}
	}

	//[NotMapped]
	//public bool IsEmpty
	//{
	//	get
	//	{
	//		return this.BatchId.HasValue ? this.Batch.GrowPlan.RecipeId.HasValue : true;
	//	}
	//}

	//public bool IsFinishedGrowing(DateOnly? now)
	//{
	//	return !IsEmpty ? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.GrowDays + Batch.GrowPlan.Recipe.PreGrowDays) <= now: false;
	//}

	//public bool IsFinishedGermination(DateOnly? now)
	//{
	//	return !IsEmpty ? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.PreGrowDays) <= now : false;
	//}

	//[NotMapped]
	//public DateOnly? FinishedDateGrowing
	//{
	//	get
	//	{
	//		return !IsEmpty ? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.GrowDays + Batch.GrowPlan.Recipe.PreGrowDays) : null;
	//	}
	//}
	//[NotMapped]
	//public DateOnly? FinishedDateGermination
	//{
	//	get
	//	{
	//		return !IsEmpty? Batch.SeedDate.Value.AddDays(Batch.GrowPlan.Recipe.PreGrowDays) : null;
	//	}
	//}
}

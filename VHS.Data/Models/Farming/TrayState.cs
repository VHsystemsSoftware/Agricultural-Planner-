using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;

public class TrayState
{
	[Key]
	public Guid Id { get; set; }
	public Guid TrayId { get; set; }
	public Guid? BatchId { get; set; }
	public Guid? PreGrowLayerId { get; set; }
	public Guid? GrowLayerId { get; set; }

	public Guid? PreGrowTransportLayerId { get; set; }
	public Guid? GrowTransportLayerId { get; set; }

	public Guid? RecipeId { get; set; }
	public Guid? EmptyReason { get; set; }

	public float? HarvestedWeightKG { get; set; }
	[Column(TypeName = "datetime2(7)")]
	public DateTime? WeightRegistered { get; set; }

	public virtual Tray Tray { get; set; }
	public virtual Batch Batch { get; set; }
	public virtual Layer PreGrowLayer { get; set; }
	public virtual Layer GrowLayer { get; set; }
	public virtual Recipe Recipe { get; set; }

	public virtual ICollection<TrayStateAudit> TrayStateAudits { get; set; } = new List<TrayStateAudit>();

	[Column(TypeName = "date")]
	public DateOnly? SeedDate { get; set; }
	[Column(TypeName = "date")]
	public DateOnly? PreGrowFinishedDate { get; set; }
	[Column(TypeName = "date")]
	public DateOnly? GrowFinishedDate { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? ArrivedAtSeeder { get; set; }

	//[Column(TypeName = "datetime2(7)")]
	//public DateTime? TransportToPreGrow { get; set; }
	//[Column(TypeName = "datetime2(7)")]
	//public DateTime? ArrivedPreGrow { get; set; }

	//[Column(TypeName = "datetime2(7)")]
	//public DateTime? TransportToGrow { get; set; }
	[Column(TypeName = "datetime2(7)")]
	public DateTime? ArrivedGrow { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? TransportToHarvest { get; set; }
	[Column(TypeName = "datetime2(7)")]
	public DateTime? ArrivedHarvest { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? EmptyToTransplant { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? PropagationToTransplant { get; set; }

	//[Column(TypeName = "datetime2(7)")]
	//public DateTime? ArrivedTransplant { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? TransportToWashing { get; set; }
	[Column(TypeName = "datetime2(7)")]
	public DateTime? ArrivedWashing { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? TransportToPaternosterUp { get; set; }
	[Column(TypeName = "datetime2(7)")]
	public DateTime? ArrivedPaternosterUp { get; set; }


	[Column(TypeName = "datetime2(7)")]
	public DateTime? WillBePushedOutPreGrow { get; set; }

	[Column(TypeName = "datetime2(7)")]
	public DateTime? WillBePushedOutGrow { get; set; }

	public Guid? PreGrowPushedOutByTrayId { get; set; }
	public Guid? GrowPushedOutByTrayId { get; set; }

	//[Column(TypeName = "datetime2(7)")]
	//public DateTime? TransportToPaternosterDown { get; set; }
	//[Column(TypeName = "datetime2(7)")]
	//public DateTime? ArrivedPaternosterDown { get; set; }

	public int? PreGrowOrderOnLayer { get; set; }
	public int? GrowOrderOnLayer { get; set; }

	public DateTime AddedDateTime { get; set; }
	public DateTime? FinishedDateTime { get; set; }

	[NotMapped]
	public Guid? LayerId
	{
		get
		{

			if (GrowTransportLayerId.HasValue && GrowOrderOnLayer.HasValue)
				return GrowTransportLayerId;
			else if(PreGrowTransportLayerId.HasValue && PreGrowOrderOnLayer.HasValue)
				return PreGrowTransportLayerId;
			else if (GrowLayerId.HasValue && GrowOrderOnLayer.HasValue)
				return GrowLayerId;
			else if (PreGrowLayerId.HasValue && PreGrowOrderOnLayer.HasValue)
				return PreGrowLayerId;
			else
				return null;
		}
	}

	[NotMapped]
	public Guid? RackId
	{
		get
		{
			return GrowLayerId.HasValue ? GrowLayer.RackId : PreGrowLayerId.HasValue ? PreGrowLayer.RackId : null;
		}
	}

	[NotMapped]
	public int? OrderOnLayer
	{
		get
		{
			return GrowOrderOnLayer.HasValue ? GrowOrderOnLayer : PreGrowOrderOnLayer.HasValue ? PreGrowOrderOnLayer : null;
		}
	}

	[NotMapped]
	public int? RackNumber
	{
		get
		{
			return GrowLayerId.HasValue ? GrowLayer.Rack.Number : PreGrowLayerId.HasValue ? PreGrowLayer.Rack.Number : null;
		}
	}

}

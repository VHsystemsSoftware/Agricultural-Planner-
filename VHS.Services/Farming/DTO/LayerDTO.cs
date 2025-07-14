using Microsoft.EntityFrameworkCore.Update.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using VHS.Services.Batches.DTO;

namespace VHS.Services.Farming.DTO;

public class LayerDTO
{
	public Guid Id { get; set; }
	public int Number { get; set; }
	public virtual List<TrayStateDTO> Trays { get; set; } = new List<TrayStateDTO>();

	public Guid RackId { get; set; }
	public int TrayCountPerLayer { get; set; }
	public bool IsTransportLayer { get; set; }
	public string Name { get; set; } = string.Empty;

	public bool Enabled { get; set; }

	public Guid? RackTypeId { get; set; }

	public virtual ICollection<TrayStateDTO> PreGrowTrayStates { get; set; } = new List<TrayStateDTO>();
	public virtual ICollection<TrayStateDTO> GrowTrayStates { get; set; } = new List<TrayStateDTO>();



	//public bool FinishedGrowing
	//{
	//	get
	//	{
	//		return Trays.Any(x => x.IsFinishedGrowing);
	//	}
	//}

	//public bool HasRoom
	//{
	//	get
	//	{
	//		if (IsTransportLayer || !Enabled)
	//			return false;

	//		int occupiedCount = Trays.Count(t =>
	//			t.CurrentPhaseId != GlobalConstants.TRAYPHASE_EMPTY &&
	//			t.CurrentPhaseId != GlobalConstants.TRAYPHASE_FULLYGROWN
	//		);

	//		return occupiedCount < TrayCountPerLayer;
	//	}
	//}

	//public int AvailableSlots
	//{
	//	get
	//	{
	//		return TrayCountPerLayer - Trays.Count(t =>
	//			t.CurrentPhaseId != GlobalConstants.TRAYPHASE_EMPTY &&
	//			t.CurrentPhaseId != GlobalConstants.TRAYPHASE_FULLYGROWN
	//		);
	//	}
	//}

	//public void ReorderTrays()
	//{
	//	var lowest = Trays.Min(x => x.OrderOnLayer);
	//	int order = 1;
	//	foreach (var tray in Trays.OrderBy(x => x.OrderOnLayer))
	//	{
	//		tray.OrderOnLayer = order;
	//		order++;
	//	}
	//}

}

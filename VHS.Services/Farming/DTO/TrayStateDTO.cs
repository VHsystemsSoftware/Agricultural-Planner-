using VHS.Services.Produce.DTO;

namespace VHS.Services.Farming.DTO
{
	public class TrayStateDTO
	{
		public Guid Id { get; set; }
		public Guid TrayId { get; set; }
		public Guid? BatchId { get; set; }
		public Guid? PreGrowLayerId { get; set; }
		public Guid? GrowLayerId { get; set; }

		public Guid? PreGrowTransportLayerId { get; set; }
		public Guid? GrowTransportLayerId { get; set; }

		public Guid? RecipeId { get; set; }
        public RecipeDTO? Recipe { get; set; }
        public Guid? EmptyReason { get; set; }

		public float? HarvestedWeightKG { get; set; }
		public DateTime? WeightRegistered { get; set; }

		public string TrayTag { get; set; }
		public string GrowLayerName { get; set; }
		public string PreGrowLayerName { get; set; }
		public string GrowTransportLayerName { get; set; }

		public int? PreGrowLayerNumber { get; set; }
		public int? GrowLayerNumber { get; set; }

		public string PreGrowTransportLayerName { get; set; }
		public DateOnly? SeedDate { get; set; }

		public DateOnly? PreGrowFinishedDate { get; set; }

		public DateOnly? GrowFinishedDate { get; set; }

		public DateTime? ArrivedAtSeeder { get; set; }

		public DateTime? ArrivedGrow { get; set; }

		public DateTime? TransportToHarvest { get; set; }
		public DateTime? ArrivedHarvest { get; set; }

		public DateTime? EmptyToTransplant { get; set; }

		public DateTime? PropagationToTransplant { get; set; }

		public DateTime? TransportToWashing { get; set; }
		public DateTime? ArrivedWashing { get; set; }

		public DateTime? TransportToPaternosterUp { get; set; }
		public DateTime? ArrivedPaternosterUp { get; set; }
        public DateTime? WillBePushedOutPreGrow { get; set; }
        public DateTime? WillBePushedOutGrow { get; set; }
        public DateTime AddedDateTime { get; set; }
		public DateTime? FinishedDateTime { get; set; }

		public int? PreGrowOrderOnLayer { get; set; }
		public int? GrowOrderOnLayer { get; set; }

		public int OrderOnLayer { get; set; }

		public bool IsEstimated { get; set; } = false;

		public bool IsFinishedGrowing
		{
			get
			{
				return GrowFinishedDate.HasValue && GrowFinishedDate.Value >= DateOnly.FromDateTime(DateTime.UtcNow);
			}
		}

		public string BatchName { get; set; }
	}
}

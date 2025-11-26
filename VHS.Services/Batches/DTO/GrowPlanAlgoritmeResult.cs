using VHS.Services.Farming.DTO;

namespace VHS.Services.Batches.DTO
{
    public class GrowDemandResultPerDay
    {
		public int DayCount { get; set; }
		public DateOnly Date { get; set; }
        public int Harvested { get; set; } = 0;
        public int Washed { get; set; } = 0;

        public List<RackDTO> Racks { get; set; } = new List<RackDTO>();

        public List<BatchDTO> Batches { get; set; } = null;

        public List<string> Messages { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}

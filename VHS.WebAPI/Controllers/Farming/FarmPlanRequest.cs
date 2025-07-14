using VHS.Services.Produce.DTO;

namespace VHS.WebAPI.Controllers.Farming;

public class FarmPlanRequest
{
    public List<ProductCategoryBatchSizeDTO> BatchSizes { get; set; } = new();
    public int TotalDays { get; set; }
    public int TotalTraysAvailable { get; set; }
    public DateTime StartDate { get; set; }
}

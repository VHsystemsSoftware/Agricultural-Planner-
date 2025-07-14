using VHS.Services.Batches.DTO;

namespace VHS.Services.Produce.DTO;

public class ProductCategoryBatchSizeDTO
{
    public BatchDTO Batch { get; set; } = new BatchDTO();
    public ProductCategoryDTO ProductCategory { get; set; } = new ProductCategoryDTO();
    public int GrowDays { get; set; } = 0;
    public int PropagationDays { get; set; } = 0;
    public int GerminationDays { get; set; } = 0;
    public int TraysPerDay { get; set; } = 0;
    public double PlusMinPercentage { get; set; }
}

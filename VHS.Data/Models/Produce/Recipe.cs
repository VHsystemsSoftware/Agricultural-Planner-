using System.ComponentModel.DataAnnotations;

namespace VHS.Data.Core.Models;

public class Recipe
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    [Required]
    public int GerminationDays { get; set; }
    [Required]
    public int PropagationDays { get; set; }
    [Required]
    public int GrowDays { get; set; }

    public virtual ICollection<RecipeLightSchedule> RecipeLightSchedules { get; set; } = new List<RecipeLightSchedule>();
    public virtual ICollection<RecipeWaterSchedule> RecipeWaterSchedules { get; set; } = new List<RecipeWaterSchedule>();
    public virtual ICollection<BatchPlan> BatchPlans { get; set; } = new List<BatchPlan>();

    public DateTime AddedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }

    public Recipe()
    {
        AddedDateTime = DateTime.UtcNow;
        ModifiedDateTime = DateTime.UtcNow;
    }

	public int PreGrowDays 
    { 
        get
        {
            return GerminationDays + PropagationDays;
		}
    }

	public bool IsGerminationProduct
	{
		get
		{
			return Product.ProductCategoryId != GlobalConstants.PRODUCTCATEGORY_LETTUCE;
		}
	}

	public bool IsPropagationProduct { get
        {
            return Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_LETTUCE;
		}
    }
}

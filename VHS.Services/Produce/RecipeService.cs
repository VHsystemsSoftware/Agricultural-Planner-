using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHS.Services.Audit;
using VHS.Services.Audit.DTO;
using VHS.Services.Batches.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IRecipeService
{
    Task<IEnumerable<RecipeDTO>> GetAllRecipesAsync(Guid? farmId = null);
    Task<RecipeDTO?> GetRecipeByIdAsync(Guid id);
    Task<RecipeDTO> CreateRecipeAsync(RecipeDTO recipeDto, string userId);
    Task<RecipeDTO> UpdateRecipeAsync(RecipeDTO recipeDto, string userId);
    Task DeleteRecipeAsync(Guid id, string userId);
}

public static class RecipeDTOSelect
{
	public static IQueryable<RecipeDTO> MapRecipeToDTO(this IQueryable<Recipe> data)
	{
		var method = System.Reflection.MethodBase.GetCurrentMethod();
		return data.TagWith(method.Name)
			.Select(r => new RecipeDTO
			{
				Id = r.Id,
				Name = r.Name,
				Product = r.Product != null
		            ? new ProductDTO
		            {
			            Id = r.Product.Id,
			            Name = r.Product.Name,
			            FarmId = r.Product.FarmId,
                        SeedSupplier= r.Product.SeedSupplier,
                        SeedIdentifier = r.Product.SeedIdentifier,
                        ProductCategoryId = r.Product.ProductCategoryId,
                        Description=r.Description                        
					} : new ProductDTO(),
				Description = r.Description,
				GerminationDays = r.GerminationDays,
				PropagationDays = r.PropagationDays,
				GrowDays = r.GrowDays,
				AddedDateTime = r.AddedDateTime,
				ModifiedDateTime = r.ModifiedDateTime
			});
	}
}


public class RecipeService : IRecipeService
{
    private readonly IUnitOfWorkCore _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public RecipeService(IUnitOfWorkCore unitOfWork, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<RecipeDTO>> GetAllRecipesAsync(Guid? farmId = null)
    {
        var recipes = await _unitOfWork.Recipe.Query(x => !farmId.HasValue || x.Product.FarmId == farmId.Value)
			.MapRecipeToDTO()
			.AsNoTracking()
			.OrderBy(p => p.Name)
			.ToListAsync();

        return recipes;
    }

    public async Task<RecipeDTO?> GetRecipeByIdAsync(Guid id)
    {
        var recipe = await _unitOfWork.Recipe.Query(x=>x.Id == id)
			.MapRecipeToDTO()
			.AsNoTracking()
			.SingleOrDefaultAsync();
        return recipe;
    }       

    public async Task<RecipeDTO> CreateRecipeAsync(RecipeDTO recipeDto, string userId)
    {
        var recipe = new Recipe
        {
            Id = recipeDto.Id == Guid.Empty ? Guid.NewGuid() : recipeDto.Id,
            Name = recipeDto.Name,                
            Description = recipeDto.Description,
            ProductId = recipeDto.Product.Id,
            GerminationDays = recipeDto.GerminationDays,
            PropagationDays = recipeDto.PropagationDays,
            GrowDays = recipeDto.GrowDays,
            AddedDateTime = DateTime.UtcNow,
            ModifiedDateTime = DateTime.UtcNow
        };

        await _unitOfWork.Recipe.AddAsync(recipe);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result > 0)
        {
            var newRecipeDto = await GetRecipeByIdAsync(recipe.Id);
            await CreateAuditLogAsync("Added", userId, recipe.Id, null, newRecipeDto);
        }

        return await GetRecipeByIdAsync(recipe.Id);
    }

    public async Task<RecipeDTO> UpdateRecipeAsync(RecipeDTO recipeDto, string userId)
    {

        var recipe = await _unitOfWork.Recipe.GetByIdAsync(recipeDto.Id);
        if (recipe == null)
            throw new Exception("Recipe not found");

        var oldRecipeDto = await GetRecipeByIdAsync(recipe.Id);

        recipe.Name = recipeDto.Name;
        recipe.Description = recipeDto.Description;
        recipe.ProductId = recipeDto.Product.Id;
        recipe.GerminationDays = recipeDto.GerminationDays;
        recipe.PropagationDays = recipeDto.PropagationDays;
        recipe.GrowDays = recipeDto.GrowDays;
        recipe.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.Recipe.Update(recipe);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result > 0)
        {
            var newRecipeDto = await GetRecipeByIdAsync(recipe.Id);
            await CreateAuditLogAsync("Modified", userId, recipe.Id, oldRecipeDto, newRecipeDto);
        }

        return await GetRecipeByIdAsync(recipe.Id);
    }

    public async Task DeleteRecipeAsync(Guid id, string userId)
    {
        var recipe = await _unitOfWork.Recipe.GetByIdAsync(id);
        if (recipe == null)
            throw new Exception("Recipe not found");

        var oldRecipeDto = await GetRecipeByIdAsync(recipe.Id);

        recipe.DeletedDateTime = DateTime.UtcNow;
        _unitOfWork.Recipe.Update(recipe);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result > 0)
        {
            await CreateAuditLogAsync("Deleted", userId, recipe.Id, oldRecipeDto, null);
        }
    }

    private async Task CreateAuditLogAsync(string action, string userId, Guid entityId, RecipeDTO? oldDto, RecipeDTO? newDto)
    {
        var auditLog = new AuditLogDTO
        {
            UserId = string.IsNullOrEmpty(userId) ? "SYSTEM" : userId,
            EntityName = nameof(Recipe),
            Action = action,
            Timestamp = DateTime.UtcNow,
            KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
            OldValues = oldDto == null ? null : JsonSerializer.Serialize(oldDto),
            NewValues = newDto == null ? null : JsonSerializer.Serialize(newDto)
        };

        await _auditLogService.CreateAuditLogAsync(auditLog);
    }
}

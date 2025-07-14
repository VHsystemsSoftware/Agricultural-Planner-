namespace VHS.Data.Core.Repositories;

public interface IRecipeRepository : IRepository<Recipe>
{
}
public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    private readonly VHSCoreDBContext _context;

    public RecipeRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

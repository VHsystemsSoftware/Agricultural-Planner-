namespace VHS.Data.Core.Repositories;

public interface IRecipeWaterScheduleRepository : IRepository<RecipeWaterSchedule>
{
}
public class RecipeWaterScheduleRepository : Repository<RecipeWaterSchedule>, IRecipeWaterScheduleRepository
{
    private readonly VHSCoreDBContext _context;

    public RecipeWaterScheduleRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

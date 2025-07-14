namespace VHS.Data.Core.Repositories;

public interface IRecipeLightScheduleRepository : IRepository<RecipeLightSchedule>
{
}
public class RecipeLightScheduleRepository : Repository<RecipeLightSchedule>, IRecipeLightScheduleRepository
{
    private readonly VHSCoreDBContext _context;

    public RecipeLightScheduleRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

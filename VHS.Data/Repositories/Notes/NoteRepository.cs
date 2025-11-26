namespace VHS.Data.Core.Repositories;

public interface INoteRepository : IRepository<Note>
{
}

public class NoteRepository : Repository<Note>, INoteRepository
{
    private readonly VHSCoreDBContext _context;

    public NoteRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }
}

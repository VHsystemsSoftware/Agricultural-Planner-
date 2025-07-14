namespace VHS.Data.Core.Repositories;

public interface ILayerRepository : IRepository<Layer> {}

public class LayerRepository : Repository<Layer>, ILayerRepository
{
    private readonly VHSCoreDBContext _context;

    public LayerRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }

}

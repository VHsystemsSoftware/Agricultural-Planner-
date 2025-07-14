using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace VHS.Data.Core.Repositories;

public interface IProductRepository : IRepository<Product>
{
}
public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly VHSCoreDBContext _context;

    public ProductRepository(VHSCoreDBContext context) : base(context)
    {
        _context = context;
    }

    //public override async Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>>? filter = null, params string[] includeProperties)
    //{
    //    IQueryable<Product> query = _context.Products;

    //    if (filter != null)
    //    {
    //        query = query.Where(filter);
    //    }

    //    return query;
    //}

    //public override async Task<Product?> GetByIdAsync(object id)
    //{
    //    return await _context.Products.FirstOrDefaultAsync(p => p.Id == (Guid)id);
    //}
}

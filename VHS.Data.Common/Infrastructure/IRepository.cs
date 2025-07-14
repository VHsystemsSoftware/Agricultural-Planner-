using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VHS.Data.Common.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        Task<T?> GetByIdAsync(object id);
        Task<T?> GetByIdWithIncludesAsync(object id, params string[] includeProperties);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, params string[] includeProperties);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params string[] includeProperties);
        IQueryable<T> Query(Expression<Func<T, bool>>? filter = null, params string[] includeProperties);
    }
}

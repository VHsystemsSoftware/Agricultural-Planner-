using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VHS.Data.Common.Infrastructure
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = IncludeProperties(query, includeProperties);
            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByIdWithIncludesAsync(object id, params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            query = IncludeProperties(query, includeProperties);
            return await query.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));
        }

        public virtual async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            query = IncludeProperties(query, includeProperties);
            return await query.FirstOrDefaultAsync();
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual IQueryable<T> Query(Expression<Func<T, bool>>? filter = null, params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = IncludeProperties(query, includeProperties);
            return query;
        }

        private static IQueryable<T> IncludeProperties(IQueryable<T> query, params string[] includeProperties)
        {
            foreach (var property in includeProperties)
            {
                query = query.Include(property);
            }
            return query;
        }
    }
}

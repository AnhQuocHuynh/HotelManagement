using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HotelManager.Data;
using HotelManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManager.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly HotelDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;

        public Repository(HotelDbContext context, ILogger<Repository<T>> logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T> AddAsync(T entity) { await _dbSet.AddAsync(entity); return entity; }
        public async Task<T> UpdateAsync(T entity) { _dbSet.Update(entity); return entity; }
        public async Task<bool> DeleteAsync(int id) { var entity = await _dbSet.FindAsync(id); if (entity == null) return false; _dbSet.Remove(entity); return true; }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.SingleOrDefaultAsync(predicate);
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);
    }
} 
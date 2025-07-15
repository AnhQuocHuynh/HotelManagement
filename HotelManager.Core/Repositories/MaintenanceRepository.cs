using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Core.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly HotelDbContext _context;

        public MaintenanceRepository(HotelDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Maintenance entity)
        {
            await _context.Maintenances.AddAsync(entity);
        }

        public async Task<IEnumerable<Maintenance>> GetAllAsync()
        {
            return await _context.Maintenances
                                 .Include(m => m.Employee)
                                 .Include(m => m.MaintenanceReport)
                                 .ToListAsync();
        }

        public async Task<Maintenance?> GetByIdAsync(int id)
        {
            return await _context.Maintenances
                                 .Include(m => m.Employee)
                                 .Include(m => m.MaintenanceReport)
                                 .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Maintenances.FindAsync(id);
            if (entity == null)
                return false;

            _context.Maintenances.Remove(entity);
            return true;
        }

        public async Task UpdateAsync(Maintenance entity)
        {
            _context.Maintenances.Update(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<Maintenance, bool>> predicate)
        {
            return await _context.Maintenances.AnyAsync(predicate);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

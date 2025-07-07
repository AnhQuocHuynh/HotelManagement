using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManager.Repositories
{
    public class EmployeeRepository : Repository<Employee>
    {
        public EmployeeRepository(HotelDbContext context, ILogger<EmployeeRepository> logger) : base(context, logger) { }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet.Include(e => e.UserAccount).ToListAsync();
        }

        public override async Task<Employee> GetByIdAsync(int id)
        {
            return await _dbSet.Include(e => e.UserAccount).FirstOrDefaultAsync(e => e.Id == id);
        }
    }
} 
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
    public class WorkAssignmentRepository : Repository<WorkAssignment>
    {
        public WorkAssignmentRepository(HotelDbContext context, ILogger<WorkAssignmentRepository> logger) : base(context, logger) { }

        public override async Task<IEnumerable<WorkAssignment>> GetAllAsync()
        {
            return await _dbSet
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .ToListAsync();
        }

        public override async Task<WorkAssignment> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
} 
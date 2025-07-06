using HotelManager.Core.Data;
using HotelManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HotelManager.Core.Repositories
{
    public class EmployeeRepository : Repository<Employee>
    {
        public EmployeeRepository(HotelDbContext context) : base(context) { }

        public override IEnumerable<Employee> GetAll()
        {
            return _dbSet.Include(e => e.UserAccount).ToList();
        }

        public override Employee GetById(int id)
        {
            return _dbSet.Include(e => e.UserAccount).FirstOrDefault(e => e.Id == id);
        }
    }
} 
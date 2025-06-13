using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Services
{
    internal class EmployeeService : IService<Employee>
    {
        private readonly HotelDbContext _dbContext;
        public EmployeeService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
            HotelDbInitializer.Seed(_dbContext);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.Employees.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            _dbContext.Employees.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<Employee> CreateAsync(Employee entity)
        {
            _dbContext.Employees.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }


        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbContext.Employees.Include(e => e.UserAccount).ToListAsync();
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            var entity = await _dbContext.Employees.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }
            return entity;
        }

        public async Task<Employee> UpdateAsync(Employee entity)
        {
            _dbContext.Employees.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}

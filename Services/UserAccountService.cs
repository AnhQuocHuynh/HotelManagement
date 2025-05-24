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
    internal class UserAccountService : IService<UserAccount>
    {
        private readonly HotelDbContext _dbContext;

        public UserAccountService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
            HotelDbInitializer.Seed(_dbContext);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.UserAccounts.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            _dbContext.UserAccounts.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public Task<UserAccount> CreateAsync(UserAccount entity)
        {
            _dbContext.UserAccounts.Add(entity);
            _dbContext.SaveChangesAsync();
            return Task.FromResult(entity);
        }
        public async Task<IEnumerable<UserAccount>> GetAllAsync()
        {
            return await _dbContext.UserAccounts.Include(u => u.Employee).ToListAsync();
        }
        public async Task<UserAccount> GetByIdAsync(int id)
        {
            var entity = await _dbContext.UserAccounts.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"UserAccount with ID {id} not found.");
            }
            return entity;
        }
        public Task<UserAccount> UpdateAsync(UserAccount entity)
        {
            _dbContext.UserAccounts.Update(entity);
            _dbContext.SaveChanges();
            return Task.FromResult(entity);
        }
    }
}

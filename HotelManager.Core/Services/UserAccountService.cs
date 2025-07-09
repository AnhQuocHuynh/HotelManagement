using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using HotelManager.Exceptions;
using HotelManager.Helpers;

namespace HotelManager.Services
{
    public class UserAccountService : IService<UserAccount>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _unitOfWork.UserAccounts.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }
        public async Task<UserAccount> CreateAsync(UserAccount entity)
        {
            await _unitOfWork.UserAccounts.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }
        public async Task<IEnumerable<UserAccount>> GetAllAsync()
        {
            return await _unitOfWork.UserAccounts.GetAllAsync();
        }
        public async Task<UserAccount> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.UserAccounts.GetByIdAsync(id);
            if (entity == null)
                throw new EntityNotFoundException("UserAccount", id);
            return entity;
        }
        public async Task<UserAccount> UpdateAsync(UserAccount entity)
        {
            await _unitOfWork.UserAccounts.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            var user = await _unitOfWork.UserAccounts.GetAllAsync();
            var targetUser = user.FirstOrDefault(u => u.Username == username);
            
            if (targetUser == null)
                return false;
            string hashedPassword = HashHelper.HashPassword(password);
            // Simple password validation for now - in production, use proper hashing
            return targetUser.PasswordHash == hashedPassword;
        }
        public async Task ChangePasswordAsync(string username, string newPassword)
        {
            var users = await _unitOfWork.UserAccounts.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            
            if (user == null)
                throw new EntityNotFoundException("UserAccount", username);

            // Simple password hashing for now - in production, use proper hashing
            user.PasswordHash = HashHelper.HashPassword(newPassword);
            await _unitOfWork.UserAccounts.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

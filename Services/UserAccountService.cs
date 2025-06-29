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

namespace HotelManager.Services
{
    internal class UserAccountService : IService<UserAccount>
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
    }
}

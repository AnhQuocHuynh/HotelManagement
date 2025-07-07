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
    public class EmployeeService : IService<Employee>
    {
        private readonly IUnitOfWork _unitOfWork;
        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            //HotelDbInitializer.Seed(_dbContext);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _unitOfWork.Employees.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }
        public async Task<Employee> CreateAsync(Employee entity)
        {
            await _unitOfWork.Employees.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }


        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.EmployeeRepository != null)
                return await uowImpl.EmployeeRepository.GetAllAsync();
            return await _unitOfWork.Employees.GetAllAsync();
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.EmployeeRepository != null)
            {
                var entity = await uowImpl.EmployeeRepository.GetByIdAsync(id);
                if (entity == null)
                    throw new EntityNotFoundException("Employee", id);
                return entity;
            }
            else
            {
                var entity = await _unitOfWork.Employees.GetByIdAsync(id);
                if (entity == null)
                    throw new EntityNotFoundException("Employee", id);
                return entity;
            }
        }

        public async Task<Employee> UpdateAsync(Employee entity)
        {
            await _unitOfWork.Employees.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }
    }
}

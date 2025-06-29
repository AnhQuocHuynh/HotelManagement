using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Exceptions;
using HotelManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class CustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerService> _logger;

        // Constructor với Unit of Work (recommended)
        public CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Legacy constructor for backward compatibility
        public CustomerService(HotelDbContext dbContext, ILogger<CustomerService> logger)
        {
            // Create a temporary UnitOfWork for legacy usage
            var loggerFactory = new LoggerFactory();
            _unitOfWork = new Repositories.UnitOfWork(dbContext, loggerFactory);
            _logger = logger;
        }

        public async Task CreateAsync(Customer customer)
        {
            try
            {
                _logger.LogInformation("Creating new customer with CCCD: {CCCD}", customer.CCCD);
                
                var existingCustomer = await _unitOfWork.Customers
                    .SingleOrDefaultAsync(c => c.CCCD == customer.CCCD);
                    
                if (existingCustomer != null)
                {
                    _logger.LogWarning("Customer with CCCD {CCCD} already exists", customer.CCCD);
                    throw new DuplicateEntityException("Customer", "CCCD", customer.CCCD);
                }

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully created customer with ID: {CustomerId}, CCCD: {CCCD}", customer.Id, customer.CCCD);
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer with CCCD: {CCCD}", customer.CCCD);
                throw new BusinessException(
                    $"Error creating customer: {ex.Message}", 
                    ex, 
                    "Lỗi khi tạo khách hàng. Vui lòng thử lại.",
                    "CUSTOMER_CREATE_ERROR");
            }
        }

        public async Task UpdateAsync(Customer customer)
        {
            try
            {
                _logger.LogInformation("Updating customer with ID: {CustomerId}, CCCD: {CCCD}", customer.Id, customer.CCCD);
                
                var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
                if (existingCustomer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", customer.Id);
                    throw new EntityNotFoundException("Customer", customer.Id);
                }

                // Update properties
                existingCustomer.FullName = customer.FullName;
                existingCustomer.CCCD = customer.CCCD;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Type = customer.Type;

                await _unitOfWork.Customers.UpdateAsync(existingCustomer);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", customer.Id);
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", customer.Id);
                throw new BusinessException(
                    $"Error updating customer: {ex.Message}", 
                    ex, 
                    "Lỗi khi cập nhật khách hàng. Vui lòng thử lại.",
                    "CUSTOMER_UPDATE_ERROR");
            }
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting customer with ID: {CustomerId}", id);
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                    throw new EntityNotFoundException("Customer", id);
                }
                
                return customer;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer with ID: {CustomerId}", id);
                throw new BusinessException(
                    $"Error retrieving customer: {ex.Message}",
                    ex,
                    "Lỗi khi lấy thông tin khách hàng.",
                    "CUSTOMER_GET_ERROR");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
                
                var result = await _unitOfWork.Customers.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found for deletion", id);
                    throw new EntityNotFoundException("Customer", id);
                }
                
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted customer with ID: {CustomerId}", id);
                
                return true;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
                throw new BusinessException(
                    $"Error deleting customer: {ex.Message}",
                    ex,
                    "Lỗi khi xóa khách hàng.",
                    "CUSTOMER_DELETE_ERROR");
            }
        }
    }
}
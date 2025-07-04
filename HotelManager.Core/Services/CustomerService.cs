using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Exceptions;
using HotelManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HotelManager.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Services
{
    public class CustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerService> _logger;
        private readonly IAuditService _auditService;

        // Enhanced constructor with audit service (preferred for DI)
        [ActivatorUtilitiesConstructor]
        public CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        // Legacy constructor for backward compatibility
        private CustomerService(HotelDbContext dbContext, ILogger<CustomerService> logger)
        {
            // Create a temporary UnitOfWork for legacy usage
            var loggerFactory = new LoggerFactory();
            _unitOfWork = new Repositories.UnitOfWork(dbContext, loggerFactory);
            _logger = logger;
            _auditService = null; // No audit service in legacy mode
        }

        public async Task CreateAsync(Customer customer)
        {
            using var performanceMonitor = _auditService != null 
                ? new PerformanceMonitor(_auditService, _logger, "CustomerService.CreateAsync", $"CCCD: {customer.CCCD}")
                : null;
            
            try
            {
                _logger.LogInformation("Creating new customer with CCCD: {CCCD}", customer.CCCD);
                
                var existingCustomer = await _unitOfWork.Customers
                    .SingleOrDefaultAsync(c => c.CCCD == customer.CCCD);
                    
                if (existingCustomer != null)
                {
                    _logger.LogWarning("Customer with CCCD {CCCD} already exists", customer.CCCD);
                    performanceMonitor?.MarkAsFailure($"Duplicate CCCD: {customer.CCCD}");
                    throw new DuplicateEntityException("Customer", "CCCD", customer.CCCD);
                }

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully created customer with ID: {CustomerId}, CCCD: {CCCD}", customer.Id, customer.CCCD);
                
                // Log user activity for audit trail
                await _auditService?.LogUserActivityAsync(null, "CREATE", "Customer", customer.Id.ToString(), 
                    $"Created customer: {customer.FullName} (CCCD: {customer.CCCD})");
                    
                await _auditService?.LogBusinessOperationAsync("CREATE_CUSTOMER", "Customer", customer.Id.ToString(), 
                    null, true, $"Customer: {customer.FullName}, CCCD: {customer.CCCD}, Type: {customer.Type}");
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer with CCCD: {CCCD}", customer.CCCD);
                performanceMonitor?.MarkAsFailure(ex.Message);
                
                await _auditService?.LogBusinessOperationAsync("CREATE_CUSTOMER", "Customer", "unknown", 
                    null, false, $"CCCD: {customer.CCCD}, Error: {ex.Message}");
                    
                throw new BusinessException(
                    $"Error creating customer: {ex.Message}", 
                    ex, 
                    "Lỗi khi tạo khách hàng. Vui lòng thử lại.",
                    "CUSTOMER_CREATE_ERROR");
            }
        }

        public async Task UpdateAsync(Customer customer)
        {
            using var performanceMonitor = _auditService != null 
                ? new PerformanceMonitor(_auditService, _logger, "CustomerService.UpdateAsync", $"CustomerID: {customer.Id}")
                : null;
            
            try
            {
                _logger.LogInformation("Updating customer with ID: {CustomerId}, CCCD: {CCCD}", customer.Id, customer.CCCD);
                
                var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
                if (existingCustomer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", customer.Id);
                    performanceMonitor?.MarkAsFailure($"Customer ID {customer.Id} not found");
                    throw new EntityNotFoundException("Customer", customer.Id);
                }

                // Capture old values for audit
                var oldValues = $"FullName: {existingCustomer.FullName}, CCCD: {existingCustomer.CCCD}, " +
                               $"PhoneNumber: {existingCustomer.PhoneNumber}, Type: {existingCustomer.Type}";

                // Update properties
                existingCustomer.FullName = customer.FullName;
                existingCustomer.CCCD = customer.CCCD;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Type = customer.Type;

                await _unitOfWork.Customers.UpdateAsync(existingCustomer);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", customer.Id);
                
                // Log audit trail
                var newValues = $"FullName: {customer.FullName}, CCCD: {customer.CCCD}, " +
                               $"PhoneNumber: {customer.PhoneNumber}, Type: {customer.Type}";
                
                await _auditService?.LogUserActivityAsync(null, "UPDATE", "Customer", customer.Id.ToString(), 
                    $"Updated customer from [{oldValues}] to [{newValues}]");
                    
                await _auditService?.LogBusinessOperationAsync("UPDATE_CUSTOMER", "Customer", customer.Id.ToString(), 
                    null, true, $"Updated: {customer.FullName}");
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", customer.Id);
                performanceMonitor?.MarkAsFailure(ex.Message);
                
                await _auditService?.LogBusinessOperationAsync("UPDATE_CUSTOMER", "Customer", customer.Id.ToString(), 
                    null, false, $"Error: {ex.Message}");
                    
                throw new BusinessException(
                    $"Error updating customer: {ex.Message}", 
                    ex, 
                    "Lỗi khi cập nhật khách hàng. Vui lòng thử lại.",
                    "CUSTOMER_UPDATE_ERROR");
            }
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            using var performanceMonitor = _auditService != null 
                ? new PerformanceMonitor(_auditService, _logger, "CustomerService.GetByIdAsync", $"CustomerID: {id}")
                : null;
            
            try
            {
                _logger.LogInformation("Getting customer with ID: {CustomerId}", id);
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                    performanceMonitor?.MarkAsFailure($"Customer ID {id} not found");
                    throw new EntityNotFoundException("Customer", id);
                }
                
                // Log read access for audit (only for sensitive operations)
                await _auditService?.LogUserActivityAsync(null, "READ", "Customer", id.ToString(), 
                    $"Accessed customer: {customer.FullName}");
                
                return customer;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer with ID: {CustomerId}", id);
                performanceMonitor?.MarkAsFailure(ex.Message);
                throw new BusinessException(
                    $"Error retrieving customer: {ex.Message}",
                    ex,
                    "Lỗi khi lấy thông tin khách hàng.",
                    "CUSTOMER_GET_ERROR");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var performanceMonitor = _auditService != null 
                ? new PerformanceMonitor(_auditService, _logger, "CustomerService.DeleteAsync", $"CustomerID: {id}")
                : null;
            
            try
            {
                _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
                
                // Get customer details for audit before deletion
                var customerToDelete = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customerToDelete == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found for deletion", id);
                    performanceMonitor?.MarkAsFailure($"Customer ID {id} not found");
                    throw new EntityNotFoundException("Customer", id);
                }
                
                var customerDetails = $"FullName: {customerToDelete.FullName}, CCCD: {customerToDelete.CCCD}";
                
                var result = await _unitOfWork.Customers.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully deleted customer with ID: {CustomerId}", id);
                
                // Log deletion for audit trail
                await _auditService?.LogUserActivityAsync(null, "DELETE", "Customer", id.ToString(), 
                    $"Deleted customer: {customerDetails}");
                    
                await _auditService?.LogBusinessOperationAsync("DELETE_CUSTOMER", "Customer", id.ToString(), 
                    null, true, $"Deleted: {customerDetails}");
                
                return true;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
                performanceMonitor?.MarkAsFailure(ex.Message);
                
                await _auditService?.LogBusinessOperationAsync("DELETE_CUSTOMER", "Customer", id.ToString(), 
                    null, false, $"Error: {ex.Message}");
                    
                throw new BusinessException(
                    $"Error deleting customer: {ex.Message}",
                    ex,
                    "Lỗi khi xóa khách hàng.",
                    "CUSTOMER_DELETE_ERROR");
            }
        }
    }
}
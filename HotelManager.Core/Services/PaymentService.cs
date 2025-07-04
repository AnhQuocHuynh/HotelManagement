using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManager.Exceptions;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using HotelManager.Utilities;
using System;

namespace HotelManager.Services
{
    public class PaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IAuditService _auditService;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IAuditService auditService = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task CreateAsync(Payment payment)
        {
            using var performanceMonitor = _auditService != null
                ? new PerformanceMonitor(_auditService, _logger, "PaymentService.CreateAsync", $"InvoiceId: {payment.InvoiceId}, Amount: {payment.Amount:C}")
                : null;

            try
            {
                _logger.LogInformation("Creating payment for InvoiceId: {InvoiceId}, Amount: {Amount:C}, Method: {PaymentMethod}", 
                    payment.InvoiceId, payment.Amount, payment.PaymentMethod);
                
                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Payment created successfully. PaymentId: {PaymentId}", payment.Id);
                
                // Audit trail for payment creation
                await _auditService?.LogUserActivityAsync(null, "CREATE", "Payment", payment.Id.ToString(),
                    $"Created payment: Amount {payment.Amount:C}, Method: {payment.PaymentMethod}, InvoiceId: {payment.InvoiceId}");
                    
                await _auditService?.LogBusinessOperationAsync("CREATE_PAYMENT", "Payment", payment.Id.ToString(),
                    null, true, $"Payment: {payment.Amount:C} for Invoice {payment.InvoiceId}");
            }
            catch (BusinessException)
            {
                performanceMonitor?.MarkAsFailure("Business exception during payment creation");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating payment: {Message}", ex.Message);
                performanceMonitor?.MarkAsFailure(ex.Message);
                
                await _auditService?.LogBusinessOperationAsync("CREATE_PAYMENT", "Payment", "unknown",
                    null, false, $"InvoiceId: {payment.InvoiceId}, Error: {ex.Message}");
                    
                throw new BusinessException($"Lỗi khi tạo payment: {ex.Message}", ex, "Lỗi khi tạo payment.", "PAYMENT_CREATE_ERROR");
            }
        }

        public async Task<Payment> GetByIdAsync(int id)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.Id == id);
            return payments.FirstOrDefault();
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _unitOfWork.Payments.GetAllAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            using var performanceMonitor = _auditService != null
                ? new PerformanceMonitor(_auditService, _logger, "PaymentService.UpdateAsync", $"PaymentId: {payment.Id}")
                : null;

            try
            {
                _logger.LogInformation("Updating payment. PaymentId: {PaymentId}", payment.Id);
                
                var payments = await _unitOfWork.Payments.FindAsync(p => p.Id == payment.Id);
                var existingPayment = payments.FirstOrDefault();
                
                if (existingPayment != null)
                {
                    // Capture old values for audit
                    var oldValues = $"Amount: {existingPayment.Amount:C}, Method: {existingPayment.PaymentMethod}, Date: {existingPayment.PaymentDate:yyyy-MM-dd}";
                    
                    existingPayment.PaymentDate = payment.PaymentDate;
                    existingPayment.Amount = payment.Amount;
                    existingPayment.PaymentMethod = payment.PaymentMethod;
                    
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Payment updated successfully. PaymentId: {PaymentId}", payment.Id);
                    
                    // Audit trail for payment update
                    var newValues = $"Amount: {payment.Amount:C}, Method: {payment.PaymentMethod}, Date: {payment.PaymentDate:yyyy-MM-dd}";
                    
                    await _auditService?.LogUserActivityAsync(null, "UPDATE", "Payment", payment.Id.ToString(),
                        $"Updated payment from [{oldValues}] to [{newValues}]");
                        
                    await _auditService?.LogBusinessOperationAsync("UPDATE_PAYMENT", "Payment", payment.Id.ToString(),
                        null, true, $"Updated payment: {payment.Amount:C}");
                }
                else
                {
                    _logger.LogWarning("Payment not found for update. PaymentId: {PaymentId}", payment.Id);
                    performanceMonitor?.MarkAsFailure($"Payment ID {payment.Id} not found");
                    throw new EntityNotFoundException("Payment", payment.Id);
                }
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating payment: {Message}", ex.Message);
                performanceMonitor?.MarkAsFailure(ex.Message);
                
                await _auditService?.LogBusinessOperationAsync("UPDATE_PAYMENT", "Payment", payment.Id.ToString(),
                    null, false, $"Error: {ex.Message}");
                    
                throw new BusinessException($"Error updating payment: {ex.Message}", ex, 
                    "Lỗi khi cập nhật thanh toán.", "PAYMENT_UPDATE_ERROR");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var performanceMonitor = _auditService != null
                ? new PerformanceMonitor(_auditService, _logger, "PaymentService.DeleteAsync", $"PaymentId: {id}")
                : null;

            try
            {
                _logger.LogInformation("Deleting payment. PaymentId: {PaymentId}", id);
                
                // Get payment details for audit before deletion
                var paymentToDelete = await GetByIdAsync(id);
                if (paymentToDelete == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found for deletion", id);
                    performanceMonitor?.MarkAsFailure($"Payment ID {id} not found");
                    throw new EntityNotFoundException("Payment", id);
                }
                
                var paymentDetails = $"Amount: {paymentToDelete.Amount:C}, Method: {paymentToDelete.PaymentMethod}, InvoiceId: {paymentToDelete.InvoiceId}";
                
                var result = await _unitOfWork.Payments.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Payment deleted successfully. PaymentId: {PaymentId}", id);
                
                // Audit trail for payment deletion
                await _auditService?.LogUserActivityAsync(null, "DELETE", "Payment", id.ToString(),
                    $"Deleted payment: {paymentDetails}");
                    
                await _auditService?.LogBusinessOperationAsync("DELETE_PAYMENT", "Payment", id.ToString(),
                    null, true, $"Deleted payment: {paymentDetails}");
                
                return result;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting payment: {Message}", ex.Message);
                performanceMonitor?.MarkAsFailure(ex.Message);
                
                await _auditService?.LogBusinessOperationAsync("DELETE_PAYMENT", "Payment", id.ToString(),
                    null, false, $"Error: {ex.Message}");
                    
                throw new BusinessException($"Error deleting payment: {ex.Message}", ex,
                    "Lỗi khi xóa thanh toán.", "PAYMENT_DELETE_ERROR");
            }
        }

        public async Task UpdateRemainingAmountAsync(int invoiceId)
        {
            var invoices = await _unitOfWork.Invoices.FindAsync(i => i.Id == invoiceId);
            var invoice = invoices.FirstOrDefault();
            if (invoice != null)
            {
                var totalPaid = await GetTotalPaidAsync(invoiceId);
                var remaining = invoice.TotalAmount - totalPaid;

                var payments = await _unitOfWork.Payments.GetAllAsync();
                foreach (var payment in payments)
                {
                    payment.RemainingAmount = remaining;
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalPaidAsync(int invoiceId)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.InvoiceId == invoiceId);
            return payments.Sum(p => p.Amount);
        }
    }
}
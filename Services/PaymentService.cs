using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManager.Exceptions;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManager.Services
{
    public class PaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task CreateAsync(Payment payment)
        {
            try
            {
                _logger.LogInformation("Creating payment for InvoiceId: {InvoiceId}, Amount: {Amount}", payment.InvoiceId, payment.Amount);
                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Payment created successfully. PaymentId: {PaymentId}", payment.Id);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception when creating payment: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating payment: {Message}", ex.Message);
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
            try
            {
                _logger.LogInformation("Updating payment. PaymentId: {PaymentId}", payment.Id);
                var payments = await _unitOfWork.Payments.FindAsync(p => p.Id == payment.Id);
                var existingPayment = payments.FirstOrDefault();
                if (existingPayment != null)
                {
                    existingPayment.PaymentDate = payment.PaymentDate;
                    existingPayment.Amount = payment.Amount;
                    existingPayment.PaymentMethod = payment.PaymentMethod;
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Payment updated successfully. PaymentId: {PaymentId}", payment.Id);
                }
                else
                {
                    _logger.LogWarning("Payment not found for update. PaymentId: {PaymentId}", payment.Id);
                    throw new Exception($"Payment with ID {payment.Id} not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating payment: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting payment. PaymentId: {PaymentId}", id);
                var result = await _unitOfWork.Payments.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Payment deleted successfully. PaymentId: {PaymentId}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting payment: {Message}", ex.Message);
                throw;
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
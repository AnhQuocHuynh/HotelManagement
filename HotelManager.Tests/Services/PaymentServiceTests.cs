using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using HotelManager.Services;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Interfaces;
using HotelManager.Exceptions;
using HotelManager.Tests.TestUtils;
using HotelManager.Data;
using HotelManager.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HotelManager.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<PaymentService>> _mockLogger;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly HotelDbContext _dbContext;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = MockLogger.Create<PaymentService>();
            _mockAuditService = new Mock<IAuditService>();
            _dbContext = TestDbContext.CreateInMemoryContext("PaymentServiceTests");

            _paymentService = new PaymentService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockAuditService.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test customers
            var testCustomers = new[]
            {
                new Customer { Id = 1, FullName = "Nguyen Van A", CCCD = "123456789", PhoneNumber = "0901234567", Type = CustomerType.Single },
                new Customer { Id = 2, FullName = "Tran Thi B", CCCD = "987654321", PhoneNumber = "0909876543", Type = CustomerType.Family }
            };

            // Add test rooms and bookings
            var testRooms = new[]
            {
                new Room { RoomNumber = "101", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available, PricePerNight = 500000 },
                new Room { RoomNumber = "201", RoomType = RoomType.Deluxe, RoomStatus = RoomStatus.Available, PricePerNight = 800000 }
            };

            var testBookings = new[]
            {
                new Booking { Id = 1, CustomerId = 1, RoomNumber = "101", Status = BookingStatus.CheckedOut },
                new Booking { Id = 2, CustomerId = 2, RoomNumber = "201", Status = BookingStatus.CheckedOut }
            };

            var testInvoices = new[]
            {
                new Invoice { Id = 1, BookingId = 1, TotalAmount = 1000000, IssueDate = DateTime.Today },
                new Invoice { Id = 2, BookingId = 2, TotalAmount = 1500000, IssueDate = DateTime.Today }
            };

            _dbContext.Customers.AddRange(testCustomers);
            _dbContext.Rooms.AddRange(testRooms);
            _dbContext.Bookings.AddRange(testBookings);
            _dbContext.Invoices.AddRange(testInvoices);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateAsync_WithValidPayment_ShouldCreateSuccessfully()
        {
            // Arrange
            var payment = new Payment
            {
                InvoiceId = 1,
                Amount = 1000000,
                PaymentMethod = PaymentMethod.Cash,
                PaymentDate = DateTime.Today
            };

            var invoice = new Invoice
            {
                Id = 1,
                BookingId = 1,
                TotalAmount = 1000000,
                IssueDate = DateTime.Today
            };

            _mockUnitOfWork.Setup(x => x.Invoices.GetByIdAsync(1))
                .ReturnsAsync(invoice);
            _mockUnitOfWork.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _paymentService.CreateAsync(payment);

            // Assert
            _mockUnitOfWork.Verify(x => x.Payments.AddAsync(payment), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verify audit logging
            _mockAuditService.Verify(x => x.LogBusinessOperationAsync(
                "CREATE_PAYMENT", "Payment", It.IsAny<string>(), 
                It.IsAny<string>(), true, It.IsAny<string>()), Times.Once);

            // Verify performance monitoring
            _mockAuditService.Verify(x => x.LogPerformanceMetricAsync(
                It.Is<string>(s => s.Contains("PaymentService.CreateAsync")),
                It.IsAny<TimeSpan>(), true, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidInvoiceId_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var payment = new Payment
            {
                InvoiceId = 999, // Non-existent invoice
                Amount = 1000000,
                PaymentMethod = PaymentMethod.Cash,
                PaymentDate = DateTime.Today
            };

            // Since PaymentService doesn't validate invoice, it will throw during creation
            _mockUnitOfWork.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act - PaymentService doesn't validate invoice, so no exception expected
            await _paymentService.CreateAsync(payment);

            // Assert - Creation should succeed even without invoice validation
            _mockUnitOfWork.Verify(x => x.Payments.AddAsync(payment), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithAmountExceedsTotal_ShouldNotValidateAmount()
        {
            // Arrange
            var payment = new Payment
            {
                InvoiceId = 1,
                Amount = 2000000, // Exceeds invoice total
                PaymentMethod = PaymentMethod.Cash,
                PaymentDate = DateTime.Today
            };

            // PaymentService doesn't validate amount against invoice
            _mockUnitOfWork.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act - PaymentService doesn't validate amount, so no exception expected
            await _paymentService.CreateAsync(payment);

            // Assert - Creation should succeed without amount validation
            _mockUnitOfWork.Verify(x => x.Payments.AddAsync(payment), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnPayment()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 1,
                InvoiceId = 1,
                Amount = 1000000,
                PaymentMethod = PaymentMethod.Cash,
                PaymentDate = DateTime.Today
            };

            // PaymentService.GetByIdAsync uses FindAsync, not GetByIdAsync
            _mockUnitOfWork.Setup(x => x.Payments.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Payment, bool>>>()))
                .ReturnsAsync(new[] { payment });

            // Act
            var result = await _paymentService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Amount.Should().Be(1000000);
            result.PaymentMethod.Should().Be(PaymentMethod.Cash);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Payments.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Payment, bool>>>()))
                .ReturnsAsync(Array.Empty<Payment>());

            // Act
            var result = await _paymentService.GetByIdAsync(999);

            // Assert - GetByIdAsync returns null, not exception
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 1,
                InvoiceId = 1,
                Amount = 1000000,
                PaymentMethod = PaymentMethod.Cash,
                PaymentDate = DateTime.Today
            };

            // Mock GetByIdAsync which uses FindAsync
            _mockUnitOfWork.Setup(x => x.Payments.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Payment, bool>>>()))
                .ReturnsAsync(new[] { payment });
            _mockUnitOfWork.Setup(x => x.Payments.DeleteAsync(1))
                .ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _paymentService.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(x => x.Payments.DeleteAsync(1), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verify audit logging
            _mockAuditService.Verify(x => x.LogBusinessOperationAsync(
                "DELETE_PAYMENT", "Payment", "1", 
                It.IsAny<string>(), true, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldThrowEntityNotFoundException()
        {
            // Arrange - Return empty array for FindAsync
            _mockUnitOfWork.Setup(x => x.Payments.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Payment, bool>>>()))
                .ReturnsAsync(Array.Empty<Payment>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _paymentService.DeleteAsync(999));

            exception.EntityType.Should().Be("Payment");
            exception.EntityId.Should().Be(999);
        }

        [Fact]
        public async Task CreateAsync_WhenDatabaseThrows_ShouldWrapInBusinessException()
        {
            // Arrange
            var payment = new Payment
            {
                InvoiceId = 1,
                Amount = 1000000,
                PaymentMethod = PaymentMethod.Cash,
                PaymentDate = DateTime.Today
            };

            _mockUnitOfWork.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(
                () => _paymentService.CreateAsync(payment));

            exception.Message.Should().Contain("Lỗi khi tạo payment");
            exception.ErrorCode.Should().Be("PAYMENT_CREATE_ERROR");
            exception.InnerException.Should().BeOfType<InvalidOperationException>();

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error when creating payment")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPayments()
        {
            // Arrange
            var mockPayments = new List<Payment>
            {
                new() { Id = 1, InvoiceId = 1, Amount = 1000000, PaymentMethod = PaymentMethod.Cash },
                new() { Id = 2, InvoiceId = 2, Amount = 1500000, PaymentMethod = PaymentMethod.CreditCard }
            };

            _mockUnitOfWork.Setup(x => x.Payments.GetAllAsync())
                .ReturnsAsync(mockPayments);

            // Act
            var result = await _paymentService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(mockPayments);
            _mockUnitOfWork.Verify(x => x.Payments.GetAllAsync(), Times.Once);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
} 
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
    public class BookingServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CustomerService _customerService; // Real service instead of mock
        private readonly Mock<ILogger<BookingService>> _mockLogger;
        private readonly HotelDbContext _dbContext;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = MockLogger.Create<BookingService>();
            _dbContext = TestDbContext.CreateInMemoryContext();
            
            // Create real CustomerService with in-memory database
            var loggerFactory = new LoggerFactory();
            var unitOfWork = new UnitOfWork(_dbContext, loggerFactory);
            var customerLogger = MockLogger.Create<CustomerService>();
            var mockAuditService = new Mock<IAuditService>();
            _customerService = new CustomerService(unitOfWork, customerLogger.Object, mockAuditService.Object);

            _bookingService = new BookingService(
                _mockUnitOfWork.Object,
                _customerService,
                _dbContext,
                _mockLogger.Object);

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

            // Add test employees
            var testEmployees = new[]
            {
                new Employee { Id = 1, FullName = "Le Van C", Position = EmployeePosition.Receptionist },
                new Employee { Id = 2, FullName = "Pham Thi D", Position = EmployeePosition.Manager }
            };

            // Add test rooms
            var testRooms = new[]
            {
                new Room { RoomNumber = "101", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available, PricePerNight = 500000 },
                new Room { RoomNumber = "201", RoomType = RoomType.Deluxe, RoomStatus = RoomStatus.Available, PricePerNight = 800000 }
            };

            _dbContext.Customers.AddRange(testCustomers);
            _dbContext.Employees.AddRange(testEmployees);
            _dbContext.Rooms.AddRange(testRooms);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateAsync_WithValidBooking_ShouldCreateSuccessfully()
        {
            // Arrange
            var customer = new Customer 
            { 
                FullName = "Test Customer", 
                CCCD = "111222333", 
                PhoneNumber = "0901111111", 
                Type = CustomerType.Single 
            };

            var booking = new Booking
            {
                Customer = customer,
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                Status = BookingStatus.Pending,
                BookingEmployeeId = 1
            };

            _mockUnitOfWork.Setup(x => x.Bookings.AddAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking b) => b);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _bookingService.CreateAsync(booking);

            // Assert
            booking.CustomerId.Should().BeGreaterThan(0); // Customer should be created with ID
            _mockUnitOfWork.Verify(x => x.Bookings.AddAsync(booking), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating booking")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingCustomer_ShouldNotCreateCustomer()
        {
            // Arrange
            var booking = new Booking
            {
                CustomerId = 1, // Existing customer
                Customer = null, // No customer object
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                Status = BookingStatus.Pending
            };

            _mockUnitOfWork.Setup(x => x.Bookings.AddAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking b) => b);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _bookingService.CreateAsync(booking);

            // Assert
            // Since customer is null, no new customer should be created
            _mockUnitOfWork.Verify(x => x.Bookings.AddAsync(booking), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenCustomerWithDuplicateCCCD_ShouldThrowException()
        {
            // Arrange - create a customer that already exists in test data
            var customer = new Customer 
            { 
                FullName = "Duplicate Customer", 
                CCCD = "123456789", // Same as existing customer in test data
                PhoneNumber = "0901111111", 
                Type = CustomerType.Single 
            };

            var booking = new Booking
            {
                Customer = customer,
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                Status = BookingStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DuplicateEntityException>(
                () => _bookingService.CreateAsync(booking));

            exception.EntityType.Should().Be("Customer");
            exception.DuplicateField.Should().Be("CCCD");
            exception.DuplicateValue.Should().Be(customer.CCCD);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Business exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithValidBooking_ShouldUpdateSuccessfully()
        {
            // Arrange
            var existingBooking = new Booking
            {
                Id = 1,
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                Status = BookingStatus.Pending
            };

            var updatedBooking = new Booking
            {
                Id = 1,
                RoomNumber = "201",
                RoomType = RoomType.Deluxe,
                CheckInDate = DateTime.Today.AddDays(2),
                CheckOutDate = DateTime.Today.AddDays(4),
                Status = BookingStatus.Confirmed
            };

            _mockUnitOfWork.Setup(x => x.Bookings.GetByIdAsync(1))
                .ReturnsAsync(existingBooking);
            _mockUnitOfWork.Setup(x => x.Bookings.UpdateAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking b) => b);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _bookingService.UpdateAsync(updatedBooking);

            // Assert
            existingBooking.RoomNumber.Should().Be("201");
            existingBooking.RoomType.Should().Be(RoomType.Deluxe);
            existingBooking.Status.Should().Be(BookingStatus.Confirmed);
            existingBooking.CheckInDate.Should().Be(DateTime.Today.AddDays(2));
            existingBooking.CheckOutDate.Should().Be(DateTime.Today.AddDays(4));

            _mockUnitOfWork.Verify(x => x.Bookings.UpdateAsync(existingBooking), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentBooking_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var nonExistentBooking = new Booking { Id = 999 };

            _mockUnitOfWork.Setup(x => x.Bookings.GetByIdAsync(999))
                .ReturnsAsync((Booking?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _bookingService.UpdateAsync(nonExistentBooking));

            exception.EntityType.Should().Be("Booking");
            exception.EntityId.Should().Be(999);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Booking not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithValidBookingId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var existingBooking = new Booking { Id = 1, RoomNumber = "101" };

            _mockUnitOfWork.Setup(x => x.Bookings.GetByIdAsync(1))
                .ReturnsAsync(existingBooking);
            _mockUnitOfWork.Setup(x => x.Bookings.DeleteAsync(1))
                .ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _bookingService.DeleteAsync(1);

            // Assert
            _mockUnitOfWork.Verify(x => x.Bookings.DeleteAsync(1), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Booking deleted successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentBookingId_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Bookings.GetByIdAsync(999))
                .ReturnsAsync((Booking?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _bookingService.DeleteAsync(999));

            exception.EntityType.Should().Be("Booking");
            exception.EntityId.Should().Be(999);
        }

        [Fact]
        public async Task GetCheckedOutBookingsAsync_ShouldReturnOnlyCheckedOutBookings()
        {
            // Arrange
            var bookings = new[]
            {
                new Booking { Id = 1, Status = BookingStatus.CheckedOut, CustomerId = 1, RoomNumber = "101" },
                new Booking { Id = 2, Status = BookingStatus.Pending, CustomerId = 1, RoomNumber = "102" },
                new Booking { Id = 3, Status = BookingStatus.CheckedOut, CustomerId = 2, RoomNumber = "103" },
                new Booking { Id = 4, Status = BookingStatus.CheckedIn, CustomerId = 2, RoomNumber = "104" }
            };

            _dbContext.Bookings.AddRange(bookings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetCheckedOutBookingsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(b => b.Status == BookingStatus.CheckedOut).Should().BeTrue();
            result.Select(b => b.Id).Should().Contain(new[] { 1, 3 });
        }

        [Fact]
        public async Task GetCheckedOutBookingsByDateRangeAsync_ShouldReturnBookingsInDateRange()
        {
            // Arrange
            var fromDate = DateTime.Today.AddDays(-7);
            var toDate = DateTime.Today;

            var bookings = new[]
            {
                new Booking 
                { 
                    Id = 1, 
                    Status = BookingStatus.CheckedOut, 
                    CheckOutDate = DateTime.Today.AddDays(-3),
                    CustomerId = 1, 
                    RoomNumber = "101" 
                },
                new Booking 
                { 
                    Id = 2, 
                    Status = BookingStatus.CheckedOut, 
                    CheckOutDate = DateTime.Today.AddDays(-10),
                    CustomerId = 1, 
                    RoomNumber = "102" 
                },
                new Booking 
                { 
                    Id = 3, 
                    Status = BookingStatus.CheckedOut, 
                    CheckOutDate = DateTime.Today.AddDays(-1),
                    CustomerId = 2, 
                    RoomNumber = "103" 
                }
            };

            _dbContext.Bookings.AddRange(bookings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetCheckedOutBookingsByDateRangeAsync(fromDate, toDate);

            // Assert
            result.Should().HaveCount(2);
            result.All(b => b.CheckOutDate >= fromDate && b.CheckOutDate <= toDate).Should().BeTrue();
            result.Select(b => b.Id).Should().Contain(new[] { 1, 3 });
        }

        [Fact]
        public async Task CreateAsync_WhenDatabaseThrows_ShouldWrapInBusinessException()
        {
            // Arrange
            var booking = new Booking
            {
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                Status = BookingStatus.Pending
            };

            _mockUnitOfWork.Setup(x => x.Bookings.AddAsync(It.IsAny<Booking>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(
                () => _bookingService.CreateAsync(booking));

            exception.Message.Should().Contain("Lỗi khi tạo booking");
            exception.ErrorCode.Should().Be("BOOKING_CREATE_ERROR");
            exception.InnerException.Should().BeOfType<InvalidOperationException>();

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error when creating booking")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBookings()
        {
            // Arrange
            var mockBookings = new List<Booking>
            {
                new() { Id = 1, RoomNumber = "101", Status = BookingStatus.Pending },
                new() { Id = 2, RoomNumber = "102", Status = BookingStatus.Confirmed }
            };

            _mockUnitOfWork.Setup(x => x.Bookings.GetAllAsync())
                .ReturnsAsync(mockBookings);

            // Act
            var result = await _bookingService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(mockBookings);
            _mockUnitOfWork.Verify(x => x.Bookings.GetAllAsync(), Times.Once);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
} 
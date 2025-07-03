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
    public class RoomServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<RoomService>> _mockLogger;
        private readonly HotelDbContext _dbContext;
        private readonly RoomService _roomService;

        public RoomServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = MockLogger.Create<RoomService>();
            _dbContext = TestDbContext.CreateInMemoryContext("RoomServiceTests");

            _roomService = new RoomService(
                _mockUnitOfWork.Object,
                _dbContext,
                _mockLogger.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test rooms
            var testRooms = new[]
            {
                new Room { RoomNumber = "101", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available, PricePerNight = 500000 },
                new Room { RoomNumber = "102", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Occupied, PricePerNight = 500000 },
                new Room { RoomNumber = "201", RoomType = RoomType.Deluxe, RoomStatus = RoomStatus.Available, PricePerNight = 800000 },
                new Room { RoomNumber = "301", RoomType = RoomType.Suite, RoomStatus = RoomStatus.UnderMaintenance, PricePerNight = 1200000 }
            };

            _dbContext.Rooms.AddRange(testRooms);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateAsync_WithValidRoom_ShouldCreateSuccessfully()
        {
            // Arrange
            var room = new Room
            {
                RoomNumber = "401",
                RoomType = RoomType.Deluxe,
                RoomStatus = RoomStatus.Available,
                PricePerNight = 850000
            };

            _mockUnitOfWork.Setup(x => x.Rooms.AddAsync(It.IsAny<Room>()))
                .ReturnsAsync((Room r) => r);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _roomService.CreateAsync(room);

            // Assert
            result.Should().NotBeNull();
            result.RoomNumber.Should().Be("401");
            result.RoomType.Should().Be(RoomType.Deluxe);
            result.PricePerNight.Should().Be(850000);

            _mockUnitOfWork.Verify(x => x.Rooms.AddAsync(room), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating room")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenDatabaseThrows_ShouldPropagateException()
        {
            // Arrange
            var room = new Room
            {
                RoomNumber = "401",
                RoomType = RoomType.Deluxe,
                RoomStatus = RoomStatus.Available,
                PricePerNight = 850000
            };

            _mockUnitOfWork.Setup(x => x.Rooms.AddAsync(It.IsAny<Room>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _roomService.CreateAsync(room));

            exception.Message.Should().Be("Database connection failed");

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error when creating room")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnRoom()
        {
            // Arrange
            var room = new Room
            {
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                RoomStatus = RoomStatus.Available,
                PricePerNight = 500000
            };

            _mockUnitOfWork.Setup(x => x.Rooms.GetByIdAsync(1))
                .ReturnsAsync(room);

            // Act
            var result = await _roomService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.RoomNumber.Should().Be("101");
            result.RoomType.Should().Be(RoomType.Standard);
            result.PricePerNight.Should().Be(500000);

            _mockUnitOfWork.Verify(x => x.Rooms.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Rooms.GetByIdAsync(999))
                .ReturnsAsync((Room?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _roomService.GetByIdAsync(999));

            exception.EntityType.Should().Be("Room");
            exception.EntityId.Should().Be(999);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Room not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithValidRoom_ShouldUpdateSuccessfully()
        {
            // Arrange
            var room = new Room
            {
                RoomNumber = "101",
                RoomType = RoomType.Deluxe, // Updated from Standard
                RoomStatus = RoomStatus.Available,
                PricePerNight = 750000 // Updated price
            };

            _mockUnitOfWork.Setup(x => x.Rooms.UpdateAsync(It.IsAny<Room>()))
                .ReturnsAsync((Room r) => r);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _roomService.UpdateAsync(room);

            // Assert
            result.Should().NotBeNull();
            result.RoomNumber.Should().Be("101");
            result.RoomType.Should().Be(RoomType.Deluxe);
            result.PricePerNight.Should().Be(750000);

            _mockUnitOfWork.Verify(x => x.Rooms.UpdateAsync(room), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Room updated successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ByIdShouldThrowNotSupportedException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotSupportedException>(
                () => _roomService.DeleteAsync(1));

            exception.Message.Should().Contain("Room entity does not support deletion by Id");

            // Verify warning logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Delete by Id is not supported")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ByRoomNumber_WithValidRoom_ShouldDeleteSuccessfully()
        {
            // Arrange - Use a unique room number not in seed data
            var uniqueRoomNumber = "999";
            var room = new Room
            {
                RoomNumber = uniqueRoomNumber,
                RoomType = RoomType.Standard,
                RoomStatus = RoomStatus.Available,
                PricePerNight = 500000
            };

            // Add room directly to the test's db context
            _dbContext.Rooms.Add(room);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _roomService.DeleteAsync(uniqueRoomNumber);

            // Assert
            result.Should().BeTrue();

            // Verify room was removed from context
            var deletedRoom = await _dbContext.Rooms.FindAsync(uniqueRoomNumber);
            deletedRoom.Should().BeNull();

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Room deleted successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ByRoomNumber_WithNonExistentRoom_ShouldReturnFalse()
        {
            // Act
            var result = await _roomService.DeleteAsync("999");

            // Assert
            result.Should().BeFalse();

            // Verify warning logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Room not found for delete")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task IsRoomAvailableAsync_WithAvailableRoom_ShouldReturnTrue()
        {
            // Arrange
            var availableRoom = new Room
            {
                RoomNumber = "101",
                RoomStatus = RoomStatus.Available
            };

            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(new[] { availableRoom });

            // Act
            var result = await _roomService.IsRoomAvailableAsync("101");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsRoomAvailableAsync_WithOccupiedRoom_ShouldReturnFalse()
        {
            // Arrange
            var occupiedRoom = new Room
            {
                RoomNumber = "102",
                RoomStatus = RoomStatus.Occupied
            };

            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(new[] { occupiedRoom });

            // Act
            var result = await _roomService.IsRoomAvailableAsync("102");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRoomAvailableAsync_WithNonExistentRoom_ShouldReturnFalse()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(Array.Empty<Room>());

            // Act
            var result = await _roomService.IsRoomAvailableAsync("999");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAvailableRoomsByTypeAsync_ShouldReturnOnlyAvailableRooms()
        {
            // Arrange
            var availableStandardRooms = new[]
            {
                new Room { RoomNumber = "101", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available },
                new Room { RoomNumber = "103", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available }
            };

            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(availableStandardRooms);

            // Act
            var result = await _roomService.GetAvailableRoomsByTypeAsync(RoomType.Standard);

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.RoomType == RoomType.Standard).Should().BeTrue();
            result.All(r => r.RoomStatus == RoomStatus.Available).Should().BeTrue();
        }

        [Fact]
        public async Task GetAvailableRoomsAsync_ShouldReturnAllAvailableRooms()
        {
            // Arrange
            var availableRooms = new[]
            {
                new Room { RoomNumber = "101", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available },
                new Room { RoomNumber = "201", RoomType = RoomType.Deluxe, RoomStatus = RoomStatus.Available }
            };

            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(availableRooms);

            // Act
            var result = await _roomService.GetAvailableRoomsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.RoomStatus == RoomStatus.Available).Should().BeTrue();
        }

        [Fact]
        public async Task GetByRoomNumberAsync_WithValidRoomNumber_ShouldReturnRoom()
        {
            // Arrange
            var room = new Room
            {
                RoomNumber = "101",
                RoomType = RoomType.Standard,
                RoomStatus = RoomStatus.Available,
                PricePerNight = 500000
            };

            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(new[] { room });

            // Act
            var result = await _roomService.GetByRoomNumberAsync("101");

            // Assert
            result.Should().NotBeNull();
            result!.RoomNumber.Should().Be("101");
            result.RoomType.Should().Be(RoomType.Standard);
        }

        [Fact]
        public async Task GetByRoomNumberAsync_WithInvalidRoomNumber_ShouldReturnNull()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Rooms.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>()))
                .ReturnsAsync(Array.Empty<Room>());

            // Act
            var result = await _roomService.GetByRoomNumberAsync("999");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRooms()
        {
            // Arrange
            var mockRooms = new List<Room>
            {
                new() { RoomNumber = "101", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Available },
                new() { RoomNumber = "102", RoomType = RoomType.Standard, RoomStatus = RoomStatus.Occupied },
                new() { RoomNumber = "201", RoomType = RoomType.Deluxe, RoomStatus = RoomStatus.Available }
            };

            _mockUnitOfWork.Setup(x => x.Rooms.GetAllAsync())
                .ReturnsAsync(mockRooms);

            // Act
            var result = await _roomService.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(mockRooms);
            _mockUnitOfWork.Verify(x => x.Rooms.GetAllAsync(), Times.Once);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
} 
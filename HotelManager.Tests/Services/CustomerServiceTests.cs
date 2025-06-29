using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using HotelManager.Services;
using HotelManager.Models;
using HotelManager.Interfaces;
using HotelManager.Exceptions;
using HotelManager.Tests.TestUtils;
using HotelManager.Data;
using HotelManager.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HotelManager.Tests.Services
{
    public class CustomerServiceTests
    {
        [Fact]
        public async Task CreateAsync_ShouldCreateCustomer_WhenValid()
        {
            // Arrange
            var context = TestDbContext.CreateInMemoryContext();
            var unitOfWork = new UnitOfWork(context, new LoggerFactory());
            var logger = MockLogger.Create<CustomerService>();
            var audit = new Mock<IAuditService>();
            var service = new CustomerService(unitOfWork, logger.Object, audit.Object);
            var customer = new Customer { CCCD = "123456789", FullName = "Test" };

            // Act
            await service.CreateAsync(customer);

            // Assert
            var created = await context.Customers.FirstOrDefaultAsync(c => c.CCCD == "123456789");
            created.Should().NotBeNull();
            created.FullName.Should().Be("Test");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenDuplicateCCCD()
        {
            // Arrange
            var context = TestDbContext.CreateInMemoryContext();
            var unitOfWork = new UnitOfWork(context, new LoggerFactory());
            var logger = MockLogger.Create<CustomerService>();
            var audit = new Mock<IAuditService>();
            var service = new CustomerService(unitOfWork, logger.Object, audit.Object);
            var customer = new Customer { CCCD = "123456789", FullName = "Test" };
            await service.CreateAsync(customer);

            // Act
            var duplicate = new Customer { CCCD = "123456789", FullName = "Dup" };
            var act = async () => await service.CreateAsync(duplicate);

            // Assert
            await act.Should().ThrowAsync<DuplicateEntityException>();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCustomer_WhenValid()
        {
            // Arrange
            var context = TestDbContext.CreateInMemoryContext();
            var unitOfWork = new UnitOfWork(context, new LoggerFactory());
            var logger = MockLogger.Create<CustomerService>();
            var audit = new Mock<IAuditService>();
            var service = new CustomerService(unitOfWork, logger.Object, audit.Object);
            var customer = new Customer { CCCD = "123456789", FullName = "Test" };
            await service.CreateAsync(customer);
            var created = await context.Customers.FirstOrDefaultAsync(c => c.CCCD == "123456789");
            created!.FullName = "Updated";

            // Act
            await service.UpdateAsync(created);

            // Assert
            var updated = await context.Customers.FirstOrDefaultAsync(c => c.CCCD == "123456789");
            updated!.FullName.Should().Be("Updated");
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCustomer_WhenExists()
        {
            // Arrange
            var context = TestDbContext.CreateInMemoryContext();
            var unitOfWork = new UnitOfWork(context, new LoggerFactory());
            var logger = MockLogger.Create<CustomerService>();
            var audit = new Mock<IAuditService>();
            var service = new CustomerService(unitOfWork, logger.Object, audit.Object);
            var customer = new Customer { CCCD = "123456789", FullName = "Test" };
            await service.CreateAsync(customer);
            var created = await context.Customers.FirstOrDefaultAsync(c => c.CCCD == "123456789");

            // Act
            var result = await service.DeleteAsync(created.Id);

            // Assert
            result.Should().BeTrue();
            (await context.Customers.AnyAsync(c => c.CCCD == "123456789")).Should().BeFalse();
        }
    }
} 
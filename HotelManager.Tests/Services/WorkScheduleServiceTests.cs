using HotelManager.Core.Services;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HotelManager.Tests.Services
{
    public class WorkScheduleServiceTests
    {
        private readonly Mock<IWorkScheduleRepository> _mockWorkScheduleRepository;
        private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
        private readonly Mock<ILogger<WorkScheduleService>> _mockLogger;
        private readonly WorkScheduleService _service;

        public WorkScheduleServiceTests()
        {
            _mockWorkScheduleRepository = new Mock<IWorkScheduleRepository>();
            _mockEmployeeRepository = new Mock<IRepository<Employee>>();
            _mockLogger = new Mock<ILogger<WorkScheduleService>>();
            
            _service = new WorkScheduleService(
                _mockWorkScheduleRepository.Object,
                _mockEmployeeRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task AssignScheduleAsync_ValidInput_ShouldCreateSchedule()
        {
            // Arrange
            var employeeId = 1;
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(1);
            var assignedByEmployeeId = 2;
            var notes = "Test schedule";

            var employee = new Employee { Id = employeeId, FullName = "Test Employee" };
            var expectedSchedule = new WorkSchedule
            {
                Id = 1,
                EmployeeId = employeeId,
                WorkDay = day,
                Shift = shift,
                StartDate = startDate,
                Status = ScheduleStatus.Scheduled,
                Notes = notes,
                AssignedByEmployeeId = assignedByEmployeeId,
                CreatedDate = DateTime.Now
            };

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);
            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(employeeId, day, shift, startDate, null))
                .ReturnsAsync(false);
            _mockWorkScheduleRepository.Setup(r => r.AddAsync(It.IsAny<WorkSchedule>()))
                .ReturnsAsync(expectedSchedule);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.AssignScheduleAsync(employeeId, day, shift, startDate, null, assignedByEmployeeId, notes);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employeeId, result.EmployeeId);
            Assert.Equal(day, result.WorkDay);
            Assert.Equal(shift, result.Shift);
            Assert.Equal(startDate, result.StartDate);
            Assert.Equal(notes, result.Notes);
            Assert.Equal(ScheduleStatus.Scheduled, result.Status);

            _mockWorkScheduleRepository.Verify(r => r.AddAsync(It.IsAny<WorkSchedule>()), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignScheduleAsync_PastStartDate_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 1;
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(-1); // Past date

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.AssignScheduleAsync(employeeId, day, shift, startDate, null, 2));
        }

        [Fact]
        public async Task AssignScheduleAsync_EndDateBeforeStartDate_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 1;
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(2);
            var endDate = DateTime.Today.AddDays(1); // Before start date

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.AssignScheduleAsync(employeeId, day, shift, startDate, endDate, 2));
        }

        [Fact]
        public async Task AssignScheduleAsync_EmployeeNotFound_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 999;
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(1);

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.AssignScheduleAsync(employeeId, day, shift, startDate, null, 2));
        }

        [Fact]
        public async Task AssignScheduleAsync_ConflictDetected_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 1;
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(1);

            var employee = new Employee { Id = employeeId, FullName = "Test Employee" };

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);
            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(employeeId, day, shift, startDate, null))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.AssignScheduleAsync(employeeId, day, shift, startDate, null, 2));
        }

        [Fact]
        public async Task GetEmployeeScheduleAsync_ValidInput_ShouldReturnSchedules()
        {
            // Arrange
            var employeeId = 1;
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);

            var schedules = new List<WorkSchedule>
            {
                new WorkSchedule { Id = 1, EmployeeId = employeeId, WorkDay = WorkDay.Monday, Shift = WorkShift.Morning, StartDate = startDate.AddDays(1) },
                new WorkSchedule { Id = 2, EmployeeId = employeeId, WorkDay = WorkDay.Tuesday, Shift = WorkShift.Afternoon, StartDate = startDate.AddDays(2) }
            };

            _mockWorkScheduleRepository.Setup(r => r.GetByEmployeeAsync(employeeId))
                .ReturnsAsync(schedules);

            // Act
            var result = await _service.GetEmployeeScheduleAsync(employeeId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(WorkDay.Monday, result[0].WorkDay);
            Assert.Equal(WorkDay.Tuesday, result[1].WorkDay);
        }

        [Fact]
        public async Task GetEmployeeScheduleAsync_InvalidDateRange_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 1;
            var startDate = DateTime.Today.AddDays(2);
            var endDate = DateTime.Today.AddDays(1); // Before start date

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.GetEmployeeScheduleAsync(employeeId, startDate, endDate));
        }

        [Fact]
        public async Task GetWeeklyScheduleAsync_ValidInput_ShouldReturnWeeklySchedules()
        {
            // Arrange
            var weekStart = DateTime.Today;
            var schedules = new List<WorkSchedule>
            {
                new WorkSchedule { Id = 1, EmployeeId = 1, WorkDay = WorkDay.Monday, Shift = WorkShift.Morning, StartDate = weekStart },
                new WorkSchedule { Id = 2, EmployeeId = 2, WorkDay = WorkDay.Tuesday, Shift = WorkShift.Afternoon, StartDate = weekStart.AddDays(1) }
            };

            _mockWorkScheduleRepository.Setup(r => r.GetWeeklySchedulesAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(schedules);

            // Act
            var result = await _service.GetWeeklyScheduleAsync(weekStart);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task ValidateScheduleConflictAsync_ValidInput_ShouldReturnConflictStatus()
        {
            // Arrange
            var employeeId = 1;
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var date = DateTime.Today.AddDays(1);
            var hasConflict = true;

            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(employeeId, day, shift, date, null))
                .ReturnsAsync(hasConflict);

            // Act
            var result = await _service.ValidateScheduleConflictAsync(employeeId, day, shift, date);

            // Assert
            Assert.Equal(hasConflict, result);
        }

        [Fact]
        public async Task UpdateScheduleStatusAsync_ValidInput_ShouldUpdateStatus()
        {
            // Arrange
            var scheduleId = 1;
            var newStatus = ScheduleStatus.Completed;
            var notes = "Completed successfully";

            var existingSchedule = new WorkSchedule
            {
                Id = scheduleId,
                EmployeeId = 1,
                WorkDay = WorkDay.Monday,
                Shift = WorkShift.Morning,
                StartDate = DateTime.Today.AddDays(1),
                Status = ScheduleStatus.Scheduled,
                CreatedDate = DateTime.Now
            };

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync(existingSchedule);
            _mockWorkScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkSchedule>()))
                .ReturnsAsync(existingSchedule);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateScheduleStatusAsync(scheduleId, newStatus, notes);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newStatus, result.Status);
            Assert.Contains("Completed successfully", result.Notes);
            Assert.NotNull(result.UpdatedDate);

            _mockWorkScheduleRepository.Verify(r => r.UpdateAsync(It.IsAny<WorkSchedule>()), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateScheduleStatusAsync_ScheduleNotFound_ShouldThrowValidationException()
        {
            // Arrange
            var scheduleId = 999;
            var newStatus = ScheduleStatus.Completed;

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync((WorkSchedule)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.UpdateScheduleStatusAsync(scheduleId, newStatus));
        }

        [Fact]
        public async Task BulkAssignScheduleAsync_ValidInput_ShouldCreateMultipleSchedules()
        {
            // Arrange
            var employeeId = 1;
            var days = new List<WorkDay> { WorkDay.Monday, WorkDay.Wednesday, WorkDay.Friday };
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(1);
            var endDate = DateTime.Today.AddDays(7);
            var assignedByEmployeeId = 2;

            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(It.IsAny<int>(), It.IsAny<WorkDay>(), It.IsAny<WorkShift>(), It.IsAny<DateTime>(), null))
                .ReturnsAsync(false);
            _mockWorkScheduleRepository.Setup(r => r.AddAsync(It.IsAny<WorkSchedule>()))
                .ReturnsAsync((WorkSchedule s) => s);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.BulkAssignScheduleAsync(employeeId, days, shift, startDate, endDate, assignedByEmployeeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.All(result, schedule => Assert.Equal(employeeId, schedule.EmployeeId));
            Assert.All(result, schedule => Assert.Equal(shift, schedule.Shift));
            Assert.All(result, schedule => Assert.Contains(schedule.WorkDay, days));

            _mockWorkScheduleRepository.Verify(r => r.AddAsync(It.IsAny<WorkSchedule>()), Times.AtLeastOnce);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task BulkAssignScheduleAsync_InvalidDateRange_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 1;
            var days = new List<WorkDay> { WorkDay.Monday };
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(2);
            var endDate = DateTime.Today.AddDays(1); // Before start date
            var assignedByEmployeeId = 2;

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.BulkAssignScheduleAsync(employeeId, days, shift, startDate, endDate, assignedByEmployeeId));
        }

        [Fact]
        public async Task BulkAssignScheduleAsync_EmptyDaysList_ShouldThrowValidationException()
        {
            // Arrange
            var employeeId = 1;
            var days = new List<WorkDay>(); // Empty list
            var shift = WorkShift.Morning;
            var startDate = DateTime.Today.AddDays(1);
            var endDate = DateTime.Today.AddDays(7);
            var assignedByEmployeeId = 2;

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.BulkAssignScheduleAsync(employeeId, days, shift, startDate, endDate, assignedByEmployeeId));
        }

        [Fact]
        public async Task GetEmployeeWorkloadSummaryAsync_ValidInput_ShouldReturnSummary()
        {
            // Arrange
            var employeeId = 1;
            var month = DateTime.Today;

            var schedules = new List<WorkSchedule>
            {
                new WorkSchedule { Id = 1, EmployeeId = employeeId, WorkDay = WorkDay.Monday, Shift = WorkShift.Morning, StartDate = month.AddDays(1), Status = ScheduleStatus.Scheduled },
                new WorkSchedule { Id = 2, EmployeeId = employeeId, WorkDay = WorkDay.Tuesday, Shift = WorkShift.Afternoon, StartDate = month.AddDays(2), Status = ScheduleStatus.Completed },
                new WorkSchedule { Id = 3, EmployeeId = employeeId, WorkDay = WorkDay.Wednesday, Shift = WorkShift.Evening, StartDate = month.AddDays(3), Status = ScheduleStatus.Cancelled }
            };

            _mockWorkScheduleRepository.Setup(r => r.GetByEmployeeAsync(employeeId))
                .ReturnsAsync(schedules);

            // Act
            var result = await _service.GetEmployeeWorkloadSummaryAsync(employeeId, month);

            // Assert
            Assert.NotNull(result);
            
            // Use reflection to access anonymous object properties
            var resultType = result.GetType();
            var employeeIdProperty = resultType.GetProperty("EmployeeId");
            var totalSchedulesProperty = resultType.GetProperty("TotalSchedules");
            var scheduledCountProperty = resultType.GetProperty("ScheduledCount");
            var completedCountProperty = resultType.GetProperty("CompletedCount");
            var cancelledCountProperty = resultType.GetProperty("CancelledCount");
            
            Assert.NotNull(employeeIdProperty);
            Assert.NotNull(totalSchedulesProperty);
            Assert.NotNull(scheduledCountProperty);
            Assert.NotNull(completedCountProperty);
            Assert.NotNull(cancelledCountProperty);
            
            Assert.Equal(employeeId, employeeIdProperty.GetValue(result));
            Assert.Equal(3, totalSchedulesProperty.GetValue(result));
            Assert.Equal(1, scheduledCountProperty.GetValue(result));
            Assert.Equal(1, completedCountProperty.GetValue(result));
            Assert.Equal(1, cancelledCountProperty.GetValue(result));
        }

        [Fact]
        public async Task GetAvailableEmployeesAsync_ValidInput_ShouldReturnAvailableEmployees()
        {
            // Arrange
            var day = WorkDay.Monday;
            var shift = WorkShift.Morning;
            var date = DateTime.Today.AddDays(1);

            var employees = new List<Employee>
            {
                new Employee { Id = 1, FullName = "Employee 1" },
                new Employee { Id = 2, FullName = "Employee 2" },
                new Employee { Id = 3, FullName = "Employee 3" }
            };

            _mockEmployeeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(employees);
            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(1, day, shift, date, null))
                .ReturnsAsync(false);
            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(2, day, shift, date, null))
                .ReturnsAsync(true); // Has conflict
            _mockWorkScheduleRepository.Setup(r => r.HasConflictAsync(3, day, shift, date, null))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetAvailableEmployeesAsync(day, shift, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.Id == 1);
            Assert.Contains(result, e => e.Id == 3);
            Assert.DoesNotContain(result, e => e.Id == 2);
        }

        [Fact]
        public async Task CancelScheduleAsync_ValidInput_ShouldCancelSchedule()
        {
            // Arrange
            var scheduleId = 1;
            var reason = "Emergency cancellation";

            var existingSchedule = new WorkSchedule
            {
                Id = scheduleId,
                EmployeeId = 1,
                WorkDay = WorkDay.Monday,
                Shift = WorkShift.Morning,
                StartDate = DateTime.Today.AddDays(2), // Future date
                Status = ScheduleStatus.Scheduled,
                CreatedDate = DateTime.Now
            };

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync(existingSchedule);
            _mockWorkScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkSchedule>()))
                .ReturnsAsync(existingSchedule);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CancelScheduleAsync(scheduleId, reason);

            // Assert
            Assert.True(result);
            Assert.Equal(ScheduleStatus.Cancelled, existingSchedule.Status);
            Assert.Contains("Emergency cancellation", existingSchedule.Notes);
            Assert.NotNull(existingSchedule.UpdatedDate);

            _mockWorkScheduleRepository.Verify(r => r.UpdateAsync(It.IsAny<WorkSchedule>()), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelScheduleAsync_ScheduleNotFound_ShouldReturnFalse()
        {
            // Arrange
            var scheduleId = 999;

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync((WorkSchedule)null);

            // Act
            var result = await _service.CancelScheduleAsync(scheduleId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CancelScheduleAsync_CompletedSchedule_ShouldThrowValidationException()
        {
            // Arrange
            var scheduleId = 1;

            var existingSchedule = new WorkSchedule
            {
                Id = scheduleId,
                EmployeeId = 1,
                WorkDay = WorkDay.Monday,
                Shift = WorkShift.Morning,
                StartDate = DateTime.Today.AddDays(2),
                Status = ScheduleStatus.Completed, // Already completed
                CreatedDate = DateTime.Now
            };

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync(existingSchedule);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CancelScheduleAsync(scheduleId));
        }

        [Fact]
        public async Task CancelScheduleAsync_StartedSchedule_ShouldThrowValidationException()
        {
            // Arrange
            var scheduleId = 1;

            var existingSchedule = new WorkSchedule
            {
                Id = scheduleId,
                EmployeeId = 1,
                WorkDay = WorkDay.Monday,
                Shift = WorkShift.Morning,
                StartDate = DateTime.Today.AddDays(-1), // Past date (already started)
                Status = ScheduleStatus.Scheduled,
                CreatedDate = DateTime.Now
            };

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync(existingSchedule);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CancelScheduleAsync(scheduleId));
        }

        // IService<T> implementation tests
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSchedules()
        {
            // Arrange
            var schedules = new List<WorkSchedule>
            {
                new WorkSchedule { Id = 1, EmployeeId = 1 },
                new WorkSchedule { Id = 2, EmployeeId = 2 }
            };

            _mockWorkScheduleRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(schedules);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnSchedule()
        {
            // Arrange
            var scheduleId = 1;
            var schedule = new WorkSchedule { Id = scheduleId, EmployeeId = 1 };

            _mockWorkScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId))
                .ReturnsAsync(schedule);

            // Act
            var result = await _service.GetByIdAsync(scheduleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(scheduleId, result.Id);
        }

        [Fact]
        public async Task CreateAsync_ValidSchedule_ShouldCreateSchedule()
        {
            // Arrange
            var schedule = new WorkSchedule { EmployeeId = 1, WorkDay = WorkDay.Monday, Shift = WorkShift.Morning };

            _mockWorkScheduleRepository.Setup(r => r.AddAsync(schedule))
                .ReturnsAsync(schedule);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(schedule);

            // Assert
            Assert.NotNull(result);
            _mockWorkScheduleRepository.Verify(r => r.AddAsync(schedule), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ValidSchedule_ShouldUpdateSchedule()
        {
            // Arrange
            var schedule = new WorkSchedule { Id = 1, EmployeeId = 1, WorkDay = WorkDay.Monday, Shift = WorkShift.Morning };

            _mockWorkScheduleRepository.Setup(r => r.UpdateAsync(schedule))
                .ReturnsAsync(schedule);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(schedule);

            // Assert
            Assert.NotNull(result);
            _mockWorkScheduleRepository.Verify(r => r.UpdateAsync(schedule), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ShouldDeleteSchedule()
        {
            // Arrange
            var scheduleId = 1;

            _mockWorkScheduleRepository.Setup(r => r.DeleteAsync(scheduleId))
                .ReturnsAsync(true);
            _mockWorkScheduleRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(scheduleId);

            // Assert
            Assert.True(result);
            _mockWorkScheduleRepository.Verify(r => r.DeleteAsync(scheduleId), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldReturnFalse()
        {
            // Arrange
            var scheduleId = 999;

            _mockWorkScheduleRepository.Setup(r => r.DeleteAsync(scheduleId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(scheduleId);

            // Assert
            Assert.False(result);
            _mockWorkScheduleRepository.Verify(r => r.DeleteAsync(scheduleId), Times.Once);
            _mockWorkScheduleRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
} 
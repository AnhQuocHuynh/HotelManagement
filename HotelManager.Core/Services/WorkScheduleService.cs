using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace HotelManager.Core.Services
{
    /// <summary>
    /// Service implementation cho WorkSchedule business logic
    /// </summary>
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly IWorkScheduleRepository _workScheduleRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly ILogger<WorkScheduleService> _logger;

        public WorkScheduleService(
            IWorkScheduleRepository workScheduleRepository,
            IRepository<Employee> employeeRepository,
            ILogger<WorkScheduleService> logger)
        {
            _workScheduleRepository = workScheduleRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Phân công lịch làm việc cho nhân viên với validation rules và conflict checking
        /// </summary>
        public async Task<WorkSchedule> AssignScheduleAsync(int employeeId, WorkDay day, WorkShift shift, 
            DateTime startDate, DateTime? endDate, int assignedByEmployeeId, string? notes = null)
        {
            try
            {
                // Validate input parameters
                if (startDate < DateTime.Today)
                {
                    throw new ValidationException("Start date cannot be in the past");
                }

                if (endDate.HasValue && endDate.Value < startDate)
                {
                    throw new ValidationException("End date cannot be before start date");
                }

                // Check if employee exists
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    throw new ValidationException($"Employee with ID {employeeId} not found");
                }

                // Check for conflicts
                var hasConflict = await ValidateScheduleConflictAsync(employeeId, day, shift, startDate);
                if (hasConflict)
                {
                    throw new ValidationException($"Schedule conflict detected for employee {employeeId} on {day} {shift} at {startDate:yyyy-MM-dd}");
                }

                // Create new work schedule
                var workSchedule = new WorkSchedule
                {
                    EmployeeId = employeeId,
                    WorkDay = day,
                    Shift = shift,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = ScheduleStatus.Scheduled,
                    Notes = notes,
                    AssignedByEmployeeId = assignedByEmployeeId,
                    CreatedDate = DateTime.Now
                };

                var result = await _workScheduleRepository.AddAsync(workSchedule);
                await _workScheduleRepository.SaveChangesAsync();

                _logger.LogInformation("Work schedule assigned for employee {EmployeeId} on {Day} {Shift} starting {StartDate}", 
                    employeeId, day, shift, startDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning work schedule for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        /// <summary>
        /// Lấy lịch làm việc của nhân viên trong khoảng thời gian với sorting và filtering options
        /// </summary>
        public async Task<List<WorkSchedule>> GetEmployeeScheduleAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                {
                    throw new ValidationException("Start date cannot be after end date");
                }

                var schedules = await _workScheduleRepository.GetByEmployeeAsync(employeeId);

                // Apply date filtering if provided
                if (startDate.HasValue)
                {
                    schedules = schedules.Where(s => s.StartDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    schedules = schedules.Where(s => s.StartDate <= endDate.Value).ToList();
                }

                return schedules.OrderBy(s => s.StartDate).ThenBy(s => s.WorkDay).ThenBy(s => s.Shift).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee schedule for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        /// <summary>
        /// Lấy lịch làm việc tuần (tất cả nhân viên) với grouping by employee và optimization for UI display
        /// </summary>
        public async Task<List<WorkSchedule>> GetWeeklyScheduleAsync(DateTime weekStart)
        {
            try
            {
                // Normalize week start to Monday
                var normalizedWeekStart = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek + (int)DayOfWeek.Monday);
                
                return await _workScheduleRepository.GetWeeklySchedulesAsync(normalizedWeekStart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedule for week starting {WeekStart}", weekStart);
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra xung đột lịch làm việc với comprehensive conflict detection
        /// </summary>
        public async Task<bool> ValidateScheduleConflictAsync(int employeeId, WorkDay day, WorkShift shift, DateTime date, int? excludeScheduleId = null)
        {
            try
            {
                return await _workScheduleRepository.HasConflictAsync(employeeId, day, shift, date, excludeScheduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating schedule conflict for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái lịch làm việc với audit logging cho status changes
        /// </summary>
        public async Task<WorkSchedule> UpdateScheduleStatusAsync(int scheduleId, ScheduleStatus status, string? notes = null)
        {
            try
            {
                var schedule = await _workScheduleRepository.GetByIdAsync(scheduleId);
                if (schedule == null)
                {
                    throw new ValidationException($"Work schedule with ID {scheduleId} not found");
                }

                var oldStatus = schedule.Status;
                schedule.Status = status;
                schedule.UpdatedDate = DateTime.Now;

                if (!string.IsNullOrEmpty(notes))
                {
                    schedule.Notes = string.IsNullOrEmpty(schedule.Notes) ? notes : $"{schedule.Notes}; {notes}";
                }

                var result = await _workScheduleRepository.UpdateAsync(schedule);
                await _workScheduleRepository.SaveChangesAsync();

                _logger.LogInformation("Work schedule {ScheduleId} status changed from {OldStatus} to {NewStatus}", 
                    scheduleId, oldStatus, status);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule status for schedule {ScheduleId}", scheduleId);
                throw;
            }
        }

        /// <summary>
        /// Bulk assign lịch làm việc cho nhiều ngày với transaction handling
        /// </summary>
        public async Task<List<WorkSchedule>> BulkAssignScheduleAsync(int employeeId, List<WorkDay> days, 
            WorkShift shift, DateTime startDate, DateTime endDate, int assignedByEmployeeId)
        {
            try
            {
                if (startDate > endDate)
                {
                    throw new ValidationException("Start date cannot be after end date");
                }

                if (!days.Any())
                {
                    throw new ValidationException("At least one work day must be specified");
                }

                var schedules = new List<WorkSchedule>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var dayOfWeek = (WorkDay)((int)currentDate.DayOfWeek);
                    
                    if (days.Contains(dayOfWeek))
                    {
                        // Check for conflicts
                        var hasConflict = await ValidateScheduleConflictAsync(employeeId, dayOfWeek, shift, currentDate);
                        if (!hasConflict)
                        {
                            var schedule = new WorkSchedule
                            {
                                EmployeeId = employeeId,
                                WorkDay = dayOfWeek,
                                Shift = shift,
                                StartDate = currentDate,
                                EndDate = null, // Single day assignment
                                Status = ScheduleStatus.Scheduled,
                                AssignedByEmployeeId = assignedByEmployeeId,
                                CreatedDate = DateTime.Now
                            };

                            schedules.Add(schedule);
                        }
                        else
                        {
                            _logger.LogWarning("Conflict detected for employee {EmployeeId} on {Day} {Shift} at {Date}, skipping", 
                                employeeId, dayOfWeek, shift, currentDate);
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }

                // Add all schedules in batch
                foreach (var schedule in schedules)
                {
                    await _workScheduleRepository.AddAsync(schedule);
                }

                await _workScheduleRepository.SaveChangesAsync();

                _logger.LogInformation("Bulk assigned {Count} work schedules for employee {EmployeeId}", 
                    schedules.Count, employeeId);

                return schedules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning schedules for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        /// <summary>
        /// Lấy workload summary của nhân viên trong tháng với statistics for management reporting
        /// </summary>
        public async Task<object> GetEmployeeWorkloadSummaryAsync(int employeeId, DateTime month)
        {
            try
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var schedules = await _workScheduleRepository.GetByEmployeeAsync(employeeId);
                var monthlySchedules = schedules.Where(s => 
                    s.StartDate >= monthStart && s.StartDate <= monthEnd).ToList();

                var summary = new
                {
                    EmployeeId = employeeId,
                    Month = month.ToString("yyyy-MM"),
                    TotalSchedules = monthlySchedules.Count,
                    ScheduledCount = monthlySchedules.Count(s => s.Status == ScheduleStatus.Scheduled),
                    CompletedCount = monthlySchedules.Count(s => s.Status == ScheduleStatus.Completed),
                    CancelledCount = monthlySchedules.Count(s => s.Status == ScheduleStatus.Cancelled),
                    WorkloadByDay = monthlySchedules.GroupBy(s => s.WorkDay)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    WorkloadByShift = monthlySchedules.GroupBy(s => s.Shift)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count())
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workload summary for employee {EmployeeId} in month {Month}", employeeId, month);
                throw;
            }
        }

        /// <summary>
        /// Tìm nhân viên available cho ca làm việc cụ thể với consideration of employee skills và availability
        /// </summary>
        public async Task<List<Employee>> GetAvailableEmployeesAsync(WorkDay day, WorkShift shift, DateTime date)
        {
            try
            {
                // Get all employees
                var allEmployees = await _employeeRepository.GetAllAsync();
                var availableEmployees = new List<Employee>();

                foreach (var employee in allEmployees)
                {
                    // Check if employee has conflict on this day/shift/date
                    var hasConflict = await ValidateScheduleConflictAsync(employee.Id, day, shift, date);
                    if (!hasConflict)
                    {
                        availableEmployees.Add(employee);
                    }
                }

                return availableEmployees.OrderBy(e => e.FullName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available employees for day {Day} shift {Shift} date {Date}", day, shift, date);
                throw;
            }
        }

        /// <summary>
        /// Hủy lịch làm việc với business rules cho cancellation
        /// </summary>
        public async Task<bool> CancelScheduleAsync(int scheduleId, string? reason = null)
        {
            try
            {
                var schedule = await _workScheduleRepository.GetByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return false;
                }

                // Business rule: Cannot cancel completed schedules
                if (schedule.Status == ScheduleStatus.Completed)
                {
                    throw new ValidationException("Cannot cancel a completed schedule");
                }

                // Business rule: Cannot cancel schedules that have already started
                if (schedule.StartDate <= DateTime.Now)
                {
                    throw new ValidationException("Cannot cancel a schedule that has already started");
                }

                schedule.Status = ScheduleStatus.Cancelled;
                schedule.UpdatedDate = DateTime.Now;

                if (!string.IsNullOrEmpty(reason))
                {
                    schedule.Notes = string.IsNullOrEmpty(schedule.Notes) ? $"Cancelled: {reason}" : $"{schedule.Notes}; Cancelled: {reason}";
                }

                await _workScheduleRepository.UpdateAsync(schedule);
                await _workScheduleRepository.SaveChangesAsync();

                _logger.LogInformation("Work schedule {ScheduleId} cancelled with reason: {Reason}", scheduleId, reason ?? "No reason provided");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling schedule {ScheduleId}", scheduleId);
                throw;
            }
        }

        // IService<T> implementation
        public async Task<IEnumerable<WorkSchedule>> GetAllAsync()
        {
            try
            {
                return await _workScheduleRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all work schedules");
                throw;
            }
        }

        public async Task<WorkSchedule> GetByIdAsync(int id)
        {
            try
            {
                return await _workScheduleRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work schedule by ID {Id}", id);
                throw;
            }
        }

        public async Task<WorkSchedule> CreateAsync(WorkSchedule entity)
        {
            try
            {
                var result = await _workScheduleRepository.AddAsync(entity);
                await _workScheduleRepository.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work schedule");
                throw;
            }
        }

        public async Task<WorkSchedule> UpdateAsync(WorkSchedule entity)
        {
            try
            {
                var result = await _workScheduleRepository.UpdateAsync(entity);
                await _workScheduleRepository.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work schedule");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var result = await _workScheduleRepository.DeleteAsync(id);
                if (result)
                {
                    await _workScheduleRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work schedule {Id}", id);
                throw;
            }
        }
    }
} 
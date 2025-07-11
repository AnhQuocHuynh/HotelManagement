using HotelManager.Data;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManager.Core.Repositories
{
    /// <summary>
    /// Repository implementation cho WorkSchedule
    /// TODO (Tuấn): Implement tất cả methods với proper error handling và optimization
    /// </summary>
    public class WorkScheduleRepository : Repository<WorkSchedule>, IWorkScheduleRepository
    {
        public WorkScheduleRepository(HotelDbContext context, ILogger<WorkScheduleRepository> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// Lấy lịch theo employee với Include navigation properties
        /// </summary>
        public async Task<List<WorkSchedule>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                return await _context.WorkSchedules
                    .Include(w => w.Employee)
                    .Include(w => w.AssignedByEmployee)
                    .Where(w => w.EmployeeId == employeeId)
                    .OrderBy(w => w.StartDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work schedules for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        /// <summary>
        /// Lấy lịch theo date range với optimization
        /// </summary>
        public async Task<List<WorkSchedule>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.WorkSchedules
                    .Include(w => w.Employee)
                    .Include(w => w.AssignedByEmployee)
                    .Where(w => w.StartDate >= startDate && w.StartDate <= endDate)
                    .OrderBy(w => w.StartDate)
                    .ThenBy(w => w.EmployeeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work schedules for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Lấy lịch theo day và shift
        /// </summary>
        public async Task<List<WorkSchedule>> GetByDayAndShiftAsync(WorkDay day, WorkShift shift)
        {
            try
            {
                return await _context.WorkSchedules
                    .Include(w => w.Employee)
                    .Include(w => w.AssignedByEmployee)
                    .Where(w => w.WorkDay == day && w.Shift == shift)
                    .OrderBy(w => w.StartDate)
                    .ThenBy(w => w.EmployeeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work schedules for day {Day} and shift {Shift}", day, shift);
                throw;
            }
        }

        /// <summary>
        /// Conflict detection logic - QUAN TRỌNG!
        /// </summary>
        public async Task<bool> HasConflictAsync(int employeeId, WorkDay day, WorkShift shift, DateTime date, int? excludeId = null)
        {
            try
            {
                var query = _context.WorkSchedules
                    .Where(w => w.EmployeeId == employeeId 
                           && w.WorkDay == day 
                           && w.Shift == shift
                           && w.Status != ScheduleStatus.Cancelled);

                // Exclude specific schedule if provided (for updates)
                if (excludeId.HasValue)
                {
                    query = query.Where(w => w.Id != excludeId.Value);
                }

                // Check for exact date conflicts
                var exactDateConflict = await query
                    .AnyAsync(w => w.StartDate.Date == date.Date);

                if (exactDateConflict)
                {
                    return true;
                }

                // Check for overlapping date ranges
                var overlappingConflict = await query
                    .AnyAsync(w => 
                        (w.StartDate <= date && (w.EndDate == null || w.EndDate >= date)) ||
                        (date >= w.StartDate && (w.EndDate == null || date <= w.EndDate)));

                return overlappingConflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking conflict for employee {EmployeeId}, day {Day}, shift {Shift}, date {Date}", 
                    employeeId, day, shift, date);
                throw;
            }
        }

        /// <summary>
        /// Weekly schedule retrieval
        /// </summary>
        public async Task<List<WorkSchedule>> GetWeeklySchedulesAsync(DateTime weekStart)
        {
            try
            {
                var weekEnd = weekStart.AddDays(6);
                return await _context.WorkSchedules
                    .Include(w => w.Employee)
                    .Include(w => w.AssignedByEmployee)
                    .Where(w => w.StartDate >= weekStart && w.StartDate <= weekEnd)
                    .OrderBy(w => w.StartDate)
                    .ThenBy(w => w.EmployeeId)
                    .ThenBy(w => w.WorkDay)
                    .ThenBy(w => w.Shift)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedules for week starting {WeekStart}", weekStart);
                throw;
            }
        }

        /// <summary>
        /// Lấy lịch theo status
        /// </summary>
        public async Task<List<WorkSchedule>> GetByStatusAsync(ScheduleStatus status)
        {
            try
            {
                return await _context.WorkSchedules
                    .Include(w => w.Employee)
                    .Include(w => w.AssignedByEmployee)
                    .Where(w => w.Status == status)
                    .OrderBy(w => w.StartDate)
                    .ThenBy(w => w.EmployeeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work schedules for status {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Counting schedules cho workload balancing
        /// </summary>
        public async Task<int> CountEmployeeSchedulesInMonthAsync(int employeeId, DateTime month)
        {
            try
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                return await _context.WorkSchedules
                    .Where(w => w.EmployeeId == employeeId 
                           && w.StartDate >= monthStart 
                           && w.StartDate <= monthEnd
                           && w.Status != ScheduleStatus.Cancelled)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting schedules for employee {EmployeeId} in month {Month}", employeeId, month);
                throw;
            }
        }
    }
} 
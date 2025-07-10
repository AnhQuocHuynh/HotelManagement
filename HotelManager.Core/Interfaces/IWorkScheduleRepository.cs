using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Interfaces;
using HotelManager.Repositories;

namespace HotelManager.Core.Interfaces
{
    /// <summary>
    /// Repository interface cho WorkSchedule
    /// TODO (Tuấn): Implement concrete class trong HotelManager.Core/Repositories/
    /// </summary>
    public interface IWorkScheduleRepository : IRepository<WorkSchedule>
    {
        /// <summary>
        /// Lấy tất cả lịch làm việc của một nhân viên
        /// TODO (Tuấn): Implement với proper error handling
        /// </summary>
        Task<List<WorkSchedule>> GetByEmployeeAsync(int employeeId);
        
        /// <summary>
        /// Lấy lịch làm việc trong khoảng thời gian
        /// TODO (Tuấn): Optimize query performance
        /// </summary>
        Task<List<WorkSchedule>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Lấy lịch làm việc theo ngày và ca làm việc
        /// TODO (Tuấn): Consider pagination for large results
        /// </summary>
        Task<List<WorkSchedule>> GetByDayAndShiftAsync(WorkDay day, WorkShift shift);
        
        /// <summary>
        /// Kiểm tra xung đột lịch làm việc
        /// TODO (Tuấn): Implement efficient conflict detection algorithm
        /// </summary>
        Task<bool> HasConflictAsync(int employeeId, WorkDay day, WorkShift shift, DateTime date, int? excludeId = null);
        
        /// <summary>
        /// Lấy lịch làm việc tuần của tất cả nhân viên
        /// TODO (Tuấn): Include employee details for display
        /// </summary>
        Task<List<WorkSchedule>> GetWeeklySchedulesAsync(DateTime weekStart);
        
        /// <summary>
        /// Lấy lịch làm việc theo trạng thái
        /// TODO (Tuấn): Add sorting and filtering options
        /// </summary>
        Task<List<WorkSchedule>> GetByStatusAsync(ScheduleStatus status);
        
        /// <summary>
        /// Đếm số lượng lịch làm việc của nhân viên trong tháng
        /// TODO (Tuấn): Use for workload balancing
        /// </summary>
        Task<int> CountEmployeeSchedulesInMonthAsync(int employeeId, DateTime month);
    }
} 
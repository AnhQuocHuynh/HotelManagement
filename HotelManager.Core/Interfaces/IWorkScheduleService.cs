using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Interfaces;
using HotelManager.Models;

namespace HotelManager.Core.Interfaces
{
    /// <summary>
    /// Service interface cho WorkSchedule business logic
    /// TODO (Tuấn): Implement concrete class trong HotelManager.Core/Services/
    /// </summary>
    public interface IWorkScheduleService : IService<WorkSchedule>
    {
        /// <summary>
        /// Phân công lịch làm việc cho nhân viên
        /// TODO (Tuấn): Add validation rules và conflict checking
        /// </summary>
        Task<WorkSchedule> AssignScheduleAsync(int employeeId, WorkDay day, WorkShift shift, 
            DateTime startDate, DateTime? endDate, int assignedByEmployeeId, string? notes = null);
        
        /// <summary>
        /// Lấy lịch làm việc của nhân viên trong khoảng thời gian
        /// TODO (Tuấn): Add sorting và filtering options
        /// </summary>
        Task<List<WorkSchedule>> GetEmployeeScheduleAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Lấy lịch làm việc tuần (tất cả nhân viên)
        /// TODO (Tuấn): Group by employee và optimize for UI display
        /// </summary>
        Task<List<WorkSchedule>> GetWeeklyScheduleAsync(DateTime weekStart);
        
        /// <summary>
        /// Kiểm tra xung đột lịch làm việc
        /// TODO (Tuấn): Implement comprehensive conflict detection
        /// </summary>
        Task<bool> ValidateScheduleConflictAsync(int employeeId, WorkDay day, WorkShift shift, DateTime date, int? excludeScheduleId = null);
        
        /// <summary>
        /// Cập nhật trạng thái lịch làm việc
        /// TODO (Tuấn): Add audit logging cho status changes
        /// </summary>
        Task<WorkSchedule> UpdateScheduleStatusAsync(int scheduleId, ScheduleStatus status, string? notes = null);
        
        /// <summary>
        /// Bulk assign lịch làm việc cho nhiều ngày
        /// TODO (Tuấn): Implement transaction handling
        /// </summary>
        Task<List<WorkSchedule>> BulkAssignScheduleAsync(int employeeId, List<WorkDay> days, 
            WorkShift shift, DateTime startDate, DateTime endDate, int assignedByEmployeeId);
        
        /// <summary>
        /// Lấy workload summary của nhân viên trong tháng
        /// TODO (Tuấn): Calculate statistics for management reporting
        /// </summary>
        Task<object> GetEmployeeWorkloadSummaryAsync(int employeeId, DateTime month);
        
        /// <summary>
        /// Tìm nhân viên available cho ca làm việc cụ thể
        /// TODO (Tuấn): Consider employee skills và availability
        /// </summary>
        Task<List<Employee>> GetAvailableEmployeesAsync(WorkDay day, WorkShift shift, DateTime date);
        
        /// <summary>
        /// Hủy lịch làm việc
        /// TODO (Tuấn): Add business rules cho cancellation
        /// </summary>
        Task<bool> CancelScheduleAsync(int scheduleId, string? reason = null);
    }
} 
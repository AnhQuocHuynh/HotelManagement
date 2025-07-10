using HotelManager.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManager.Core.Models
{
    /// <summary>
    /// Mô hình quản lý lịch làm việc cho nhân viên
    /// TODO (Tuấn): Thêm validation attributes và business logic nếu cần
    /// </summary>
    public class WorkSchedule
    {
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public WorkDay WorkDay { get; set; }
        
        [Required]
        public WorkShift Shift { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;
        
        public string? Notes { get; set; }
        
        public int? AssignedByEmployeeId { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Employee? AssignedByEmployee { get; set; }

        // TODO (Tuấn): Thêm các computed properties nếu cần
        // Ví dụ: IsActive, IsUpcoming, etc.
        
        /// <summary>
        /// Kiểm tra xem lịch có đang hoạt động không
        /// TODO (Tuấn): Implement logic check active schedule
        /// </summary>
        public bool IsActive => Status == ScheduleStatus.Scheduled && StartDate <= DateTime.Now && (EndDate == null || EndDate >= DateTime.Now);
        
        /// <summary>
        /// Lấy mô tả ngắn gọn về lịch làm việc
        /// TODO (Tuấn): Customize format nếu cần
        /// </summary>
        public string DisplayText => $"{WorkDay.ToDisplay()} - {Shift.ToDisplay()}";
    }
} 
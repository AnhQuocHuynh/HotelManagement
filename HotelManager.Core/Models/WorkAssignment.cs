using System;
using System.ComponentModel.DataAnnotations;
using HotelManager.Models.Enums;

namespace HotelManager.Models
{
    public class WorkAssignment
    {
        public int Id { get; set; }
        
        [Required]
        public string RoomNumber { get; set; } = string.Empty;
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public AssignmentType Type { get; set; }
        
        [Required]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        
        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        
        public DateTime? CompletedDate { get; set; }
        
        public string? Notes { get; set; }
        
        public int? AssignedByEmployeeId { get; set; }
        
        // Navigation properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Employee? AssignedByEmployee { get; set; }
        public virtual Room Room { get; set; } = null!;
    }
} 
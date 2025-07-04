using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManager.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public EmployeePosition Position { get; set; }
        public UserAccount UserAccount { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "CCCD phải gồm 12 ký tự")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD chỉ chứa 12 chữ số")]
        public string CCCD { get; set; } = string.Empty;
    }
}

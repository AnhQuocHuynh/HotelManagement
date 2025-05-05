using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Data;

namespace HotelManager.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Staff;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }

}

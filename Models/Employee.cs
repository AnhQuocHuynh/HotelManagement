using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

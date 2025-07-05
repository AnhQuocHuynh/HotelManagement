using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManager.Models
{
    public class Cleaning
    {
        public int Id { get; set; }

        [Required]
        public DateTime CleaningDate { get; set; }

        [Required]
        public string RoomNumber { get; set; }
        public Room Room { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public string Notes { get; set; }
    }
}

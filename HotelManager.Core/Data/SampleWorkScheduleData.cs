using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Models;

namespace HotelManager.Core.Data
{
    /// <summary>
    /// Sample data cho testing WorkSchedule functionality
    /// TODO (Bảo): Sử dụng class này để test UI khi chưa có backend
    /// TODO (Tuấn): Có thể dùng để seed database trong development
    /// </summary>
    public static class SampleWorkScheduleData
    {
        /// <summary>
        /// TODO (Bảo): Sử dụng cho MockWorkScheduleService khi test UI
        /// </summary>
        public static List<WorkSchedule> GetSampleSchedules()
        {
            var sampleSchedules = new List<WorkSchedule>
            {
                new WorkSchedule
                {
                    Id = 1,
                    EmployeeId = 1,
                    WorkDay = WorkDay.Monday,
                    Shift = WorkShift.Morning,
                    StartDate = DateTime.Today.AddDays(-7), // Tuần trước
                    Status = ScheduleStatus.Completed,
                    Notes = "Hoàn thành tốt",
                    AssignedByEmployeeId = 10,
                    CreatedDate = DateTime.Today.AddDays(-10),
                    Employee = new Employee { Id = 1, FullName = "Nguyễn Văn A", Position = EmployeePosition.Receptionist }
                },
                
                new WorkSchedule
                {
                    Id = 2,
                    EmployeeId = 2,
                    WorkDay = WorkDay.Monday,
                    Shift = WorkShift.Afternoon,
                    StartDate = DateTime.Today,
                    Status = ScheduleStatus.Scheduled,
                    Notes = "Ca chiều receptionist",
                    AssignedByEmployeeId = 10,
                    CreatedDate = DateTime.Today.AddDays(-3),
                    Employee = new Employee { Id = 2, FullName = "Trần Thị B", Position = EmployeePosition.Receptionist }
                },
                
                new WorkSchedule
                {
                    Id = 3,
                    EmployeeId = 3,
                    WorkDay = WorkDay.Tuesday,
                    Shift = WorkShift.Morning,
                    StartDate = DateTime.Today.AddDays(1),
                    Status = ScheduleStatus.Scheduled,
                    Notes = "Dọn dẹp phòng VIP",
                    AssignedByEmployeeId = 10,
                    CreatedDate = DateTime.Today.AddDays(-2),
                    Employee = new Employee { Id = 3, FullName = "Lê Văn C", Position = EmployeePosition.Cleaner }
                },
                
                new WorkSchedule
                {
                    Id = 4,
                    EmployeeId = 4,
                    WorkDay = WorkDay.Wednesday,
                    Shift = WorkShift.Evening,
                    StartDate = DateTime.Today.AddDays(2),
                    Status = ScheduleStatus.Scheduled,
                    Notes = "Bảo trì hệ thống điện",
                    AssignedByEmployeeId = 10,
                    CreatedDate = DateTime.Today.AddDays(-1),
                    Employee = new Employee { Id = 4, FullName = "Phạm Minh D", Position = EmployeePosition.Technician }
                },
                
                new WorkSchedule
                {
                    Id = 5,
                    EmployeeId = 1,
                    WorkDay = WorkDay.Friday,
                    Shift = WorkShift.Night,
                    StartDate = DateTime.Today.AddDays(4),
                    Status = ScheduleStatus.Scheduled,
                    Notes = "Ca đêm cuối tuần",
                    AssignedByEmployeeId = 10,
                    CreatedDate = DateTime.Today,
                    Employee = new Employee { Id = 1, FullName = "Nguyễn Văn A", Position = EmployeePosition.Receptionist }
                }
            };

            return sampleSchedules;
        }

        /// <summary>
        /// TODO (Bảo): Sử dụng cho dropdown employees
        /// </summary>
        public static List<Employee> GetSampleEmployees()
        {
            return new List<Employee>
            {
                new Employee { Id = 1, FullName = "Nguyễn Văn A", Position = EmployeePosition.Receptionist, Email = "nva@hotel.com" },
                new Employee { Id = 2, FullName = "Trần Thị B", Position = EmployeePosition.Receptionist, Email = "ttb@hotel.com" },
                new Employee { Id = 3, FullName = "Lê Văn C", Position = EmployeePosition.Cleaner, Email = "lvc@hotel.com" },
                new Employee { Id = 4, FullName = "Phạm Minh D", Position = EmployeePosition.Technician, Email = "pmd@hotel.com" },
                new Employee { Id = 5, FullName = "Hoàng Thị E", Position = EmployeePosition.Manager, Email = "hte@hotel.com" }
            };
        }

        /// <summary>
        /// TODO (Bảo): Helper cho generate schedules tuần hiện tại
        /// </summary>
        public static List<WorkSchedule> GenerateCurrentWeekSchedules()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // Monday
            var employees = GetSampleEmployees();
            var schedules = new List<WorkSchedule>();
            var random = new Random();

            for (int day = 0; day < 7; day++)
            {
                var currentDay = (WorkDay)(day + 1); // WorkDay enum starts from 1
                var shifts = Enum.GetValues<WorkShift>();

                foreach (var shift in shifts.Take(2)) // Chỉ lấy 2 ca đầu để không quá đông
                {
                    var employee = employees[random.Next(employees.Count)];
                    
                    schedules.Add(new WorkSchedule
                    {
                        Id = schedules.Count + 100,
                        EmployeeId = employee.Id,
                        WorkDay = currentDay,
                        Shift = shift,
                        StartDate = startOfWeek.AddDays(day),
                        Status = ScheduleStatus.Scheduled,
                        Notes = $"Lịch tuần này - {currentDay} {shift}",
                        AssignedByEmployeeId = 10,
                        CreatedDate = DateTime.Today.AddDays(-1),
                        Employee = employee
                    });
                }
            }

            return schedules;
        }

        /// <summary>
        /// TODO (Bảo): Helper method để create mock service cho testing
        /// </summary>
        public static class MockWorkScheduleService
        {
            private static List<WorkSchedule> _schedules = GetSampleSchedules();

            public static async Task<List<WorkSchedule>> GetEmployeeScheduleAsync(int employeeId)
            {
                await Task.Delay(500); // Simulate network delay
                return _schedules.Where(s => s.EmployeeId == employeeId).ToList();
            }

            public static async Task<List<WorkSchedule>> GetWeeklyScheduleAsync(DateTime weekStart)
            {
                await Task.Delay(300);
                return GenerateCurrentWeekSchedules();
            }

            public static async Task<WorkSchedule> AssignScheduleAsync(int employeeId, WorkDay day, WorkShift shift, DateTime startDate)
            {
                await Task.Delay(200);
                
                var employee = GetSampleEmployees().FirstOrDefault(e => e.Id == employeeId);
                var newSchedule = new WorkSchedule
                {
                    Id = _schedules.Count + 1,
                    EmployeeId = employeeId,
                    WorkDay = day,
                    Shift = shift,
                    StartDate = startDate,
                    Status = ScheduleStatus.Scheduled,
                    Notes = "Mới phân công",
                    AssignedByEmployeeId = 10,
                    CreatedDate = DateTime.Now,
                    Employee = employee!
                };

                _schedules.Add(newSchedule);
                return newSchedule;
            }
        }
    }
} 
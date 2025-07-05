using HotelManager.Core.Models;
using HotelManager.Helpers;
using HotelManager.Models;
using HotelManager.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Data
{
    public static class HotelDbInitializer
    {
        public static void Seed(HotelDbContext context)
        {
            // Đảm bảo schema luôn khớp với Model trước khi seed dữ liệu
            context.Database.Migrate();

            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange( 
                    new Room { RoomNumber = "101", RoomType = RoomType.Standard, PricePerNight = 500000m, RoomStatus = RoomStatus.Available },
                    new Room { RoomNumber = "102", RoomType = RoomType.Deluxe, PricePerNight = 750000m },
                    new Room { RoomNumber = "103", RoomType = RoomType.Suite, PricePerNight = 1000000m }
                );
            }

            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { FullName = "Nguyen Van A", PhoneNumber = "0901234567" , CCCD = "89879123"},
                    new Customer { FullName = "Tran Thi B", PhoneNumber = "0912345678", CCCD = "89875123" },
                    new Customer { FullName = "Le Van C", PhoneNumber = "0923456789" , CCCD = "89879523" }
                );
            }

            // Create accounts individually (only if they don't already exist)
            
            // Admin User
            if (!context.UserAccounts.Any(u => u.Username == "admin1"))
            {
                System.Diagnostics.Debug.WriteLine("Creating admin1 account...");
                var adminEmp = new Employee
                {
                    FullName = "Admin User",
                    Position = EmployeePosition.Manager, 
                    Email = "admin@hotelmanager.com",
                    PhoneNumber = "0900000000",
                    HireDate = DateTime.Now.AddYears(-2),
                    CCCD = "000000000001"
                };
                context.Employees.Add(adminEmp);

                context.UserAccounts.Add(new UserAccount
                {
                    Username = "admin1",
                    PasswordHash = HashHelper.HashPassword("admin1"),
                    Role = UserRole.Admin,
                    Employee = adminEmp,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }

            else
            {
                System.Diagnostics.Debug.WriteLine("admin1 account already exists, skipping...");
            }

            // Manager User
            if (!context.UserAccounts.Any(u => u.Username == "manager1"))
            {
                System.Diagnostics.Debug.WriteLine("Creating manager1 account...");
                var managerEmp = new Employee
                {
                    FullName = "Manager User",
                    Position = EmployeePosition.Manager,
                    Email = "manager@hotelmanager.com", 
                    PhoneNumber = "0900000001",
                    HireDate = DateTime.Now.AddYears(-1),
                    CCCD = "000000000002"
                };
                context.Employees.Add(managerEmp);

                context.UserAccounts.Add(new UserAccount
                {
                    Username = "manager1",
                    PasswordHash = HashHelper.HashPassword("manager1"),
                    Role = UserRole.Manager,
                    Employee = managerEmp,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("manager1 account already exists, skipping...");
            }

            // Cleaner Staff
            if (!context.UserAccounts.Any(u => u.Username == "cleaner1"))
            {
                for (int i = 1; i <= 5; i++)
                {
                    string username = $"cleaner{i}";

                    if (!context.UserAccounts.Any(u => u.Username == username))
                    {
                        System.Diagnostics.Debug.WriteLine($"Creating {username} account...");

                        var cleanerEmp = new Employee
                        {
                            FullName = $"Cleaner User {i}",
                            Position = EmployeePosition.Cleaner,
                            Email = $"{username}@hotelmanager.com",
                            PhoneNumber = $"090000000{i + 2}", // 0900000003 -> 0900000007
                            HireDate = DateTime.Now.AddMonths(-i),
                            CCCD = $"00000000000{i + 2}"
                        };
                        context.Employees.Add(cleanerEmp);

                        context.UserAccounts.Add(new UserAccount
                        {
                            Username = username,
                            PasswordHash = HashHelper.HashPassword(username),
                            Role = UserRole.Staff,
                            Employee = cleanerEmp,
                            CreatedAt = DateTime.Now,
                            IsActive = true
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{username} account already exists, skipping...");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("cleaners' accounts already exist, skipping...");
            }


            // Technician Staff
            if (!context.UserAccounts.Any(u => u.Username == "technician1"))
            {
                for (int i = 1; i <= 5; i++)
                {
                    string username = $"technician{i}";

                    if (!context.UserAccounts.Any(u => u.Username == username))
                    {
                        System.Diagnostics.Debug.WriteLine($"Creating {username} account...");

                        var technicianEmp = new Employee
                        {
                            FullName = $"Technician User {i}",
                            Position = EmployeePosition.Technician,
                            Email = $"{username}@hotelmanager.com",
                            PhoneNumber = $"090000001{i + 7}", // 0900000018 -> 0900000022
                            HireDate = DateTime.Now.AddMonths(-i - 5),
                            CCCD = $"00000000002{i}"
                        };
                        context.Employees.Add(technicianEmp);

                        context.UserAccounts.Add(new UserAccount
                        {
                            Username = username,
                            PasswordHash = HashHelper.HashPassword(username),
                            Role = UserRole.Staff,
                            Employee = technicianEmp,
                            CreatedAt = DateTime.Now,
                            IsActive = true
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{username} account already exists, skipping...");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("technicians' accounts already exist, skipping...");
            }

            // Receptionist Staff
            if (!context.UserAccounts.Any(u => u.Username == "receptionist1"))
            {
                for (int i = 1; i <= 7; i++)
                {
                    string username = $"receptionist{i}";

                    if (!context.UserAccounts.Any(u => u.Username == username))
                    {
                        System.Diagnostics.Debug.WriteLine($"Creating {username} account...");

                        var receptionistEmp = new Employee
                        {
                            FullName = $"Receptionist User {i}",
                            Position = EmployeePosition.Receptionist,
                            Email = $"{username}@hotelmanager.com",
                            PhoneNumber = $"090000001{i}", // 0900000011 -> 0900000017
                            HireDate = DateTime.Now.AddMonths(-i),
                            CCCD = $"00000000001{i}"
                        };
                        context.Employees.Add(receptionistEmp);

                        context.UserAccounts.Add(new UserAccount
                        {
                            Username = username,
                            PasswordHash = HashHelper.HashPassword(username),
                            Role = UserRole.Staff,
                            Employee = receptionistEmp,
                            CreatedAt = DateTime.Now,
                            IsActive = true
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{username} account already exists, skipping...");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("receptionists' accounts already exist, skipping...");
            }

            context.SaveChanges();
            
            // Debug: Show all accounts in database
            var allAccounts = context.UserAccounts.Include(u => u.Employee).ToList();
            System.Diagnostics.Debug.WriteLine($"Total UserAccounts in database: {allAccounts.Count}");
            foreach (var account in allAccounts)
            {
                System.Diagnostics.Debug.WriteLine($"Username: {account.Username}, Role: {account.Role}, Position: {account.Employee?.Position}");
            }

            if (!context.Bookings.Any())
            {
                var customers = context.Customers.ToList();
                var rooms = context.Rooms.ToList();
                var receptionists = context.Employees.Where(e => e.Position == EmployeePosition.Receptionist).ToList();

                var random = new Random();

                for (int i = 0; i < 30; i++)
                {
                    var customer = customers[i % customers.Count];
                    var room = rooms[i % rooms.Count];

                    var bookingEmployee = receptionists[random.Next(receptionists.Count)];
                    var checkInEmployee = receptionists[random.Next(receptionists.Count)];
                    var checkOutEmployee = receptionists[random.Next(receptionists.Count)];

                    var checkIn = DateTime.Today.AddDays(-i * 2); // Lùi dần theo thời gian
                    var stayLength = random.Next(1, 4); // 1 - 3 ngày
                    var checkOut = checkIn.AddDays(stayLength);
                    var bookingDate = checkIn.AddDays(-random.Next(1, 3));

                    var booking = new Booking
                    {
                        Customer = customer,
                        Room = room,
                        BookingDate = bookingDate,
                        CheckInDate = checkIn,
                        CheckOutDate = checkOut,
                        BookingEmployeeId = bookingEmployee.Id,
                        CheckInEmployeeID = checkInEmployee.Id,
                        CheckOutEmployeeID = checkOutEmployee.Id,
                        Status = BookingStatus.CheckedOut
                    };
                    context.Bookings.Add(booking);
                    context.SaveChanges();

                    var invoice = new Invoice
                    {
                        BookingId = booking.Id,
                        IssueDate = checkOut,
                        TotalAmount = room.PricePerNight * stayLength
                    };
                    context.Invoices.Add(invoice);
                    context.SaveChanges();

                    context.InvoiceDetails.Add(new InvoiceDetail
                    {
                        InvoiceId = invoice.Id,
                        RoomNumber = room.RoomNumber,
                        Quantity = stayLength,
                        UnitPrice = room.PricePerNight
                    });

                    context.Payments.Add(new Payment
                    {
                        InvoiceId = invoice.Id,
                        PaymentDate = checkOut,
                        Amount = invoice.TotalAmount,
                        PaymentMethod = PaymentMethod.Cash
                    });
                }
            }

            context.SaveChanges();

            //cleanings
            if (!context.Cleanings.Any())
            {
                var cleaners = context.Employees.Where(e => e.Position == EmployeePosition.Cleaner).ToList();
                var rooms = context.Rooms.ToList();
                var random = new Random();

                // Phân chia cleaning records đều giữa cleaners và các loại phòng
                int cleaningCount = 50;
                int cleanerCount = cleaners.Count;
                int roomCount = rooms.Count;

                for (int i = 0; i < cleaningCount; i++)
                {
                    // Xoay vòng cleaners (đảm bảo mỗi cleaner đều có việc)
                    var cleaner = cleaners[i % cleanerCount];

                    // Xoay vòng rooms, nhảy từng step để trải đều các loại phòng
                    var room = rooms[(i * 3 + random.Next(0, 3)) % roomCount];

                    // Tạo cleaning date phân bổ từ hôm nay lùi lại 30 ngày, mỗi cleaning cách nhau khoảng n/50 ngày
                    var cleaningDate = DateTime.Today.AddDays(-(i * (30 / cleaningCount)) - random.Next(0, 3));

                    var cleaning = new Cleaning
                    {
                        CleaningDate = cleaningDate,
                        EmployeeId = cleaner.Id,
                        RoomNumber = room.RoomNumber,
                        Notes = $"Routine cleaning by {cleaner.FullName}"
                    };

                    context.Cleanings.Add(cleaning);
                }

                context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Seeded {context.Cleanings.Count()} cleaning records.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Cleaning records already exist, skipping...");
            }

            
            // maintenance reports
            if (!context.MaintenanceReports.Any())
            {
                var rooms = context.Rooms.ToList();
                var random = new Random();
                int reportCount = 30;

                var maintenanceReports = new List<MaintenanceReport>();

                for (int i = 0; i < reportCount; i++)
                {
                    var room = rooms[(i * 2 + random.Next(0, 2)) % rooms.Count];
                    var reportedDate = DateTime.Today.AddDays(-random.Next(0, 60));

                    var report = new MaintenanceReport
                    {
                        RoomNumber = room.RoomNumber,
                        Description = "Reported issue: " + (random.Next(0, 2) == 0 ? "Leaking pipe" : "Broken AC"),
                        ReportedDate = reportedDate
                    };
                    maintenanceReports.Add(report);
                    context.MaintenanceReports.Add(report);
                }

                context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Seeded {context.MaintenanceReports.Count()} maintenance reports.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MaintenanceReports already exist, skipping...");
            }


            // maintenances
            if (!context.Maintenances.Any())
            {
                var technicians = context.Employees.Where(e => e.Position == EmployeePosition.Technician).ToList();
                var reports = context.MaintenanceReports.Include(mr => mr.Room).ToList();
                var random = new Random();
                int maintenanceCount = 50;

                for (int i = 0; i < maintenanceCount; i++)
                {
                    var report = reports[i % reports.Count];
                    var technician = technicians[random.Next(technicians.Count)];

                    var repairDate = report.ReportedDate.AddDays(random.Next(1, 10));
                    var cost = random.Next(100, 1000);

                    var maintenance = new Maintenance
                    {
                        MaintenanceReportId = report.Id,
                        EmployeeId = technician.Id,
                        RepairDate = repairDate,
                        Cost = (decimal)cost,
                        Notes = $"Repaired by {technician.FullName}"
                    };

                    context.Maintenances.Add(maintenance);
                }

                context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Seeded {context.Maintenances.Count()} maintenance records.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Maintenances already exist, skipping...");
            }

        }


    }
}

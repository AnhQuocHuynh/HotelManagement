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
            context.Database.Migrate();

            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange( 
                    new Room { RoomNumber = "101", RoomType = RoomType.Standard, PricePerNight = 500000m },
                    new Room { RoomNumber = "102", RoomType = RoomType.Deluxe, PricePerNight = 750000m },
                    new Room { RoomNumber = "103", RoomType = RoomType.Suite, PricePerNight = 1000000m }
                );
            }

            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { FullName = "Nguyen Van A", PhoneNumber = "0901234567" },
                    new Customer { FullName = "Tran Thi B", PhoneNumber = "0912345678" },
                    new Customer { FullName = "Le Van C", PhoneNumber = "0923456789" }
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
                    HireDate = DateTime.Now.AddYears(-2)
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
                    HireDate = DateTime.Now.AddYears(-1)
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
                System.Diagnostics.Debug.WriteLine("Creating cleaner1 account...");
                var cleanerEmp = new Employee
                {
                    FullName = "Cleaner User",
                    Position = EmployeePosition.Cleaner,
                    Email = "cleaner@hotelmanager.com",
                    PhoneNumber = "0900000002", 
                    HireDate = DateTime.Now.AddMonths(-6)
                };
                context.Employees.Add(cleanerEmp);

                context.UserAccounts.Add(new UserAccount
                {
                    Username = "cleaner1",
                    PasswordHash = HashHelper.HashPassword("cleaner1"),
                    Role = UserRole.Staff,
                    Employee = cleanerEmp,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("cleaner1 account already exists, skipping...");
            }

            // Technician Staff  
            if (!context.UserAccounts.Any(u => u.Username == "technician1"))
            {
                System.Diagnostics.Debug.WriteLine("Creating technician1 account...");
                var technicianEmp = new Employee
                {
                    FullName = "Technician User",
                    Position = EmployeePosition.Technician,
                    Email = "technician@hotelmanager.com",
                    PhoneNumber = "0900000003",
                    HireDate = DateTime.Now.AddMonths(-8)
                };
                context.Employees.Add(technicianEmp);

                context.UserAccounts.Add(new UserAccount
                {
                    Username = "technician1", 
                    PasswordHash = HashHelper.HashPassword("technician1"),
                    Role = UserRole.Staff,
                    Employee = technicianEmp,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("technician1 account already exists, skipping...");
            }

            // Receptionist Staff
            if (!context.UserAccounts.Any(u => u.Username == "receptionist1"))
            {
                System.Diagnostics.Debug.WriteLine("Creating receptionist1 account...");
                var receptionistEmp = new Employee
                {
                    FullName = "Receptionist User",
                    Position = EmployeePosition.Receptionist,
                    Email = "receptionist@hotelmanager.com",
                    PhoneNumber = "0900000004",
                    HireDate = DateTime.Now.AddMonths(-4)
                };
                context.Employees.Add(receptionistEmp);

                context.UserAccounts.Add(new UserAccount
                {
                    Username = "receptionist1",
                    PasswordHash = HashHelper.HashPassword("receptionist1"), 
                    Role = UserRole.Staff,
                    Employee = receptionistEmp,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("receptionist1 account already exists, skipping...");
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
                var customer = context.Customers.First();
                var room = context.Rooms.First();

                var booking = new Booking
                {
                    Customer = customer,
                    Room = room,
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(2)
                };
                context.Bookings.Add(booking);
                context.SaveChanges();

                var invoice = new Invoice
                {
                    Booking = booking,
                    IssueDate = DateTime.Today,
                    TotalAmount = room.PricePerNight * 2
                };
                context.Invoices.Add(invoice);
                context.SaveChanges();

                context.InvoiceDetails.Add(new InvoiceDetail
                {
                    Invoice = invoice,
                    Room = room,
                    Quantity = 2,
                    UnitPrice = room.PricePerNight
                });

                context.Payments.Add(new Payment
                {
                    Invoice = invoice,
                    PaymentDate = DateTime.Today,
                    Amount = invoice.TotalAmount,
                    PaymentMethod = PaymentMethod.Cash 
                });
            }

            context.SaveChanges();

            // Thêm dữ liệu mẫu cho Customer nếu bảng trống
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer
                    {
                        FullName = "Nguyễn Văn A",
                        PhoneNumber = "0901234567",
                        CCCD = "012345678901",
                        Type = CustomerType.Single
                    },
                    new Customer
                    {
                        FullName = "Trần Thị B",
                        PhoneNumber = "0912345678",
                        CCCD = "123456789012",
                        Type = CustomerType.Family
                    },
                    new Customer
                    {
                        FullName = "Lê Văn C",
                        PhoneNumber = "0923456789",
                        CCCD = "234567890123",
                        Type = CustomerType.Business
                    },
                    new Customer
                    {
                        FullName = "Phạm Thị D",
                        PhoneNumber = "0934567890",
                        CCCD = "345678901234",
                        Type = CustomerType.VIP
                    },
                    new Customer
                    {
                        FullName = "Hoàng Văn E",
                        PhoneNumber = "0945678901",
                        CCCD = "456789012345",
                        Type = CustomerType.Single
                    }
                );

                context.SaveChanges();
            }
            if (!context.Rooms.Any())
            {
                var rooms = new List<Room>
                {
                    new Room { RoomNumber = "101", RoomType = RoomType.Standard, PricePerNight = 500000, IsAvailable = true },
                    new Room { RoomNumber = "102", RoomType = RoomType.Deluxe, PricePerNight = 750000, IsAvailable = true },
                    new Room { RoomNumber = "201", RoomType = RoomType.Suite, PricePerNight = 1000000, IsAvailable = false }
                };
                context.Rooms.AddRange(rooms);
                context.SaveChanges();
            }
            if (!context.Bookings.Any())
            {
                var customers = context.Customers.ToList();
                var rooms = context.Rooms.ToList();

                var bookings = new List<Booking>
                {
                    new Booking
                    {
                        CheckInDate = new DateTime(2025, 6, 10),
                        CheckOutDate = new DateTime(2025, 6, 12),
                        Status = BookingStatus.Confirmed,
                        CustomerId = customers.First(c => c.FullName == "Nguyễn Văn An").Id,
                        RoomNumber = "101",
                        Customer = customers.First(c => c.FullName == "Nguyễn Văn An"),
                        RoomType = RoomType.Standard
                    },
                    new Booking
                    {
                        CheckInDate = new DateTime(2025, 6, 8),
                        CheckOutDate = new DateTime(2025, 6, 10),
                        Status = BookingStatus.CheckedOut,
                        CustomerId = customers.First(c => c.FullName == "Trần Thị Bình").Id,
                        RoomNumber = "102",
                        Customer = customers.First(c => c.FullName == "Trần Thị Bình"),
                        RoomType = RoomType.Deluxe
                    },
                    new Booking
                    {
                        CheckInDate = new DateTime(2025, 6, 11),
                        CheckOutDate = new DateTime(2025, 6, 15),
                        Status = BookingStatus.Pending,
                        CustomerId = customers.First(c => c.FullName == "Lê Hoàng Cường").Id,
                        RoomNumber = "201",
                        Customer = customers.First(c => c.FullName == "Lê Hoàng Cường"),
                        RoomType = RoomType.Suite
                    }
                };
                context.Bookings.AddRange(bookings);
                context.SaveChanges();
            }
        }
    }
}

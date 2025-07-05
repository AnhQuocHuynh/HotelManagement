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
                System.Diagnostics.Debug.WriteLine("Creating cleaner1 account...");
                var cleanerEmp = new Employee
                {
                    FullName = "Cleaner User",
                    Position = EmployeePosition.Cleaner,
                    Email = "cleaner@hotelmanager.com",
                    PhoneNumber = "0900000002", 
                    HireDate = DateTime.Now.AddMonths(-6),
                    CCCD = "000000000003"
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
                    HireDate = DateTime.Now.AddMonths(-8),
                    CCCD = "000000000004"
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



            if (!context.Cleanings.Any())
            {
                var cleaners = context.Employees.Where(e => e.Position == EmployeePosition.Cleaner).ToList();
                var rooms = context.Rooms.ToList();
                var random = new Random();

                for (int i = 0; i < 50; i++)
                {
                    var cleaner = cleaners[random.Next(cleaners.Count)];
                    var room = rooms[random.Next(rooms.Count)];
                    var cleaningDate = DateTime.Today.AddDays(-random.Next(0, 30)); // cleaned trong 30 ngày qua

                    var cleaning = new Cleaning
                    {
                        CleaningDate = cleaningDate,
                        EmployeeId = cleaner.Id,
                        RoomNumber = room.RoomNumber,
                        Notes = "Routine cleaning"
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




        }


    }
}

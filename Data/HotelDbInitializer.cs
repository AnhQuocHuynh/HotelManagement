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

            if (!context.Employees.Any())
            {
                // Admin User
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

                // Manager User
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

                // Cleaner Staff
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

                // Technician Staff  
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

                // Receptionist Staff
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

            context.SaveChanges();

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
        }
    }
}

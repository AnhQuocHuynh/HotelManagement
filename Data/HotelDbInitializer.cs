using HotelManager.Helpers;
using HotelManager.Models;
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
                var emp = new Employee
                {
                    FullName = "Admin User 1",
                    Position = EmployeePosition.Manager, 
                    Email = "admin@example.com",
                    PhoneNumber = "0900000000"
                };
                context.Employees.Add(emp);

                context.UserAccounts.Add(new UserAccount
                {
                    Username = "admin1",
                    PasswordHash = HashHelper.HashPassword("admin1"),
                    Role = UserRole.Admin,
                    Employee = emp
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

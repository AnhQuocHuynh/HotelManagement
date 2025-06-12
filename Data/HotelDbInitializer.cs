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

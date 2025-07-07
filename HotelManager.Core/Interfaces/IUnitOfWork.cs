using System;
using System.Threading.Tasks;
using HotelManager.Models;

namespace HotelManager.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Customer> Customers { get; }
        IRepository<Room> Rooms { get; }
        IRepository<Booking> Bookings { get; }
        IRepository<Employee> Employees { get; }
        IRepository<UserAccount> UserAccounts { get; }
        IRepository<Invoice> Invoices { get; }
        IRepository<InvoiceDetail> InvoiceDetails { get; }
        IRepository<Payment> Payments { get; }
        IRepository<MaintenanceReport> MaintenanceReports { get; }
        IRepository<WorkAssignment> WorkAssignments { get; }
        Task<int> SaveChangesAsync();
    }
} 
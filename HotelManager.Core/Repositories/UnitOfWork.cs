using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace HotelManager.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HotelDbContext _context;
        private readonly ILoggerFactory _loggerFactory;
        private IDbContextTransaction _transaction;
        private readonly Dictionary<Type, object> _repositories = new();
        private EmployeeRepository _employeeRepository;
        private WorkAssignmentRepository _workAssignmentRepository;
        private BookingRepository _bookingRepository;

        public UnitOfWork(HotelDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _loggerFactory = loggerFactory;
        }

        public IRepository<Customer> Customers => GetRepository<Customer>();
        public IRepository<Room> Rooms => GetRepository<Room>();
        public IRepository<Booking> Bookings => BookingRepository;
        public IRepository<Employee> Employees => GetRepository<Employee>();
        public IRepository<UserAccount> UserAccounts => GetRepository<UserAccount>();
        public IRepository<Invoice> Invoices => GetRepository<Invoice>();
        public IRepository<InvoiceDetail> InvoiceDetails => GetRepository<InvoiceDetail>();
        public IRepository<Payment> Payments => GetRepository<Payment>();
        public IRepository<MaintenanceReport> MaintenanceReports => GetRepository<MaintenanceReport>();
        public IRepository<WorkAssignment> WorkAssignments => GetRepository<WorkAssignment>();
        public EmployeeRepository EmployeeRepository => _employeeRepository ??= new EmployeeRepository(_context, _loggerFactory.CreateLogger<EmployeeRepository>());
        public WorkAssignmentRepository WorkAssignmentRepository => _workAssignmentRepository ??= new WorkAssignmentRepository(_context, _loggerFactory.CreateLogger<WorkAssignmentRepository>());
        public BookingRepository BookingRepository => _bookingRepository ??= new BookingRepository(_context, _loggerFactory.CreateLogger<BookingRepository>());

        private IRepository<T> GetRepository<T>() where T : class
        {
            if (!_repositories.TryGetValue(typeof(T), out var repo))
            {
                var logger = _loggerFactory.CreateLogger<Repository<T>>();
                repo = new Repository<T>(_context, logger);
                _repositories[typeof(T)] = repo;
            }
            return (IRepository<T>)repo;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
} 
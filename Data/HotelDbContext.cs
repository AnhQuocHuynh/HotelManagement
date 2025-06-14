using Microsoft.EntityFrameworkCore;
using HotelManager.Models;
using HotelManager.Config;
using Microsoft.Extensions.Configuration;

namespace HotelManager.Data
{
    public class HotelDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public HotelDbContext() { }

        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        public HotelDbContext(DbContextOptions<HotelDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Sử dụng connection string từ DatabaseConfig thay vì hardcoded default
                var connectionString = _configuration?.GetConnectionString("DefaultConnection")
                    ?? Config.DatabaseConfig.GetConnectionString();
              
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<MaintenanceReport> MaintenanceReports { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FullName).IsRequired();
                entity.Property(c => c.PhoneNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.RoomNumber);
                entity.Property(r => r.RoomType)
                      .HasConversion<int>()
                      .IsRequired();
                entity.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.CheckInDate).IsRequired();
                entity.Property(b => b.CheckOutDate).IsRequired();

                entity.HasOne(b => b.Customer)
                      .WithMany(c => c.Bookings)
                      .HasForeignKey(b => b.CustomerId);

                entity.HasOne(b => b.Room)
                      .WithMany(r => r.Bookings)
                      .HasForeignKey(b => b.RoomNumber);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(i => i.IssueDate).IsRequired();

                entity.HasOne(i => i.Booking)
                      .WithMany(b => b.Invoices)
                      .HasForeignKey(i => i.BookingId);
            });

            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                entity.HasKey(id => id.Id);
                entity.Property(id => id.Quantity).IsRequired();
                entity.Property(id => id.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(id => id.Invoice)
                      .WithMany(i => i.InvoiceDetails)
                      .HasForeignKey(id => id.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(id => id.Room)
                      .WithMany(r => r.InvoiceDetails)
                      .HasForeignKey(id => id.RoomNumber)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.PaymentDate).IsRequired();
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.PaymentMethod)
                      .HasConversion<int>()
                      .IsRequired();

                entity.HasOne(p => p.Invoice)
                      .WithMany(i => i.Payments)
                      .HasForeignKey(p => p.InvoiceId);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.Position).HasMaxLength(100);
            });

            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();

                entity.HasOne(u => u.Employee)
                      .WithOne(e => e.UserAccount)
                      .HasForeignKey<UserAccount>(u => u.EmployeeId);
            });
            modelBuilder.Entity<MaintenanceReport>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Description).IsRequired();
                entity.Property(m => m.ReportedDate).IsRequired();

                entity.HasOne(m => m.Room)
                      .WithMany(r => r.MaintenanceReports)
                      .HasForeignKey(m => m.RoomNumber)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}

using HotelManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using HotelManager.Models.Enums;

namespace HotelManager.Services.Manager
{
    public class InVoiceService
    {
        private readonly HotelDbContext _dbContext;
        public InVoiceService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// Lấy doanh thu theo khoảng thời gian và loại phòng
        public async Task<Dictionary<string, decimal>> GetInvoicePaymentOnTimeRangeAsync(DateTime startDate, DateTime endDate, string timeUnit, string roomType)
        {
            var query = _dbContext.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Room)
                .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate);

            // Lọc theo RoomType nếu không phải "All"
            if (!string.IsNullOrEmpty(roomType) && roomType != "All")
            {
                // Chuyển từ string sang enum RoomType
                if (Enum.TryParse<RoomType>(roomType, out var parsedRoomType))
                {
                    query = query.Where(i => i.Booking.Room.RoomType == parsedRoomType);
                }
            }

            var invoices = await query.ToListAsync();

            var result = new Dictionary<string, decimal>();

            if (timeUnit == "Weekdays")
            {
                // Khởi tạo các giá trị ban đầu cho từng thứ
                var days = Enum.GetNames(typeof(DayOfWeek));
                foreach (var d in days) result[d] = 0;

                // Cộng doanh thu theo thứ trong tuần
                foreach (var invoice in invoices)
                {
                    string day = invoice.IssueDate.DayOfWeek.ToString();
                    if (result.ContainsKey(day))
                        result[day] += invoice.TotalAmount;
                }
            }
            else if (timeUnit == "Every day")
            {
                // Khởi tạo tất cả các ngày từ startDate đến endDate với giá trị 0
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    result[date.ToString("yyyy-MM-dd")] = 0;
                }

                // Cộng doanh thu theo từng ngày
                foreach (var invoice in invoices)
                {
                    var dateKey = invoice.IssueDate.Date.ToString("yyyy-MM-dd");
                    if (result.ContainsKey(dateKey))
                    {
                        result[dateKey] += invoice.TotalAmount;
                    }
                }
            }

            else if (timeUnit == "12 months")
            {
                // Khởi tạo tháng 1–12
                for (int month = 1; month <= 12; month++)
                {
                    result[month.ToString("00")] = 0;
                }

                // Cộng doanh thu theo tháng
                foreach (var invoice in invoices)
                {
                    string month = invoice.IssueDate.Month.ToString("00");
                    if (result.ContainsKey(month))
                        result[month] += invoice.TotalAmount;
                }
            }
            else if (timeUnit == "Every month")
            {
                // Khởi tạo các tháng từ startDate đến endDate
                var current = new DateTime(startDate.Year, startDate.Month, 1);
                var end = new DateTime(endDate.Year, endDate.Month, 1);

                while (current <= end)
                {
                    var key = current.ToString("yyyy-MM"); // VD: "2025-06"
                    result[key] = 0;
                    current = current.AddMonths(1);
                }

                // Cộng doanh thu theo từng tháng
                foreach (var invoice in invoices)
                {
                    var key = new DateTime(invoice.IssueDate.Year, invoice.IssueDate.Month, 1).ToString("yyyy-MM");
                    if (result.ContainsKey(key))
                    {
                        result[key] += invoice.TotalAmount;
                    }
                }
            }


            return result;
        }

        /// lấy số lượng hóa đơn theo khoảng thời gian và loại phòng
        public async Task<Dictionary<string, int>> GetInvoiceCountOnTimeRangeAsync(DateTime startDate, DateTime endDate, string timeUnit, string roomType)
        {
            var query = _dbContext.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Room)
                .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate);

            if (!string.IsNullOrEmpty(roomType) && roomType != "All")
            {
                if (Enum.TryParse<RoomType>(roomType, out var parsedRoomType))
                {
                    query = query.Where(i => i.Booking.Room.RoomType == parsedRoomType);
                }
            }

            var invoices = await query.ToListAsync();
            var result = new Dictionary<string, int>();

            if (timeUnit == "Weekdays")
            {
                var days = Enum.GetNames(typeof(DayOfWeek));
                foreach (var d in days) result[d] = 0;
                foreach (var i in invoices)
                {
                    var key = i.IssueDate.DayOfWeek.ToString();
                    result[key]++;
                }
            }
            else if (timeUnit == "Every day")
            {
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    result[date.ToString("yyyy-MM-dd")] = 0;
                }

                foreach (var i in invoices)
                {
                    var key = i.IssueDate.Date.ToString("yyyy-MM-dd");
                    if (result.ContainsKey(key))
                        result[key]++;
                }
            }
            else if (timeUnit == "12 months")
            {
                for (int m = 1; m <= 12; m++) result[m.ToString("00")] = 0;
                foreach (var i in invoices)
                {
                    var key = i.IssueDate.Month.ToString("00");
                    result[key]++;
                }
            }
            else if (timeUnit == "Every month")
            {
                var current = new DateTime(startDate.Year, startDate.Month, 1);
                var end = new DateTime(endDate.Year, endDate.Month, 1);

                while (current <= end)
                {
                    var key = current.ToString("yyyy-MM");
                    result[key] = 0;
                    current = current.AddMonths(1);
                }

                foreach (var i in invoices)
                {
                    var key = new DateTime(i.IssueDate.Year, i.IssueDate.Month, 1).ToString("yyyy-MM");
                    if (result.ContainsKey(key))
                        result[key]++;
                }
            }

            return result;
        }

        /// Lấy thống kê doanh thu và số lượng hóa đơn theo khoảng thời gian, loại phòng và đơn vị thời gian
        public async Task<Dictionary<string, (decimal revenue, int invoiceCount)>> GetInvoiceStatsGroupedAsync(DateTime startDate, DateTime endDate, string timeUnit, string roomType)
        {
            var query = _dbContext.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Room)
                .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate);

            if (!string.IsNullOrEmpty(roomType) && roomType != "All")
            {
                if (Enum.TryParse<RoomType>(roomType, out var parsedRoomType))
                {
                    query = query.Where(i => i.Booking.Room.RoomType == parsedRoomType);
                }
            }

            var invoices = await query.ToListAsync();
            var result = new Dictionary<string, (decimal revenue, int invoiceCount)>();

            if (timeUnit == "Weekdays")
            {
                foreach (var day in Enum.GetNames(typeof(DayOfWeek)))
                {
                    result[day] = (0, 0);
                }

                foreach (var invoice in invoices)
                {
                    var key = invoice.IssueDate.DayOfWeek.ToString();
                    var (rev, count) = result[key];
                    result[key] = (rev + invoice.TotalAmount, count + 1);
                }
            }
            else if (timeUnit == "Every day")
            {
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    result[key] = (0, 0);
                }

                foreach (var invoice in invoices)
                {
                    var key = invoice.IssueDate.Date.ToString("yyyy-MM-dd");
                    if (result.ContainsKey(key))
                    {
                        var (rev, count) = result[key];
                        result[key] = (rev + invoice.TotalAmount, count + 1);
                    }
                }
            }
            else if (timeUnit == "12 months")
            {
                for (int month = 1; month <= 12; month++)
                {
                    var key = month.ToString("00");
                    result[key] = (0, 0);
                }

                foreach (var invoice in invoices)
                {
                    var key = invoice.IssueDate.Month.ToString("00");
                    var (rev, count) = result[key];
                    result[key] = (rev + invoice.TotalAmount, count + 1);
                }
            }
            else if (timeUnit == "Every month")
            {
                var current = new DateTime(startDate.Year, startDate.Month, 1);
                var end = new DateTime(endDate.Year, endDate.Month, 1);

                while (current <= end)
                {
                    var key = current.ToString("yyyy-MM");
                    result[key] = (0, 0);
                    current = current.AddMonths(1);
                }

                foreach (var invoice in invoices)
                {
                    var key = new DateTime(invoice.IssueDate.Year, invoice.IssueDate.Month, 1).ToString("yyyy-MM");
                    if (result.ContainsKey(key))
                    {
                        var (rev, count) = result[key];
                        result[key] = (rev + invoice.TotalAmount, count + 1);
                    }
                }
            }

            return result;
        }

    }
}

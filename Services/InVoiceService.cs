using HotelManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using HotelManager.Models.Enums;

namespace HotelManager.Services
{
    public class InVoiceService
    {
        private readonly HotelDbContext _dbContext;
        public InVoiceService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

            if (timeUnit == "Daily")
            {
                var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                // Khởi tạo các giá trị ban đầu cho từng thứ
                foreach (var day in daysOfWeek)
                {
                    result[day] = 0;
                }

                // Cộng doanh thu theo thứ trong tuần
                foreach (var invoice in invoices)
                {
                    string day = invoice.IssueDate.DayOfWeek.ToString();
                    if (result.ContainsKey(day))
                        result[day] += invoice.TotalAmount;
                }
            }
            else if (timeUnit == "Monthly")
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
            else if (timeUnit == "Seasonally")
            {
                // Khởi tạo 4 mùa
                var seasons = new[] { "Spring", "Summer", "Fall", "Winter" };
                foreach (var season in seasons)
                {
                    result[season] = 0;
                }

                // Cộng doanh thu theo mùa
                foreach (var invoice in invoices)
                {
                    string season = GetSeason(invoice.IssueDate.Month);
                    result[season] += invoice.TotalAmount;
                }
            }

            return result;
        }

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

            if (timeUnit == "Daily")
            {
                var days = Enum.GetNames(typeof(DayOfWeek));
                foreach (var d in days) result[d] = 0;
                foreach (var i in invoices)
                {
                    var key = i.IssueDate.DayOfWeek.ToString();
                    result[key]++;
                }
            }
            else if (timeUnit == "Monthly")
            {
                for (int m = 1; m <= 12; m++) result[m.ToString("00")] = 0;
                foreach (var i in invoices)
                {
                    var key = i.IssueDate.Month.ToString("00");
                    result[key]++;
                }
            }
            else if (timeUnit == "Seasonally")
            {
                var seasons = new[] { "Spring", "Summer", "Fall", "Winter" };
                foreach (var s in seasons) result[s] = 0;
                foreach (var i in invoices)
                {
                    var s = GetSeason(i.IssueDate.Month);
                    result[s]++;
                }
            }

            return result;
        }


        private string GetSeason(int month)
        {
            return month switch
            {
                1 or 2 or 3 => "Spring",
                4 or 5 or 6 => "Summer",
                7 or 8 or 9 => "Fall",
                10 or 11 or 12 => "Winter",
                _ => "Unknown"
            };
        }

    }
}

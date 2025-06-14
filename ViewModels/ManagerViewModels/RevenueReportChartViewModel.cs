using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using HotelManager.Services;
using HotelManager.Models.Enums;

namespace HotelManager.ViewModels.ManagerViewModels
{
    public class RevenueReportChartViewModel : BaseViewModel
    {
        InVoiceService inVoiceService;

        public ObservableCollection<string> TimeUnits { get; set; }

        private string _selectedTimeUnit;
        public string SelectedTimeUnit
        {
            get => _selectedTimeUnit;
            set
            {
                if (_selectedTimeUnit != value)
                {
                    _selectedTimeUnit = value;
                    OnPropertyChanged(nameof(SelectedTimeUnit));
                    _ = UpdateChartAsync();
                }
            }
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                    _ = UpdateChartAsync();
                }
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                    _ = UpdateChartAsync();
                }
            }
        }

        public ObservableCollection<object> RoomTypes { get; set; }

        private object _selectedRoomType;
        public object SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                if (_selectedRoomType != value)
                {
                    _selectedRoomType = value;
                    OnPropertyChanged(nameof(SelectedRoomType));
                    _ = UpdateChartAsync();
                }
            }
        }


        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        public RevenueReportChartViewModel()
        {
            inVoiceService = new InVoiceService(new Data.HotelDbContext());
            TimeUnitInit();
            RoomTypeInit();
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            UpdateChartAsync();
        }

        void TimeUnitInit()
        {
            TimeUnits = new ObservableCollection<string>
            {
                "Daily",
                "Monthly",
                "Seasonally",
            };
            SelectedTimeUnit = TimeUnits.FirstOrDefault();
        }

        void RoomTypeInit()
        {
            RoomTypes = new ObservableCollection<Object>
            {
                "All",
                RoomType.Deluxe,
                RoomType.Standard,
                RoomType.Suite,
            };
            SelectedRoomType = RoomTypes.FirstOrDefault();
        }

        private async Task UpdateChartAsync()
        {
            if (SelectedTimeUnit == null) return;

            // 1. Lấy dữ liệu doanh thu
            var revenueData = await inVoiceService.GetInvoicePaymentOnTimeRangeAsync(StartDate, EndDate, SelectedTimeUnit, SelectedRoomType?.ToString());

            // 2. Lấy dữ liệu số lượng booking
            var bookingCountData = await inVoiceService.GetInvoiceCountOnTimeRangeAsync(StartDate, EndDate, SelectedTimeUnit, SelectedRoomType?.ToString());

            // 3. Xử lý thứ tự nếu là theo mùa
            if (SelectedTimeUnit == "Seasonally")
            {
                var seasonOrder = new List<string> { "Spring", "Summer", "Fall", "Winter" };
                revenueData = seasonOrder.ToDictionary(s => s, s => revenueData.ContainsKey(s) ? revenueData[s] : 0);
                bookingCountData = seasonOrder.ToDictionary(s => s, s => bookingCountData.ContainsKey(s) ? bookingCountData[s] : 0);
            }
            var labels = revenueData.Keys.Union(bookingCountData.Keys).Distinct().ToList();
            labels.Sort(); // hoặc tự định nghĩa thứ tự nếu là Season

            var revenueValues = labels.Select((label, index) =>
                new ObservablePoint(index, revenueData.ContainsKey(label) ? (double)revenueData[label] : 0)).ToList();

            var bookingCountValues = labels.Select((label, index) =>
                new ObservablePoint(index, bookingCountData.ContainsKey(label) ? bookingCountData[label] : 0)).ToList();


            Series = new ISeries[]
            {
        // Cột doanh thu
        new ColumnSeries<ObservablePoint>
        {
            Values = revenueValues,
            MaxBarWidth = 40,
            Padding = 10,
            Stroke = null,
            DataLabelsSize = 12,
            DataLabelsPaint = new SolidColorPaint(SKColors.Black),
            DataLabelsFormatter = point =>
            {
                var y = (point.Model as ObservablePoint)?.Y ?? 0;
                return y.ToString("N0") + " ₫";
            },
            Name = "Doanh thu"
        },

        // Đường số lượng booking
        new LineSeries<ObservablePoint>
        {
            Values = bookingCountValues,
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(SKColors.DarkRed, 2),
            Stroke = new SolidColorPaint(SKColors.DarkRed, 2),
            Fill = null,
            Name = "Lượt đặt phòng",
            ScalesYAt = 1,
            LineSmoothness = 0

        }
            };

            XAxes = new Axis[]{
                new Axis{
                    Labels = labels,
                    Name = "Thời gian",
                    LabelsRotation = 0,
                    UnitWidth = 1
                }
            };


            YAxes = new Axis[]{
                // Trục Y bên trái: doanh thu
                new Axis{
                    Name = "Revenue (VNĐ)",
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 12,
                    Labeler = value => value.ToString("N0") + " ₫"
                },

                // Trục Y bên phải: lượt đặt
                new Axis{
                    Name = "Bookings",
                    Position = LiveChartsCore.Measure.AxisPosition.End,
                    LabelsPaint = new SolidColorPaint(SKColors.DarkRed),
                    TextSize = 12,
                    Labeler = value => value.ToString("N0")
                }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }
    }

}

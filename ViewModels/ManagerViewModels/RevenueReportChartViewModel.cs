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
using System.Windows.Media;

namespace HotelManager.ViewModels.ManagerViewModels
{
    public class RevenueReportChartViewModel : BaseViewModel
    {
        InVoiceService inVoiceService;

// time units
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

// start date
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
                    SetTimeBackground(false);
                }
            }
        }

// end date
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
                    SetTimeBackground(false);
                }
            }
        }

// time ranges
        public ObservableCollection<string> TimeRanges { get; set; }

        private string _selectedTimeRange;
        public string SelectedTimeRange
        {
            get => _selectedTimeRange;
            set
            {
                if (_selectedTimeRange != value)
                {
                    _selectedTimeRange = value;
                    OnPropertyChanged(nameof(SelectedTimeRange));
                    SetTimeRange();
                    SetTimeBackground(true);
                }
            }
        }

// room types
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

        // high light
        private Brush _timeRangeBackground;
        public Brush TimeRangeBackground
        {
            get => _timeRangeBackground;
            set
            {
                if (_timeRangeBackground != value)
                {
                    _timeRangeBackground = value;
                    OnPropertyChanged(nameof(TimeRangeBackground));
                }
            }
        }

        private Brush _dateBackground;
        public Brush DateBackground
        {
            get => _dateBackground;
            set
            {
                if (_dateBackground != value)
                {
                    _dateBackground = value;
                    OnPropertyChanged(nameof(DateBackground));
                }
            }
        }


        // chart elements
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

// constructor
        public RevenueReportChartViewModel()
        {
            inVoiceService = new InVoiceService(new Data.HotelDbContext());
            TimeUnitInit();
            TimeRangeInit();
            RoomTypeInit();
            UpdateChartAsync();
        }

        void TimeUnitInit()
        {
            TimeUnits = new ObservableCollection<string>
            {
                "Weekdays",
                "Every day",
                "12 months",
                "Every month",
            };
            SelectedTimeUnit = TimeUnits.FirstOrDefault();
        }

        void TimeRangeInit()
        {
            TimeRanges = new ObservableCollection<string>
            {
                "Last 7 days",
                "Last 30 days",
                "Last 90 days",
                "Last 6 months",
                "Last 12 months",
                "Last 3 years",
            };
            SelectedTimeRange = TimeRanges.FirstOrDefault();
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

        void SetTimeRange()
        {
            switch (SelectedTimeRange)
            {
                case "Last 7 days":
                    StartDate = DateTime.Now.AddDays(-7);
                    EndDate = DateTime.Now;
                    break;
                case "Last 30 days":
                    StartDate = DateTime.Now.AddDays(-30);
                    EndDate = DateTime.Now;
                    break;
                case "Last 90 days":
                    StartDate = DateTime.Now.AddDays(-90);
                    EndDate = DateTime.Now;
                    break;
                case "Last 6 months":
                    StartDate = DateTime.Now.AddMonths(-6);
                    EndDate = DateTime.Now;
                    break;
                case "Last 12 months":
                    StartDate = DateTime.Now.AddMonths(-12);
                    EndDate = DateTime.Now;
                    break;
                case "Last 3 years":
                    StartDate = DateTime.Now.AddYears(-3);
                    EndDate = DateTime.Now;
                    break;
            }
        }

        void SetTimeBackground(bool isTimeRange)
        {
            if (isTimeRange)
            {
                TimeRangeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8CCDEB"));
                DateBackground = Brushes.White;
            }
            else
            {
                TimeRangeBackground = Brushes.White;
                DateBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8CCDEB"));
            }
        }


        private async Task UpdateChartAsync()
        {
            if (SelectedTimeUnit == null) return;

            // lấy dữ liệu doanh thu
            var revenueData = await inVoiceService.GetInvoicePaymentOnTimeRangeAsync(StartDate, EndDate, SelectedTimeUnit, SelectedRoomType?.ToString());

            // lấy dữ liệu số lượng invoice
            var bookingCountData = await inVoiceService.GetInvoiceCountOnTimeRangeAsync(StartDate, EndDate, SelectedTimeUnit, SelectedRoomType?.ToString());

            var labels = revenueData.Keys.Union(bookingCountData.Keys).Distinct().ToList();
            labels.Sort();

            var revenueValues = labels.Select((label, index) =>
                new ObservablePoint(index, revenueData.ContainsKey(label) ? (double)revenueData[label] : 0)).ToList();

            var bookingCountValues = labels.Select((label, index) =>
                new ObservablePoint(index, bookingCountData.ContainsKey(label) ? bookingCountData[label] : 0)).ToList();


            Series = new ISeries[]
            {
                // cột doanh thu
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
                    Name = "Revenue"
                },

                // đường số lượng invoice
                new LineSeries<ObservablePoint>
                {
                    Values = bookingCountValues,
                    GeometrySize = 0,
                    GeometryStroke = new SolidColorPaint(SKColors.DarkRed, 2),
                    Stroke = new SolidColorPaint(SKColors.DarkRed, 2),
                    Fill = null,
                    Name = "Invoice Count",
                    ScalesYAt = 1,
                    LineSmoothness = 0
                }
            };

            // khi update biểu đồ, nếu số label > 30 thì cuộn về cuối, default chỉ hiển thị 30 label trong window
            int maxUnits = 31;
            bool enableScrolling = labels.Count > maxUnits;
            double? minLimit = null;
            double? maxLimit = null;
            if (enableScrolling)
            {
                minLimit = labels.Count - maxUnits;
                maxLimit = labels.Count;
            }

            XAxes = new Axis[]{
                new Axis{
                    Labels = labels,
                    Name = "Time",
                    LabelsRotation = 0,
                    MinLimit = minLimit,
                    MaxLimit = maxLimit
                }
            };

            double maxRevenue = revenueValues.Max(p => p.Y ?? 0);
            double maxBooking = bookingCountValues.Max(p => p.Y ?? 0);

            YAxes = new Axis[]{
                // trục Y bên trái: doanh thu
                new Axis{
                    Name = "Revenue (VNĐ)",
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 12,
                    Labeler = value => value.ToString("N0") + " ₫",
                    MinLimit = 0,
                    MaxLimit = maxRevenue * 1.1
                },

                // trục Y bên phải: lượt đặt
                new Axis{
                    Name = "Invoice Count",
                    Position = LiveChartsCore.Measure.AxisPosition.End,
                    LabelsPaint = new SolidColorPaint(SKColors.DarkRed),
                    TextSize = 12,
                    Labeler = value => value.ToString("N0"),
                    MinLimit = 0,
                    MaxLimit = maxBooking * 1.1
                }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }
    }

}

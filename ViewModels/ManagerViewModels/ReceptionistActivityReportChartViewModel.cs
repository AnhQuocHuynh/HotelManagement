using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using System.Collections.ObjectModel;
using HotelManager.Services.Manager;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.ViewModels.ManagerViewModels
{
    internal class ReceptionistActivityReportChartViewModel : BaseViewModel
    {
        ReceptionistService _receptionistService;

        private bool _isUpdatingRange = false;
        private bool _isManualDateChange = false;
        private bool _isInitializing = false;

        // time units
        public ObservableCollection<string> Units { get; set; }

        private string _selectedUnit;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    OnPropertyChanged(nameof(SelectedUnit));
                    if (!_isInitializing)
                    {
                    _ = UpdateChartAsync();
                    }
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

                    if (!_isUpdatingRange && !_isInitializing)
                    {
                        _isManualDateChange = true;
                        SelectedTimeRange = "Custom";
                        _isManualDateChange = false;

                        _ = UpdateChartAsync();
                        SetTimeBackground(false);
                    }
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
                    if (!_isUpdatingRange && !_isInitializing)
                    {
                        _isManualDateChange = true;
                        SelectedTimeRange = "Custom";
                        _isManualDateChange = false;
                        _ = UpdateChartAsync();
                        SetTimeBackground(false);
                    }
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
                    if (!_isManualDateChange)
                    {
                        SetTimeRange();
                        SetTimeBackground(true);
                        if (!_isInitializing)
                        {
                        _ = UpdateChartAsync();
                        }
                    }
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
                    if (!_isInitializing)
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

        public ISeries[] Series { get; set; }
        public Axis[] YAxes { get; set; }
        public Axis[] XAxes { get; set; }
        public string[] Labels { get; set; }

        // constructor
        public ReceptionistActivityReportChartViewModel()
        {
            _isInitializing = true;

            _receptionistService = new ReceptionistService(new Data.HotelDbContext());
            UnitInit();
            TimeRangeInit();
            RoomTypeInit();

            _isInitializing = false;
            _ = UpdateChartAsync();
        }

        void UnitInit()
        {
            Units = new ObservableCollection<string>
            {
                "Counting",
                "Revenue",
            };
            SelectedUnit = Units.FirstOrDefault();
        }
        void TimeRangeInit()
        {
            TimeRanges = new ObservableCollection<string>
            {
                "Last 7 days",
                "Last 1 month",
                "Last 3 months",
                "Last 6 months",
                "Last 1 year",
                "Last 3 years",
                "Custom"
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
            SelectedRoomType = RoomTypes.First();
        }

        void SetTimeRange()
        {
            _isUpdatingRange = true;
            switch (SelectedTimeRange)
            {
                case "Last 7 days":
                    StartDate = DateTime.Now.AddDays(-7).Date;
                    EndDate = DateTime.Now.Date;
                    break;
                case "Last 1 month":
                    StartDate = DateTime.Now.AddDays(-30).Date;
                    EndDate = DateTime.Now.Date;
                    break;
                case "Last 3 months":
                    StartDate = DateTime.Now.AddDays(-90).Date;
                    EndDate = DateTime.Now.Date;
                    break;
                case "Last 6 months":
                    StartDate = DateTime.Now.AddMonths(-6).Date;
                    EndDate = DateTime.Now.Date;
                    break;
                case "Last 1 year":
                    StartDate = DateTime.Now.AddMonths(-12).Date;
                    EndDate = DateTime.Now.Date;
                    break;
                case "Last 3 years":
                    StartDate = DateTime.Now.AddYears(-3).Date;
                    EndDate = DateTime.Now.Date;
                    break;
                case "Custom":
                    _isUpdatingRange = false;
                    return;
            }
            _isUpdatingRange = false;
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

        public async Task UpdateChartAsync()
        {
            if (SelectedUnit == "Counting")
            {
                var data = await _receptionistService.GetCountBookingByReceptionistAsync(
                    StartDate,
                    EndDate,
                    SelectedRoomType?.ToString()
                    );

                // kiem tra data tra ve
                foreach (var entry in data)
                {
                    var name = entry.Key;
                    var bookingCreated = entry.Value.BookingCount;
                    var checkIn = entry.Value.CheckInCount;
                    var checkOut = entry.Value.CheckOutCount;

                    Debug.WriteLine($"{name} => Booking: {bookingCreated}, CheckIn: {checkIn}, CheckOut: {checkOut}");
                    Console.WriteLine($"{name} => CheckIn: Booking: {bookingCreated}, {checkIn}, CheckOut: {checkOut}");
                }

                Labels = data.Select(x => x.Key).ToArray();

                var bookingCreatedValues = data.Select(x => x.Value.BookingCount).ToArray();
                var checkInValues = data.Select(x => x.Value.CheckInCount).ToArray();
                var checkOutValues = data.Select(x => x.Value.CheckOutCount).ToArray();

                int maxValue = new[] { bookingCreatedValues.Max(), checkInValues.Max(), checkOutValues.Max() }.Max();

                Series = new ISeries[]
                {
                    new RowSeries<int>
                    {
                        Name = "Booking Created",
                        Values = bookingCreatedValues,
                        Stroke = null,
                        MaxBarWidth = 25,
                        DataLabelsSize = 14,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                        DataLabelsFormatter = point => point.Model == 0 ? "0" : point.Model.ToString(),
                        Fill = new SolidColorPaint(SKColors.LightGreen),
                    },
                    new RowSeries<int>
                    {
                        Name = "Check-In",
                        Values = checkInValues,
                        Stroke = null,
                        MaxBarWidth = 25,
                        DataLabelsSize = 14,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                        DataLabelsFormatter = point => point.Model == 0 ? "0" : point.Model.ToString(),
                        Fill = new SolidColorPaint(SKColors.SkyBlue),
                    },
                    new RowSeries<int>
                    {
                        Name = "Check-Out",
                        Values = checkOutValues,
                        Stroke = null,
                        MaxBarWidth = 25,
                        DataLabelsSize = 14,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                        DataLabelsFormatter = point => point.Model == 0 ? "0" : point.Model.ToString(),
                        Fill = new SolidColorPaint(SKColors.Orange),
                    }
                };

                // Trục OX là số lần (Count)
                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Count",
                        MinLimit = 0,
                        MaxLimit = maxValue * 1.1, // thêm chút khoảng trắng
                        NameTextSize = 14,
                        TextSize = 12,
                        Labeler = value => value.ToString("N0"),
                        MinStep = 1
                    }
                };
            }
            else if (SelectedUnit == "Revenue")
            {
                var data = await _receptionistService.GetRevenueBreakdownByReceptionistAsync(
                    StartDate, 
                    EndDate, 
                    SelectedRoomType?.ToString());

                Labels = data.Select(x => x.Key).ToArray();

                var bookingRevenueValues = data.Select(x => x.Value.BookingRevenue).ToArray();
                var checkInRevenueValues = data.Select(x => x.Value.CheckInRevenue).ToArray();
                var checkOutRevenueValues = data.Select(x => x.Value.CheckOutRevenue).ToArray();

                decimal maxValue = new[] { bookingRevenueValues.Max(), checkInRevenueValues.Max(), checkOutRevenueValues.Max() }.Max();

                Series = new ISeries[]
                {
                    new RowSeries<decimal>
                    {
                        Name = "Booking Revenue",
                        Values = bookingRevenueValues,
                        Stroke = null,
                        MaxBarWidth = 25,
                        DataLabelsSize = 14,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                        DataLabelsFormatter = point => point.Model == 0 ? "0 ₫" : point.Model.ToString("N0") + " ₫",
                        Fill = new SolidColorPaint(SKColors.LightGreen),
                    },
                    new RowSeries<decimal>
                    {
                        Name = "Check-In Revenue",
                        Values = checkInRevenueValues,
                        Stroke = null,
                        MaxBarWidth = 25,
                        DataLabelsSize = 14,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                        DataLabelsFormatter = point => point.Model == 0 ? "0 ₫" : point.Model.ToString("N0") + " ₫",
                        Fill = new SolidColorPaint(SKColors.SkyBlue),
                    },
                    new RowSeries<decimal>
                    {
                        Name = "Check-Out Revenue",
                        Values = checkOutRevenueValues,
                        Stroke = null,
                        MaxBarWidth = 25,
                        DataLabelsSize = 14,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                        DataLabelsFormatter = point => point.Model == 0 ? "0 ₫" : point.Model.ToString("N0") + " ₫",
                        Fill = new SolidColorPaint(SKColors.Orange),
                    }
                };

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Revenue (VNĐ)",
                        MinLimit = 0,
                        MaxLimit = (double)(maxValue * 1.1m),
                        NameTextSize = 14,
                        TextSize = 12,
                        Labeler = value => value.ToString("N0") + " ₫",
                        MinStep = 1000
                    }
                };
            }


            // Trục OY là tên nhân viên
            YAxes = new Axis[]
            {
                new Axis
                {
                    Labels = Labels,
                    LabelsRotation = 0,
                    TextSize = 14
                }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }

    }
}
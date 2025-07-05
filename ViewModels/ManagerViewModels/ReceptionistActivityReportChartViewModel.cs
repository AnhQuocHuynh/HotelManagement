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
using HotelManager.Data;
using HotelManager.ViewModels.ManagerViewModels.UIModel;

namespace HotelManager.ViewModels.ManagerViewModels
{
    public class ReceptionistActivityReportChartViewModel : BaseViewModel
    {
        ReceptionistService _receptionistService;

        // khi start date hoặc end date đổi refresh chart
        // khi dùng time ranges sẽ set nhanh cả start với end date
        // ==> nếu set time range nhanh cần đợi set cả start và end date xong mới refresh chart
        private bool _isUpdatingRange = false;


        // khi gọi constructor sẽ set giá trị cho tất cả unit, startDate, endDate, roomType
        // ==> refresh chart bị gọi 4 lần
        // ==> cần vô hiệu hóa refresh chart khi đang set giá trị trong constructor đến khi hoàn tất
        private bool _isInitializing = false;

        // tránh gọi update data liên tục khi data chưa update xong
        private bool _isLoading = false;

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
                    _ = RefreshChartAsync();
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
                        SelectedTimeRange = "Custom";

                        _ = RefreshChartAsync();

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
                        SelectedTimeRange = "Custom";
                        _ = RefreshChartAsync();

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
                    SetTimeRange();
                    SetTimeBackground(true);
                    if (!_isInitializing)
                    {
                        _ = RefreshChartAsync();

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
                        _ = RefreshChartAsync();

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

        private ISeries[] _series;
        public ISeries[] Series
        {
            get => _series;
            set { _series = value; OnPropertyChanged(); }
        }

        private Axis[] _yAxes;
        public Axis[] YAxes {
            get => _yAxes;
            set { _yAxes = value; OnPropertyChanged(); }
        }

        private Axis[] _xAxes;
        public Axis[] XAxes { 
            get => _xAxes;
            set { _xAxes = value; OnPropertyChanged(); }
        }

        private string[] _labels;
        public string[] Labels { 
            get => _labels;
            set { _labels = value; OnPropertyChanged(); } 
        }

        // constructor
        private IServiceScope _scope;

        public ReceptionistActivityReportChartViewModel()
        {
            _isInitializing = true;

            _scope = App.ServiceProvider.CreateScope(); // GIỮ scope trong ViewModel
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            _receptionistService = new ReceptionistService(dbContext);

            UnitInit();
            TimeRangeInit();
            RoomTypeInit();

            _isInitializing = false;
            _ = RefreshChartAsync();
        }

        public void Dispose()
        {
            _scope?.Dispose();
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
            DateTime today = DateTime.Today;
            _isUpdatingRange = true;
            switch (SelectedTimeRange)
            {
                case "Last 7 days":
                    StartDate = DateTime.Now.AddDays(-7).Date;
                    EndDate = today;
                    break;
                case "Last 1 month":
                    StartDate = DateTime.Now.AddDays(-30).Date;
                    EndDate = today;
                    break;
                case "Last 3 months":
                    StartDate = DateTime.Now.AddDays(-90).Date;
                    EndDate = today;
                    break;
                case "Last 6 months":
                    StartDate = DateTime.Now.AddMonths(-6).Date;
                    EndDate = today;
                    break;
                case "Last 1 year":
                    StartDate = DateTime.Now.AddMonths(-12).Date;
                    EndDate = today;
                    break;
                case "Last 3 years":
                    StartDate = DateTime.Now.AddYears(-3).Date;
                    EndDate = today;
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

        // load data
        public async Task<ReceptionistChartData> FetchChartDataAsync()
        {
            var chartData = new ReceptionistChartData();
            try
            {
                Debug.WriteLine($"🚀 LoadDataAsync called, SelectedUnit = {SelectedUnit}");

                if (SelectedUnit == "Counting")
                {
                    Debug.WriteLine("⚙ Counting branch selected");
                    var data = await _receptionistService.GetCountBookingByReceptionistAsync(
                        StartDate, EndDate, SelectedRoomType?.ToString());
                    Debug.WriteLine($"Load Counting: found {data.Count} receptionists");

                    chartData.Labels = data.Select(x => x.Key).ToArray();
                    chartData.BookingCounts = data.Select(x => x.Value.BookingCount).ToArray();
                    chartData.CheckInCounts = data.Select(x => x.Value.CheckInCount).ToArray();
                    chartData.CheckOutCounts = data.Select(x => x.Value.CheckOutCount).ToArray();
                }
                else if (SelectedUnit == "Revenue")
                {
                    Debug.WriteLine("💰 Revenue branch selected");
                    var data = await _receptionistService.GetRevenueBreakdownByReceptionistAsync(
                        StartDate, EndDate, SelectedRoomType?.ToString());
                    Debug.WriteLine($"Load Revenue: found {data.Count} receptionists");

                    chartData.Labels = data.Select(x => x.Key).ToArray();
                    chartData.BookingRevenues = data.Select(x => x.Value.BookingRevenue).ToArray();
                    chartData.CheckInRevenues = data.Select(x => x.Value.CheckInRevenue).ToArray();
                    chartData.CheckOutRevenues = data.Select(x => x.Value.CheckOutRevenue).ToArray();
                }
                else
                {
                    Debug.WriteLine("🚨 SelectedUnit not matched any case!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🚨 Exception in LoadDataAsync: {ex.Message}");
            }

            return chartData;
        }



        // update chart based on data
        public void UpdateChart(ReceptionistChartData data)
        {
            if (SelectedUnit == "Counting")
            {
                int maxValue = new[] {
            data.BookingCounts.DefaultIfEmpty(0).Max(),
            data.CheckInCounts.DefaultIfEmpty(0).Max(),
            data.CheckOutCounts.DefaultIfEmpty(0).Max()
        }.Max();

                Series = new ISeries[]
                {
            new RowSeries<int>
            {
                Name = "Booking Created",
                Values = data.BookingCounts,
                Fill = new SolidColorPaint(SKColors.LightGreen),
                MaxBarWidth = 25,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = DataLabelsPosition.End,
                DataLabelsFormatter = point => point.Model == 0 ? "0" : point.Model.ToString(),
            },
            new RowSeries<int>
            {
                Name = "Check-In",
                Values = data.CheckInCounts,
                Fill = new SolidColorPaint(SKColors.SkyBlue),
                MaxBarWidth = 25,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = DataLabelsPosition.End,
                DataLabelsFormatter = point => point.Model == 0 ? "0" : point.Model.ToString(),
            },
            new RowSeries<int>
            {
                Name = "Check-Out",
                Values = data.CheckOutCounts,
                Fill = new SolidColorPaint(SKColors.Orange),
                MaxBarWidth = 25,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = DataLabelsPosition.End,
                DataLabelsFormatter = point => point.Model == 0 ? "0" : point.Model.ToString(),
            }
                };

                XAxes = new Axis[]
                {
            new Axis
            {
                Name = "Count",
                MinLimit = 0,
                MaxLimit = maxValue * 1.1,
                NameTextSize = 14,
                TextSize = 12,
                Labeler = value => value.ToString("N0"),
                MinStep = 1
            }
                };
            }
            else if (SelectedUnit == "Revenue")
            {
                decimal maxValue = new[] {
            data.BookingRevenues.DefaultIfEmpty(0).Max(),
            data.CheckInRevenues.DefaultIfEmpty(0).Max(),
            data.CheckOutRevenues.DefaultIfEmpty(0).Max()
        }.Max();

                Series = new ISeries[]
                {
            new RowSeries<decimal>
            {
                Name = "Booking Revenue",
                Values = data.BookingRevenues,
                Fill = new SolidColorPaint(SKColors.LightGreen),
                MaxBarWidth = 25,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = DataLabelsPosition.End,
                DataLabelsFormatter = point => point.Model == 0 ? "0 ₫" : point.Model.ToString("N0") + " ₫",
            },
            new RowSeries<decimal>
            {
                Name = "Check-In Revenue",
                Values = data.CheckInRevenues,
                Fill = new SolidColorPaint(SKColors.SkyBlue),
                MaxBarWidth = 25,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = DataLabelsPosition.End,
                DataLabelsFormatter = point => point.Model == 0 ? "0 ₫" : point.Model.ToString("N0") + " ₫",
            },
            new RowSeries<decimal>
            {
                Name = "Check-Out Revenue",
                Values = data.CheckOutRevenues,
                Fill = new SolidColorPaint(SKColors.Orange),
                MaxBarWidth = 25,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = DataLabelsPosition.End,
                DataLabelsFormatter = point => point.Model == 0 ? "0 ₫" : point.Model.ToString("N0") + " ₫",
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

            YAxes = new Axis[]
            {
        new Axis
        {
            Labels = data.Labels,
            LabelsRotation = 0,
            TextSize = 14
        }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }

        public async Task RefreshChartAsync()
        {
            // ngane gọi refresh đến khi refresh xong
            if (_isLoading) return;
            try
            {
                _isLoading = true;
                var data = await FetchChartDataAsync();
                // update chart sau khi đủ data
                UpdateChart(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🚨 Exception in RefreshChartAsync: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }


    }
}
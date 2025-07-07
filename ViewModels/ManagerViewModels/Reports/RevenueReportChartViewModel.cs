using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using HotelManager.Models.Enums;
using System.Windows.Media;
using HotelManager.Services.Manager;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Data;
using HotelManager.Models;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.SKCharts;



namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class RevenueReportChartViewModel : BaseViewModel
    {
        InVoiceService inVoiceService;
        ExportService _exportService;

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
                    if (!_isInitializing)
                        _ = RefreshChartAsync();

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
                        _ = RefreshChartAsync();
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


        // chart elements
        private ISeries[] _series;
        public ISeries[] Series
        {
            get => _series;
            set { _series = value; OnPropertyChanged(); }
        }

        private Axis[] _yAxes;
        public Axis[] YAxes
        {
            get => _yAxes;
            set { _yAxes = value; OnPropertyChanged(); }
        }

        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get => _xAxes;
            set { _xAxes = value; OnPropertyChanged(); }
        }


        // commands
        public ICommand ExportChartAndDataCommand { get; }


        // constructor
        private IServiceScope _scope;

        public RevenueReportChartViewModel()
        {
            _isInitializing = true;

            _scope = App.ServiceProvider.CreateScope(); // GIỮ scope trong ViewModel
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            inVoiceService = new InVoiceService(dbContext);
            _exportService = new ExportService();

            TimeUnitInit();
            TimeRangeInit();
            RoomTypeInit();

            ExportChartAndDataCommand = new RelayCommand(ExportChartAndData);

            _isInitializing = false;
            _ = RefreshChartAsync();

        }
        public void Dispose()
        {
            _scope?.Dispose();
        }


        // export dữ liệu ra file excel
        private void ExportChartAndData()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "RevenueReport.xlsx"

            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _exportService.ExportRevenueDataWithChart(_chartData, saveFileDialog.FileName);
            }
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
                "Last 1 month",
                "Last 3 months",
                "Last 6 months",
                "Last 1 year",
                "Last 3 years",
                "Custom"
            };
            SelectedTimeRange = TimeRanges.First();
        }

        void RoomTypeInit()
        {
            RoomTypes = new ObservableCollection<object>
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


        // get data
        public Dictionary<string, (decimal revenue, int invoiceCount)> _chartData;
        private async Task FetchChartDataAsync()
        {
            var data = await inVoiceService.GetInvoiceStatsGroupedAsync(
                StartDate,
                EndDate,
                SelectedTimeUnit,
                SelectedRoomType?.ToString()
            );

            _chartData = data ?? new Dictionary<string, (decimal revenue, int invoiceCount)>();
        }

        // update chart
        private void UpdateChart()
        {
            if (_chartData == null || _chartData.Count == 0)
            {
                Series = Array.Empty<ISeries>();
                XAxes = Array.Empty<Axis>();
                YAxes = Array.Empty<Axis>();
                OnPropertyChanged(nameof(Series));
                OnPropertyChanged(nameof(XAxes));
                OnPropertyChanged(nameof(YAxes));
                return;
            }

            var labels = _chartData.Keys.ToList();
            labels.Sort();

            var revenueValues = labels.Select((label, index) =>
            {
                var data = _chartData.TryGetValue(label, out var value) ? value : (0m, 0);
                return new ObservablePoint(index, (double)value.revenue);
            }).ToList();

            var invoiceCountValues = labels.Select((label, index) =>
            {
                var data = _chartData.TryGetValue(label, out var value) ? value : (0m, 0);
                return new ObservablePoint(index, value.invoiceCount);
            }).ToList();

            Series = new ISeries[]
            {
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
            Name = "Revenue",
            Fill = new SolidColorPaint(SKColors.DeepSkyBlue),
        },
        new LineSeries<ObservablePoint>
        {
            Values = invoiceCountValues,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(SKColors.DarkRed,2),
            Fill = null,
            Name = "Invoice Count",
            ScalesYAt = 1,
            LineSmoothness = 0
        }
            };

            int maxUnits = 31;
            bool enableScrolling = labels.Count > maxUnits;
            double? minLimit = null;
            double? maxLimit = null;
            if (enableScrolling)
            {
                minLimit = labels.Count - maxUnits;
                maxLimit = labels.Count;
            }

            XAxes = new Axis[]
            {
        new Axis
        {
            Labels = labels,
            Name = "Time",
            LabelsRotation = 0,
            MinLimit = minLimit,
            MaxLimit = maxLimit
        }
            };

            double maxRevenue = revenueValues.Max(p => p.Y ?? 0);
            double maxInvoiceCount = invoiceCountValues.Max(p => p.Y ?? 0);

            YAxes = new Axis[]
            {
        new Axis
        {
            Name = "Revenue (VNĐ)",
            LabelsPaint = new SolidColorPaint(SKColors.Black),
            TextSize = 12,
            Labeler = value => value.ToString("N0") + " ₫",
            MinLimit = 0,
            MaxLimit = maxRevenue * 1.1
        },
        new Axis
        {
            Name = "Invoice Count",
            Position = AxisPosition.End,
            LabelsPaint = new SolidColorPaint(SKColors.DarkRed),
            TextSize = 12,
            Labeler = value => value.ToString("N0"),
            MinLimit = 0,
            MaxLimit = maxInvoiceCount * 1.1
        }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }


        // rèfresh chart
        private async Task RefreshChartAsync()
        {
            // ngăn khi đang refresh
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                await FetchChartDataAsync();
                //update chart sau khi đủ data
                UpdateChart();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 Error in RefreshChartAsync: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }


    }

}

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
using System.Runtime.CompilerServices;



namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class RevenueReportChartViewModel : BaseViewModel, IDisposable
    {
        InVoiceService inVoiceService;
        ExportService _exportService;

        // Loading state
        private bool _isUpdatingRange = false;
        // Indicates that filters are being initialised, prevents redundant refresh
        private bool _isInitializing = false;

        // loading to disable UI while refreshing chart
        private bool _isLoading = false;
        private bool _disposed = false;

        // kiểm tra có thay đổi gì không
        private bool _isChanged = false;

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
                    _isChanged = true; // mark as changed so refresh knows
                    if (!_isInitializing)
                        _ = ExecuteSafelyAsync(() => RefreshChartAsync(), "Refresh chart data");
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
                    //nếu startdate lớn hơn enddate thì set startdate = enddate
                    // endDate fix khi lọc là endDate ở 23h59' của endDate nên time range khi lọc là 1 ngày
                    if (value > EndDate)
                    {
                        _startDate = EndDate;
                    }
                    else
                    {
                        _startDate = value;
                    }
                    OnPropertyChanged(nameof(StartDate));
                    SetTimeBackground(false);
                    _isChanged = true; // đánh dấu đã thay đổi
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
                    //nếu startdate lớn hơn enddate thì set startdate = enddate
                    // endDate fix khi lọc là endDate ở 23h59' của endDate nên time range khi lọc là 1 ngày
                    if (value < StartDate)
                    {
                        _endDate = StartDate;
                    }
                    else
                    {
                        _endDate = value;
                    }
                    OnPropertyChanged(nameof(EndDate));
                    _isChanged = true; // đánh dấu đã thay đổi
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
                    _isChanged = true; // đánh dấu đã thay đổi
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
                    OnPropertyChanged(nameof(SelectedRoomType));;
                    _isChanged = true; // đánh dấu đã thay đổi
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
        public ICommand ApplyFilterCommand { get; }

        // constructor
        private IServiceScope _scope;

        private string _selectedPeriod = "";

        public RevenueReportChartViewModel()
        {

            _scope = App.ServiceProvider.CreateScope(); // GIỮ scope trong ViewModel
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            inVoiceService = new InVoiceService(dbContext);
            _exportService = new ExportService();

            ExportChartAndDataCommand = new RelayCommand(ExportChartAndData);
            ApplyFilterCommand = new AsyncRelayCommand(RefreshChartAsync);


            _ = InitAsync();

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _scope?.Dispose();
                    // Dispose other resources nếu có
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing RevenueReportChartViewModel: {ex.Message}");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        public async Task InitAsync()
        {
            TimeUnitInit();
            TimeRangeInit();
            RoomTypeInit();
            await RefreshChartAsync();
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

            EndDate = today;
            switch (SelectedTimeRange)
            {
                case "Last 7 days":
                    StartDate = today.AddDays(-7);
                break;
                case "Last 1 month":
                    StartDate = today.AddMonths(-1);
                    break;
                case "Last 3 months":
                    StartDate = today.AddMonths(-3);
                    break;
                case "Last 6 months":
                    StartDate = today.AddMonths(-6);
                    break;
                case "Last 1 year":
                    StartDate = today.AddYears(-1);
                    break;
                case "Last 3 years":
                    StartDate = today.AddYears(-3);
                    break;
                Default:
                    return;
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


        // get data
        public Dictionary<string, (decimal revenue, int invoiceCount)> _chartData;
        private async Task FetchChartDataAsync()
        {
            try
            {
                // fix EndDate to include the whole day (23:59:59)
                var fixedEndDate = EndDate.Date.AddDays(1).AddTicks(-1);
                string roomType = SelectedRoomType is RoomType rt ? rt.ToString() : SelectedRoomType?.ToString();

                var data = await inVoiceService.GetInvoiceStatsGroupedAsync(
                    StartDate,
                    fixedEndDate,
                    SelectedTimeUnit,
                    roomType
                ).ConfigureAwait(false);

                _chartData = data ?? new Dictionary<string, (decimal revenue, int invoiceCount)>();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error fetching chart data");
                _chartData = new Dictionary<string, (decimal revenue, int invoiceCount)>();
                throw; // Re-throw để caller handle
            }
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
            // Prevent refresh when already loading, disposed, or nothing changed
            if (_isLoading || _disposed || !_isChanged) return;
            _isLoading = true;

            try
            {
                await FetchChartDataAsync().ConfigureAwait(false);
                //update chart sau khi đủ data
                UpdateChart();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error refreshing chart");
                ShowError($"Lỗi khi cập nhật biểu đồ: {ex.Message}");
            }
            finally
            {
                _isChanged = false; // reset flag sau khi bắt đầu refresh
                _isLoading = false;
            }
        }

        private void SetSelectedPeriod(string value)
        {
            _selectedPeriod = value;
            OnPropertyChanged();
            _ = ExecuteSafelyAsync(() => RefreshChartAsync(), "Refresh chart data");
        }

    }

}

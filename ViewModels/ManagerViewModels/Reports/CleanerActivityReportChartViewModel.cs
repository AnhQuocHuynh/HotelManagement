using HotelManager.Data;
using HotelManager.Services;
using HotelManager.Services.Manager;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;

namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class CleanerActivityReportChartViewModel : BaseViewModel, IDisposable
    {
        private readonly ICleanerActivityService _cleanerActivityService;
        private readonly ExportService _exportService;

        private bool _isChanged = false;

        public ObservableCollection<string> ViewTypeOptions { get; set; }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value > EndDate ? EndDate : value;
                    OnPropertyChanged(nameof(StartDate));
                    _isChanged = true; // Đánh dấu đã thay đổi
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
                    _endDate = value < StartDate ? StartDate : value;
                    OnPropertyChanged(nameof(EndDate));
                    _isChanged = true; // Đánh dấu đã thay đổi
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
                    _isChanged = true;
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
            SelectedTimeRange = TimeRanges.FirstOrDefault();
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

        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(); }
        }

        // commands
        public ICommand ExportChartAndDataCommand { get; }
        public ICommand ApplyFilterCommand { get; }


        public CleanerActivityReportChartViewModel(ICleanerActivityService cleanerActivityService, ExportService exportService)
        {
            _cleanerActivityService = cleanerActivityService;
            _exportService = exportService;

            TimeRangeInit();
            _ = RefreshChartAsync();

            ApplyFilterCommand = new RelayCommand(async () =>
            {
                await RefreshChartAsync();
            });

            ExportChartAndDataCommand = new RelayCommand(ExportChartAndData);
        }
        public void Dispose()
        {
            // Xóa IServiceScope _scope và Dispose
        }

        private void ExportChartAndData()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "CleanerActivityReport.xlsx"

            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _exportService.ExportCleanerActivitiesToExcel(_chartData, saveFileDialog.FileName);
            }
        }



        public Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)> _chartData;
        private async Task FetchChartDataAsync()
        {
            DateTime fixedEndDate = EndDate.Date.AddDays(1).AddTicks(-1);


        var data = await _cleanerActivityService.getCountNumbersOfRoomEachCleanerCleaned(
                StartDate,
                fixedEndDate
            );

            _chartData = data ?? new Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>();

            System.Diagnostics.Debug.WriteLine($"=== FetchChartDataAsync result for {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} ===");
            if (_chartData.Any())
            {
                foreach (var item in _chartData)
                {
                    var (deluxe, standard, suite) = item.Value;
                    System.Diagnostics.Debug.WriteLine($"Cleaner: {item.Key} | Deluxe: {deluxe}, Standard: {standard}, Suite: {suite}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠ Không có dữ liệu cleaner nào được trả về.");
            }

        }

        // update chart
        private void UpdateChart()
        {
            if (_chartData == null || !_chartData.Any())
            {
                Series = Array.Empty<ISeries>();
                Labels = Array.Empty<string>();
                return;
            }

            // chuẩn bị data
            var labels = _chartData.Keys.ToArray();
            Labels = labels; // 🔥 QUAN TRỌNG NHẤT: cập nhật Labels

            // tạo series cột
            Series = new ISeries[]
            {
    new RowSeries<int>
    {
        Name = "Deluxe",
        Values = _chartData.Values.Select(x => x.DeluxeCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.SkyBlue),
        MaxBarWidth = 25,
    },
    new RowSeries<int>
    {
        Name = "Standard",
        Values = _chartData.Values.Select(x => x.StandardCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.Orange),
        MaxBarWidth = 25,
    },
    new RowSeries<int>
    {
        Name = "Suite",
        Values = _chartData.Values.Select(x => x.SuiteCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.Purple),
        MaxBarWidth = 25,
    }
            };

            XAxes = new Axis[]
            {
    new Axis
    {
        Name = "Số phòng đã dọn",
        SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                MinStep = 1,
                MinLimit = 0,
        Labeler = value => ((int)value).ToString()
    }
            };

            YAxes = new Axis[]
            {
    new Axis
    {
        Labels = Labels, // Cleaner names
        LabelsRotation = 0
    }
            };

        }




        // rèfresh chart

        // tránh gọi update data liên tục khi data chưa update xong
        private bool _isLoading = false;
        private async Task RefreshChartAsync()
        {
            // ngăn khi đang refresh
            if (_isLoading || !_isChanged) return;

            try
            {
                _isLoading = true;
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
                _isChanged = false; // Reset flag sau khi cập nhật
            }
        }


    }
}

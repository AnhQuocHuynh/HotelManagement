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

namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class CleanerActivityReportChartViewModel : BaseViewModel
    {
        CleanerActivityService cleanerActivityService;
        ExportService _exportService;

        public ObservableCollection<string> ViewTypeOptions { get; set; }

        private string _selectedViewType;
        public string SelectedViewType
        {
            get => _selectedViewType;
            set
            {
                if (_selectedViewType != value)
                {
                    _selectedViewType = value;
                    OnPropertyChanged(nameof(SelectedViewType));
                }
            }
        }

        private void ViewTypeOptionsInit()
        {
            ViewTypeOptions = new ObservableCollection<string>
            {
                "Total by time",
                "Each cleaner on time",
            };
            SelectedViewType = ViewTypeOptions.FirstOrDefault();
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


        private IServiceScope _scope;
        public CleanerActivityReportChartViewModel()
        {
            _scope = App.ServiceProvider.CreateScope(); // GIỮ scope trong ViewModel
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            cleanerActivityService = new CleanerActivityService(dbContext);
            _exportService = new ExportService();

            ViewTypeOptionsInit();
            StartDate = DateTime.Now.AddDays(-7);
            EndDate = DateTime.Now;

            _ = RefreshChartAsync();

            ApplyFilterCommand = new RelayCommand(async () =>
            {
                await RefreshChartAsync();
            });

            ExportChartAndDataCommand = new RelayCommand(ExportChartAndData);
        }
        public void Dispose()
        {
            _scope?.Dispose();
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
            var data = await cleanerActivityService.getCountNumbersOfRoomEachCleanerCleaned(
                StartDate,
                EndDate
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

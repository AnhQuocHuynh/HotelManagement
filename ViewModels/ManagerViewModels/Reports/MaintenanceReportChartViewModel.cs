using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Services.Manager;
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
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Axis = LiveChartsCore.SkiaSharpView.Axis;
using LiveChartsCore.Defaults;
using HotelManager.Models.Enums;


namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class MaintenanceReportChartViewModel : BaseViewModel, IDisposable
    {
        MaintenanceDataService maintenanceDataService;
        ExportService _exportService;

        private bool _isChanged = false;

        public ObservableCollection<string> ViewTypeOptions { get; set; }
        private void ViewTypeOptionsInit()
        {
            ViewTypeOptions = new ObservableCollection<string>
            {
                "Each technician on time",
                "Each room on time",
                "Monthly by room type"
            };
            SelectedViewType = ViewTypeOptions.FirstOrDefault();
        }

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
                    _isChanged = true; // Đánh dấu đã thay đổi
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
                    _startDate = value > EndDate ? EndDate : value;
                    OnPropertyChanged(nameof(StartDate));
                    _isChanged = true; // Đánh dấu đã thay đổi
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
                }
            }
        }


        // char settings
        private LiveChartsCore.Measure.ZoomAndPanMode _zoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Y;
        public LiveChartsCore.Measure.ZoomAndPanMode ZoomMode
        {
            get => _zoomMode;
            set { _zoomMode = value; OnPropertyChanged(); }
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
        public MaintenanceReportChartViewModel()
        {
            _scope = App.ServiceProvider.CreateScope(); // GIỮ scope trong ViewModel
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            maintenanceDataService = new MaintenanceDataService(dbContext);
            _exportService = new ExportService();

            ViewTypeOptionsInit();
            EndDate = DateTime.Now;
            StartDate = DateTime.Now.AddDays(-7);


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


        // each technician
        public Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)> eachTechnicianData;
        private async Task FetchEachTechnicianChartDataAsync()
        {
            var fixedEndDate = EndDate.Date.AddDays(1).AddTicks(-1);

            var data = await maintenanceDataService.GetCountNumbersOfRoomEachTechnicianMaintained(
                StartDate,
                fixedEndDate
            );

            eachTechnicianData = data ?? new Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>();
        }

        // update chart 
        private void UpdateChartEachTechnician()
        {
            if (eachTechnicianData == null || !eachTechnicianData.Any())
            {
                Series = Array.Empty<ISeries>();
                Labels = Array.Empty<string>();
                return;
            }

            // chuẩn bị data
            var labels = eachTechnicianData.Keys.ToArray();
            Labels = labels;

            // tạo series cột
            Series = new ISeries[]
            {
    new RowSeries<int>
    {
        Name = "Deluxe",
        Values = eachTechnicianData.Values.Select(x => x.DeluxeCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.SkyBlue),
        MaxBarWidth = 25,
    },
    new RowSeries<int>
    {
        Name = "Standard",
        Values = eachTechnicianData.Values.Select(x => x.StandardCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.Orange),
        MaxBarWidth = 25,
    },
    new RowSeries<int>
    {
        Name = "Suite",
        Values = eachTechnicianData.Values.Select(x => x.SuiteCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.Purple),
        MaxBarWidth = 25,
    }
            };

            XAxes = new Axis[]
            {
    new Axis
    {
        Name = "Số phòng sửa chữa",
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
        Name = "Technicians",
        Labels = Labels,
        LabelsRotation = 0
    }
            };
        }



        // each room
        Dictionary<string, (string RoomType, int MaintenanceCount)> eachRoomData;
        private async Task FetchEachRoomChartDataAsync()
        {
            var fixedEndDate = EndDate.Date.AddDays(1).AddTicks(-1);

            var data = await maintenanceDataService.GetCountNumbersOfMaintenanceEachRoom(
                StartDate,
                fixedEndDate
            );

            eachRoomData = data ?? new Dictionary<string, (string RoomType, int MaintenanceCount)>();
        }
        private void UpdateChartEachRoom()
        {
            if (eachRoomData == null || !eachRoomData.Any())
            {
                Series = Array.Empty<ISeries>();
                Labels = Array.Empty<string>();
                return;
            }

            var roomList = eachRoomData.Keys.ToList();
            Labels = roomList.ToArray();

            // chuẩn bị mảng ObservableValue chứa số lần bảo trì cho mỗi phòng
            var values = new ObservableValue[roomList.Count];

            for (int i = 0; i < roomList.Count; i++)
            {
                var roomName = roomList[i];
                var (_, maintenanceCount) = eachRoomData[roomName];
                values[i] = new ObservableValue(maintenanceCount);
            }

            Series = new ISeries[]
            {
        new RowSeries<ObservableValue>
        {
            Name = "Số lần bảo trì",
            Values = values,
            Fill = new SolidColorPaint(SKColors.ForestGreen),
        }
            };

            XAxes = new Axis[]
            {
        new Axis
        {
            Name = "Số lần bảo trì",
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
            Name = "Phòng",
            Labels = Labels,
            LabelsRotation = 0,
            MinStep = 1
        }
            };
        }



        // chart counting maintenance activities by room type each month
        private Dictionary<string, (int Deluxe, int Standard, int Suite)> _monthlyData;
        private async Task FetchMonthlyMaintenanceChartDataAsync()
        {
            var fixedEndDate = EndDate.Date.AddDays(1).AddTicks(-1);

            var data = await maintenanceDataService.GetMonthlyMaintenanceCountsByRoomType(
                StartDate,
                fixedEndDate
            );

            _monthlyData = data ?? new Dictionary<string, (int Deluxe, int Standard, int Suite)>();
        }
        private void UpdateChartMonthlyMaintenance()
        {
            if (_monthlyData == null || !_monthlyData.Any())
            {
                Series = Array.Empty<ISeries>();
                Labels = Array.Empty<string>();
                return;
            }

            Labels = _monthlyData.Keys.ToArray();

            Series = new ISeries[]
            {
        new ColumnSeries<int>
        {
            Name = "Deluxe",
            Values = _monthlyData.Values.Select(x => x.Deluxe).ToArray(),
            Fill = new SolidColorPaint(SKColors.Gold)
        },
        new ColumnSeries<int>
        {
            Name = "Standard",
            Values = _monthlyData.Values.Select(x => x.Standard).ToArray(),
            Fill = new SolidColorPaint(SKColors.ForestGreen)
        },
        new ColumnSeries<int>
        {
            Name = "Suite",
            Values = _monthlyData.Values.Select(x => x.Suite).ToArray(),
            Fill = new SolidColorPaint(SKColors.CornflowerBlue)
        }
            };

            XAxes = new Axis[]
            {
        new Axis
        {
            Name = "Tháng",
            Labels = Labels,
            LabelsRotation = 15
        }
            };

            YAxes = new Axis[]
            {
        new Axis
        {
            Name = "Số lần bảo trì",
            MinStep = 1,
            MinLimit = 0,
            Labeler = value => ((int)value).ToString()
        }
            };
        }


        // rèfresh chart

        // tránh gọi update data liên tục khi data chưa update xong
        private bool _isLoading = false;
        private async Task RefreshChartAsync()
        {
            if (_isLoading || !_isChanged) return;

            try
            {
                _isLoading = true;

                switch (SelectedViewType)
                {
                    case "Each technician on time":
                        await FetchEachTechnicianChartDataAsync();
                        UpdateChartEachTechnician();
                        ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Y;
                        break;
                    case "Each room on time":
                        await FetchEachRoomChartDataAsync();
                        UpdateChartEachRoom();
                        ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Y;
                        break;
                    case "Monthly by room type":
                        await FetchMonthlyMaintenanceChartDataAsync();
                        UpdateChartMonthlyMaintenance();
                        ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X;
                        break;
                    default:
                        await FetchEachTechnicianChartDataAsync();
                        UpdateChartEachTechnician();
                        ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 Error in RefreshChartAsync: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                _isChanged = false; // Reset the change flag after loading
            }
        }

        private void ExportChartAndData()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "MaintainaceActivityReport.xlsx"

            };
            if (saveFileDialog.ShowDialog() == true)
            {
                switch (SelectedViewType)
                {
                    case "Each technician on time":
                        _exportService.ExportTechnicianActivitiesToExcel(eachTechnicianData, saveFileDialog.FileName);
                        break;
                    case "Each room on time":
                        _exportService.ExportEachRoomMaintenanceToExcel(eachRoomData, saveFileDialog.FileName);
                        break;
                    case "Monthly by room type":
                        _exportService.ExportMonthlyMaintenanceCountsToExcel(_monthlyData, saveFileDialog.FileName);
                        break;
                    default:
                        _exportService.ExportTechnicianActivitiesToExcel(eachTechnicianData, saveFileDialog.FileName);
                        break;
                }
            }
                
        }
    }
}

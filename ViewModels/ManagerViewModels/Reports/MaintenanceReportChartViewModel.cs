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


namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class MaintenanceReportChartViewModel : BaseViewModel
    {
        MaintenanceDataService maintenanceDataService;
        ExportService _exportService;



        public ObservableCollection<string> ViewTypeOptions { get; set; }
        private void ViewTypeOptionsInit()
        {
            ViewTypeOptions = new ObservableCollection<string>
            {
                "Each technician on time",
                "Each room on time",
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
        public MaintenanceReportChartViewModel()
        {
            _scope = App.ServiceProvider.CreateScope(); // GIỮ scope trong ViewModel
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            maintenanceDataService = new MaintenanceDataService(dbContext);
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


        // each technician
        public Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)> eachTechnicianData;
        private async Task FetchEachTechnicianChartDataAsync()
        {
            var data = await maintenanceDataService.GetCountNumbersOfRoomEachTechnicianMaintained(
                StartDate,
                EndDate
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
        Fill = new SolidColorPaint(SKColors.SkyBlue)
    },
    new RowSeries<int>
    {
        Name = "Standard",
        Values = eachTechnicianData.Values.Select(x => x.StandardCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.Orange)
    },
    new RowSeries<int>
    {
        Name = "Suite",
        Values = eachTechnicianData.Values.Select(x => x.SuiteCount).ToArray(),
        Fill = new SolidColorPaint(SKColors.Purple)
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
            var data = await maintenanceDataService.GetCountNumbersOfMaintenanceEachRoom(
                StartDate,
                EndDate
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

            // tạo series cho từng phòng để đổi màu
            var seriesList = new List<ISeries>();

            for (int i = 0; i < roomList.Count; i++)
            {
                var roomName = roomList[i];
                var (roomType, maintenanceCount) = eachRoomData[roomName];

                // xây dựng Values với chỉ duy nhất vị trí i có maintenanceCount, còn lại = 0
                var values = new ObservableValue[roomList.Count];
                for (int j = 0; j < roomList.Count; j++)
                {
                    values[j] = new ObservableValue(j == i ? maintenanceCount : 0);
                }

                var color = roomType switch
                {
                    "Deluxe" => SKColors.Yellow,
                    "Standard" => SKColors.Green,
                    "Suite" => SKColors.Blue,
                    _ => SKColors.Gray
                };

                seriesList.Add(new RowSeries<ObservableValue>
                {
                    Name = $"{roomName} ({roomType})",
                    Values = values,
                    Fill = new SolidColorPaint(color),
                    MaxBarWidth = 20 // cho đẹp hơn, tuỳ chỉnh
                });
            }

            Series = seriesList.ToArray();

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
            LabelsRotation = 0
        }
            };
        }


        // rèfresh chart

        // tránh gọi update data liên tục khi data chưa update xong
        private bool _isLoading = false;
        private async Task RefreshChartAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                switch (SelectedViewType)
                {
                    case "Each technician on time":
                        await FetchEachTechnicianChartDataAsync();
                        UpdateChartEachTechnician();
                        break;
                    case "Each room on time":
                        await FetchEachRoomChartDataAsync();
                        UpdateChartEachRoom();
                        break;
                    default:
                        await FetchEachTechnicianChartDataAsync();
                        UpdateChartEachTechnician();
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
                    default:
                        _exportService.ExportTechnicianActivitiesToExcel(eachTechnicianData, saveFileDialog.FileName);
                        break;
                }
            }
                
        }
    }
}

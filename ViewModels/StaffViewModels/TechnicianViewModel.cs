using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using Microsoft.Win32;
using System.ComponentModel;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class TechnicianViewModel : BaseViewModel
    {
        private readonly IMaintenanceService _maintenanceService;

        private ObservableCollection<MaintenanceReport> _maintenanceReports;
        public ObservableCollection<MaintenanceReport> MaintenanceReports
        {
            get => _maintenanceReports;
            set
            {
                _maintenanceReports = value;
                OnPropertyChanged();
            }
        }

        // Command để cập nhật trạng thái sửa xong
        public ICommand ToggleResolvedCommand { get; }
        public ICommand SelectCompletionImageCommand { get; }

        public TechnicianViewModel(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
            MaintenanceReports = new ObservableCollection<MaintenanceReport>();

            ToggleResolvedCommand = new RelayCommand<MaintenanceReport>(ToggleResolved);
            SelectCompletionImageCommand = new RelayCommand<MaintenanceReport>(SelectCompletionImage);

            LoadMaintenanceReports();
        }

        public TechnicianViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time: mock or empty data
            }
            else
            {
                // Runtime: resolve dependencies as needed
            }
        }

        private async void LoadMaintenanceReports()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting to load maintenance reports...");
                var reports = await _maintenanceService.GetAllReportsAsync();
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {reports.Count} maintenance reports");
                
                MaintenanceReports = new ObservableCollection<MaintenanceReport>(reports);
                
                if (reports.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No maintenance reports found in database");
                    // Không hiển thị MessageBox cho trường hợp không có data, đây là normal
                }
                else
                {
                    foreach (var report in reports)
                    {
                        System.Diagnostics.Debug.WriteLine($"Report: ID={report.Id}, Room={report.RoomNumber}, Description={report.Description}, IsResolved={report.IsResolved}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadMaintenanceReports: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Kiểm tra loại lỗi cụ thể
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                MessageBox.Show($"Lỗi khi hiển thị maintainanceReport: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ToggleResolved(MaintenanceReport report)
        {
            if (report == null)
                return;

            try
            {
                if (!report.IsResolved)
                {
                    // Chuyển từ "Cần sửa chữa" sang "Đã sửa chữa"
                    report.IsResolved = true;
                    report.CompletedDate = DateTime.Now;
                    
                    System.Diagnostics.Debug.WriteLine($"Marking report {report.Id} as resolved at {report.CompletedDate}");
                    
                    await _maintenanceService.UpdateReportAsync(report);
                    
                    // Force UI update bằng cách refresh collection
                    System.Diagnostics.Debug.WriteLine($"Report {report.Id} IsResolved changed to: {report.IsResolved}");
                    
                    MessageBox.Show("Đã đánh dấu báo cáo là hoàn thành sửa chữa.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Đã sửa xong rồi, có thể revert lại nếu cần
                    MessageBox.Show("Báo cáo này đã được đánh dấu là hoàn thành.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ToggleResolved: {ex.Message}");
                MessageBox.Show($"Lỗi khi cập nhật báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectCompletionImage(MaintenanceReport report)
        {
            if (report == null || !report.IsResolved)
                return;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh minh chứng hoàn thành sửa chữa",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Tất cả các tệp (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                report.CompletionImagePath = openFileDialog.FileName;
                System.Diagnostics.Debug.WriteLine($"Selected completion image: {report.CompletionImagePath}");
                
                // Update trong database
                Task.Run(async () =>
                {
                    try
                    {
                        await _maintenanceService.UpdateReportAsync(report);
                        System.Diagnostics.Debug.WriteLine("Updated completion image path in database");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating completion image: {ex.Message}");
                    }
                });
                
                MessageBox.Show("Đã thêm ảnh minh chứng hoàn thành.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

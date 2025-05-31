using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Interfaces;
using HotelManager.Models;

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
        public ICommand ConfirmResolvedCommand { get; }

        public TechnicianViewModel(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
            MaintenanceReports = new ObservableCollection<MaintenanceReport>();

            ConfirmResolvedCommand = new RelayCommand<MaintenanceReport>(MarkAsResolved);

            LoadMaintenanceReports();
        }

        private async void LoadMaintenanceReports()
        {
            try
            {
                var reports = await _maintenanceService.GetAllReportsAsync();
                MaintenanceReports = new ObservableCollection<MaintenanceReport>(reports);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo bảo trì: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MarkAsResolved(MaintenanceReport report)
        {
            if (report == null || report.IsResolved)
                return;

            try
            {
                report.IsResolved = true;
                await _maintenanceService.UpdateReportAsync(report);
                MaintenanceReports.Remove(report); // Loại bỏ báo cáo khỏi danh sách sau khi cập nhật
                MessageBox.Show("Đã sửa chữa phòng xong.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

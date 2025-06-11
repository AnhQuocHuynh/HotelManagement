using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Services;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class CleanerViewModel : BaseViewModel
    {
        private readonly ICleanRoomService _cleanroomService;
        private ObservableCollection<Room> _roomsToClean;
        public ObservableCollection<Room> RoomsToClean
        {
            get => _roomsToClean;
            set { _roomsToClean = value; OnPropertyChanged(); }
        }

        private bool _isReportExpanded;
        public bool IsReportExpanded
        {
            get => _isReportExpanded;
            set
            {
                _isReportExpanded = value;
                OnPropertyChanged();
            }
        }
        private MaintenanceReport _damageReport = new();
        public MaintenanceReport DamageReport
        {
            get => _damageReport;
            set
            {
                _damageReport = value;
                OnPropertyChanged();
            }
        }





        //public DamageReportModel DamageReport { get; set; }

        // Commands
        public ICommand MarkAsCleanedCommand { get; }
        public ICommand ReportIssueCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand SendDamageReportCommand { get; }

        public CleanerViewModel(ICleanRoomService cleanroomService)
        {
            _cleanroomService = cleanroomService;
            RoomsToClean = new ObservableCollection<Room>();
            // Initialize commands
            MarkAsCleanedCommand = new RelayCommand<Room>(MarkRoomAsCleaned);
            ReportIssueCommand = new RelayCommand<Room>(ReportIssue);
            SelectImageCommand = new RelayCommand(SelectImage);
            SendDamageReportCommand = new RelayCommand(SendDamageReport);
            LoadRoomsToClean();
        }
        private async Task LoadRoomsToClean()
        {
            try
            {
                var rooms = await _cleanroomService.GetAllAsync();
                RoomsToClean = new ObservableCollection<Room>(rooms);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách phòng cần dọn: " + ex.Message);
            }
        }
        private async void MarkRoomAsCleaned(Room room)
        {
            if (room == null) return;

            try
            {
                await _cleanroomService.MarkRoomAsCleanedAsync(room);

                // Sau khi cập nhật DB thành công, loại khỏi danh sách
                RoomsToClean.Remove(room);

                MessageBox.Show($"Phòng {room.RoomNumber} đã được đánh dấu là đã dọn.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đánh dấu phòng đã dọn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportIssue(Room room)
        {
            if (room != null)
            {
                IsReportExpanded = true;
                DamageReport = new MaintenanceReport
                {
                    RoomNumber = room.RoomNumber
                };
            }
        }

        private void SelectImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn ảnh báo cáo",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Tất cả các tệp (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                DamageReport.ImagePath = openFileDialog.FileName;
                OnPropertyChanged(nameof(DamageReport)); // Nếu bạn binding nguyên DamageReport
                                                         // hoặc OnPropertyChanged("DamageReport.ImagePath"); nếu binding riêng trường này
            }
        }

        private async void SendDamageReport()
        {
            try
            {
                // Debug information
                System.Diagnostics.Debug.WriteLine($"Attempting to send damage report:");
                System.Diagnostics.Debug.WriteLine($"RoomNumber: '{DamageReport.RoomNumber}'");
                System.Diagnostics.Debug.WriteLine($"Description: '{DamageReport.Description}'");
                System.Diagnostics.Debug.WriteLine($"ImagePath: '{DamageReport.ImagePath}'");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(DamageReport.RoomNumber))
                {
                    MessageBox.Show("Vui lòng nhập số phòng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(DamageReport.Description))
                {
                    MessageBox.Show("Vui lòng nhập mô tả hư hỏng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Validation passed, calling service...");
                
                DamageReport.ReportedDate = DateTime.Now;
                await _cleanroomService.SendDamageReportAsync(DamageReport);
                
                System.Diagnostics.Debug.WriteLine("Service call completed successfully");
                
                MessageBox.Show("Gửi báo cáo thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                DamageReport = new MaintenanceReport(); // Reset form
                IsReportExpanded = false; // Collapse the report section
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendDamageReport: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Gửi báo cáo thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}

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
using HotelManager.Models.Enums;
using HotelManager.Services;
using System.Diagnostics;
using System.ComponentModel;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class CleanerViewModel : BaseViewModel
    {
        private readonly ICleanRoomService _cleanroomService;
        private readonly IService<HotelManager.Models.Room> _roomService;
        private readonly HotelManager.Interfaces.IWorkAssignmentService? _workAssignmentService;
        private ObservableCollection<Room> _roomsToClean;
        public ObservableCollection<Room> RoomsToClean
        {
            get => _roomsToClean;
            set { _roomsToClean = value; OnPropertyChanged(); }
        }

        // Store all rooms for filtering
        private List<Room> _allRooms = new();
        private List<Room> _roomsNeedCleaning = new();

        // Lịch sử báo cáo hư hỏng/phòng đã dọn
        public ObservableCollection<MaintenanceReport> DamageReportHistory { get; set; } = new();

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

        // Cho phép upload nhiều ảnh cho một báo cáo
        public List<string> ImagePaths { get; set; } = new();

        private bool _isNotificationVisible = true;
        public bool IsNotificationVisible
        {
            get => _isNotificationVisible;
            set
            {
                _isNotificationVisible = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand MarkAsCleanedCommand { get; }
        public ICommand ReportIssueCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand SendDamageReportCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand RefreshDamageReportHistoryCommand { get; }
        public ICommand ViewImageCommand { get; }
        public ICommand CloseNotificationCommand { get; }
        public ICommand FilterPendingCommand { get; }
        public ICommand FilterCleanedCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand NavigateProfileCommand { get; set; }

        public CleanerViewModel(ICleanRoomService cleanroomService, IService<HotelManager.Models.Room> roomService)
        {
            _cleanroomService = cleanroomService;
            _roomService = roomService;
            _workAssignmentService = App.ServiceProvider != null
                ? (HotelManager.Interfaces.IWorkAssignmentService?)App.ServiceProvider.GetService(typeof(HotelManager.Interfaces.IWorkAssignmentService))
                : null;
            RoomsToClean = new ObservableCollection<Room>();
            MarkAsCleanedCommand = new RelayCommand<Room>(MarkRoomAsCleaned);
            ReportIssueCommand = new RelayCommand<Room>(ReportIssue);
            SelectImageCommand = new RelayCommand(SelectImage);
            SendDamageReportCommand = new RelayCommand(SendDamageReport);
            RemoveImageCommand = new RelayCommand<string>(RemoveImage);
            RefreshDamageReportHistoryCommand = new RelayCommand(async () => await LoadDamageReportHistoryAsync());
            ViewImageCommand = new RelayCommand<string>(ViewImage);
            CloseNotificationCommand = new RelayCommand(CloseNotification);
            FilterPendingCommand = new RelayCommand(FilterPending);
            FilterCleanedCommand = new RelayCommand(FilterCleaned);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);

            // sequential async initialization to avoid concurrent DbContext operations
            _ = InitializeAsync();
        }

        public CleanerViewModel() : base()
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

        private async Task InitializeAsync()
        {
            await LoadAllRoomsAsync();
            await LoadRoomsToCleanAsync();
            await LoadDamageReportHistoryAsync();
        }

        private async Task LoadAllRoomsAsync()
        {
            try
            {
                var allRooms = await _roomService.GetAllAsync();
                _allRooms = allRooms.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tất cả phòng: " + ex.Message);
            }
        }

        private async Task LoadRoomsToCleanAsync()
        {
            try
            {
                var roomsNeedCleaning = await _cleanroomService.GetAllAsync();
                _roomsNeedCleaning = roomsNeedCleaning;
                
                // Mặc định hiển thị rooms cần dọn dẹp
                RoomsToClean = new ObservableCollection<Room>(roomsNeedCleaning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách phòng cần dọn: " + ex.Message);
            }
        }

        private async void MarkRoomAsCleaned(Room room)
        {
            if (room == null) return;
            // Thêm xác nhận trước khi đánh dấu
            var result = MessageBox.Show($"Bạn có chắc chắn muốn đánh dấu phòng {room.RoomNumber} là đã dọn xong?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                await _cleanroomService.MarkRoomAsCleanedAsync(room);

                // Complete work assignment if exists
                if (_workAssignmentService != null)
                {
                    try
                    {
                        var assignment = await _workAssignmentService.GetActiveAssignmentForRoomAsync(
                            room.RoomNumber, 
                            HotelManager.Models.Enums.AssignmentType.Cleaning);
                        
                        if (assignment != null)
                        {
                            await _workAssignmentService.CompleteAssignmentAsync(assignment.Id, "Room cleaning completed");
                        }
                    }
                    catch (Exception assignEx)
                    {
                        // Log but don't fail the main operation
                        System.Diagnostics.Debug.WriteLine($"Error completing assignment: {assignEx.Message}");
                    }
                }
                
                // Refresh data sau khi mark as cleaned
                await LoadAllRoomsAsync();
                await LoadRoomsToCleanAsync();
                await LoadDamageReportHistoryAsync();
                
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
                ImagePaths = new List<string>();
                OnPropertyChanged(nameof(ImagePaths));
            }
        }

        private void SelectImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn ảnh báo cáo",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Tất cả các tệp (*.*)|*.*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    if (!ImagePaths.Contains(file))
                        ImagePaths.Add(file);
                }
                OnPropertyChanged(nameof(ImagePaths));
            }
        }

        private void RemoveImage(string imagePath)
        {
            if (ImagePaths.Contains(imagePath))
            {
                ImagePaths.Remove(imagePath);
                OnPropertyChanged(nameof(ImagePaths));
            }
        }

        private async void SendDamageReport()
        {
            try
            {
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
                // Lưu nhiều ảnh vào trường ImagePath (dạng chuỗi phân tách hoặc json nếu DB hỗ trợ)
                DamageReport.ImagePath = string.Join(";", ImagePaths);
                DamageReport.ReportedDate = DateTime.Now;
                await _cleanroomService.SendDamageReportAsync(DamageReport);
                MessageBox.Show("Gửi báo cáo thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DamageReport = new MaintenanceReport();
                ImagePaths = new List<string>();
                OnPropertyChanged(nameof(ImagePaths));
                IsReportExpanded = false;
                await LoadDamageReportHistoryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gửi báo cáo thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDamageReportHistoryAsync()
        {
            try
            {
                // Giả sử service có method lấy lịch sử báo cáo cho cleaner hiện tại
                var reports = await _cleanroomService.GetDamageReportsAsync();
                DamageReportHistory = new ObservableCollection<MaintenanceReport>(reports);
                OnPropertyChanged(nameof(DamageReportHistory));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải lịch sử báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewImage(string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                }
                catch
                {
                    MessageBox.Show($"Cannot open image: {imagePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseNotification()
        {
            IsNotificationVisible = false;
        }

        private void FilterPending()
        {
            // Hiển thị rooms cần dọn dẹp (từ CleanRoomService - rooms có booking CheckedOut)
            RoomsToClean = new ObservableCollection<Room>(_roomsNeedCleaning);
        }

        private void FilterCleaned()
        {
            // Hiển thị rooms đã clean (Available status)
            var cleaned = _allRooms.Where(r => r.RoomStatus == RoomStatus.Available);
            RoomsToClean = new ObservableCollection<Room>(cleaned);
        }

        private void ClearFilter()
        {
            // Hiển thị tất cả rooms
            RoomsToClean = new ObservableCollection<Room>(_allRooms);
        }

        private void NavigateProfile()
        {
            _navigationService.NavigateTo<ProfileViewModel>();
        }
    }
}

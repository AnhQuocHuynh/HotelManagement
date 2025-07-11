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
using Timer = System.Timers.Timer;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class CleanerViewModel : BaseViewModel, IDisposable
    {
        private readonly ICleanRoomService _cleanroomService;
        private readonly IService<HotelManager.Models.Room> _roomService;
        private readonly HotelManager.Interfaces.IWorkAssignmentService? _workAssignmentService;
        private readonly INavigationService _navigationService;
        private Timer? _autoRefreshTimer;
        private bool _disposed = false;
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
        public ICommand MarkAsCleanedCommand { get; private set; }
        public ICommand ReportIssueCommand { get; private set; }
        public ICommand SelectImageCommand { get; private set; }
        public ICommand SendDamageReportCommand { get; private set; }
        public ICommand RemoveImageCommand { get; private set; }
        public ICommand RefreshDamageReportHistoryCommand { get; private set; }
        public ICommand ViewImageCommand { get; private set; }
        public ICommand CloseNotificationCommand { get; private set; }
        public ICommand FilterPendingCommand { get; private set; }
        public ICommand FilterCleanedCommand { get; private set; }
        public ICommand ClearFilterCommand { get; private set; }
        public ICommand NavigateProfileCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand NavigateMyScheduleCommand { get; private set; }

        public CleanerViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time: mock or empty data
                RoomsToClean = new ObservableCollection<Room>();
                InitializeCommands();
                return;
            }

            // Runtime: resolve dependencies
            var serviceProvider = App.ServiceProvider;
            if (serviceProvider != null)
            {
                _cleanroomService = App.ServiceProvider.GetRequiredService<ICleanRoomService>();
                _roomService = App.ServiceProvider?.GetRequiredService<IService<Room>>();
                _workAssignmentService = App.ServiceProvider?.GetService<IWorkAssignmentService>();
                _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>();
            }
            else
            {
                throw new InvalidOperationException("ServiceProvider is not available");
            }

            RoomsToClean = new ObservableCollection<Room>();
            InitializeCommands();

            // Proper async initialization with error handling
            InitializeAsyncSafely();
            
            // Setup auto-refresh timer (30 seconds)
            _autoRefreshTimer = new Timer(30000); // 30 seconds
            _autoRefreshTimer.Elapsed += OnAutoRefreshElapsed;
            _autoRefreshTimer.AutoReset = true;
            _autoRefreshTimer.Start();
        }

        private void InitializeCommands()
        {
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
            LogoutCommand = new RelayCommand(Logout);
            LoadedCommand = new AsyncRelayCommand(LoadDataAsync);
            NavigateMyScheduleCommand = new RelayCommand(NavigateMySchedule);
        }

        private async void InitializeAsyncSafely()
        {
            try
            {
                await InitializeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CleanerViewModel initialization failed: {ex.Message}");
                // Log error but don't crash the application
            }
        }

        private async void OnAutoRefreshElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_disposed) return;
            
            try
            {
                await LoadDataAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-refresh failed: {ex.Message}");
                // Continue operation, don't crash
            }
        }

        private async Task InitializeAsync()
        {
            await LoadAllRoomsAsync();
            await LoadRoomsToCleanAsync();
            await LoadDamageReportHistoryAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await LoadAllRoomsAsync();
                await LoadRoomsToCleanAsync();
                await LoadDamageReportHistoryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override async Task OnLoadedAsync()
        {
            await LoadDataAsync();
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

        private void Logout()
        {
            // Stop auto-refresh timer
            Dispose(); // Proper disposal instead of manual timer handling
            
            var mainVM = System.Windows.Application.Current.MainWindow?.DataContext as HotelManager.ViewModels.MainViewModel;
            mainVM?.Logout();
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
                _autoRefreshTimer?.Stop();
                _autoRefreshTimer?.Dispose();
                _autoRefreshTimer = null;
                _disposed = true;
            }
        }

        private void NavigateMySchedule()
        {
            _navigationService.NavigateTo<MyScheduleViewModel>();
        }
    }
}

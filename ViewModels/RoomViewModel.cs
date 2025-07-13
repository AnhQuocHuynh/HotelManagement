using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.ViewModels
{
    public class RoomViewModel : BaseViewModel
    {
        private readonly RoomService roomService;
        private readonly DialogService _dialogService = new DialogService();
        private readonly INavigationService _navigationService;
        private List<Room> _allRooms = new();  // Holds unfiltered data

        public ObservableCollection<Room> _rooms = new();
        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set
            {
                if (_rooms != value)
                {
                    _rooms = value;
                    OnPropertyChanged(nameof(Rooms));
                    OnPropertyChanged(nameof(TotalRooms));
                    OnPropertyChanged(nameof(AvailableRooms));
                    OnPropertyChanged(nameof(OccupiedRooms));
                    OnPropertyChanged(nameof(MaintenanceRooms));
                }
            }
        }

        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand NavigateBackCommand { get; set; }
        public ICommand NavigateProfileCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (_selectedRoom != value)
                {
                    _selectedRoom = value;
                    OnPropertyChanged(nameof(SelectedRoom));
                }
            }
        }

        private string _roomNumber { get; set; } = string.Empty;
        public string RoomNumber
        {
            get => _roomNumber;
            set
            {
                if (_roomNumber != value)
                {
                    _roomNumber = value;
                    OnPropertyChanged(nameof(RoomNumber));
                }
            }
        }

        private RoomStatus _roomStatus { get; set; } = RoomStatus.Available;
        public RoomStatus RoomStatus
        {
            get => _roomStatus; 
            set
            {
                if (_roomStatus != value)
                {
                    _roomStatus = value;
                    OnPropertyChanged(nameof(RoomStatus));                   
                }
            }
        }

        private RoomType _roomType { get; set; } = RoomType.Standard;
        public RoomType RoomType
        {
            get => _roomType; set
            {
                if (_roomType != value)
                {
                    _roomType = value;
                    OnPropertyChanged(nameof(RoomType));
                }
            }
        }
        private decimal _pricePerNight { get; set; } = 0m;
        public decimal PricePerNight
        {
            get => _pricePerNight; set
            {
                if (_pricePerNight != value)
                {
                    _pricePerNight = value;
                    OnPropertyChanged(nameof(PricePerNight));
                }
            }
        }

        private string _roomFilter;
        public string RoomFilter
        {
            get => _roomFilter;
            set
            {
                _roomFilter = value;
                OnPropertyChanged();
                FilterRooms();
            }
        }

        public IEnumerable<RoomStatus> RoomStatusOptions { get; } = Enum.GetValues(typeof(RoomStatus)).Cast<RoomStatus>();
        public IEnumerable<RoomType> RoomTypeOptions { get; } = Enum.GetValues(typeof(RoomType)).Cast<RoomType>();

        public int TotalRooms => Rooms?.Count ?? 0;
        public int AvailableRooms => Rooms?.Count(r => r.RoomStatus == RoomStatus.Available) ?? 0;
        public int OccupiedRooms => Rooms?.Count(r => r.RoomStatus == RoomStatus.Occupied) ?? 0;
        public int MaintenanceRooms => Rooms?.Count(r => r.RoomStatus == RoomStatus.UnderMaintenance) ?? 0;

        public RoomViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Initialize with design-time data
                Rooms = new ObservableCollection<Room>();
                RoomStatusOptions = Enum.GetValues(typeof(RoomStatus)).Cast<RoomStatus>();
                RoomTypeOptions = Enum.GetValues(typeof(RoomType)).Cast<RoomType>();
                
                // Initialize commands with empty implementations for design-time
                UpdateCommand = new RelayCommand<object>(_ => { });
                DeleteCommand = new RelayCommand<object>(_ => { });
                AddCommand = new AsyncRelayCommand(() => Task.CompletedTask);
                SearchCommand = new RelayCommand(() => { });
                LoadedCommand = new RelayCommand(() => { }); // Initialize LoadedCommand for design-time
                NavigateBackCommand = new RelayCommand(() => { });
                NavigateProfileCommand = new RelayCommand(() => { });
                LogoutCommand = new RelayCommand(() => { });
                return;
            }

            // Runtime initialization
            this.roomService = App.ServiceProvider?.GetRequiredService<RoomService>() ?? throw new InvalidOperationException("RoomService not registered");
            this._dialogService = App.ServiceProvider?.GetRequiredService<DialogService>() ?? new DialogService();
            this._navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? throw new InvalidOperationException("INavigationService not registered");
            InitializeViewModel();
        }

        public RoomViewModel(RoomService roomService, DialogService dialogService, INavigationService navigationService)
        {
            this.roomService = roomService;
            this._dialogService = dialogService;
            this._navigationService = navigationService;
            InitializeViewModel();
        }

        public RoomViewModel(RoomService roomService, INavigationService navigationService)
        {
            this.roomService = roomService;
            this._dialogService = App.ServiceProvider?.GetRequiredService<DialogService>() ?? new DialogService();
            this._navigationService = navigationService;
            InitializeViewModel();
        }

        public RoomViewModel(INavigationService navigationService)
        {
            this.roomService = App.ServiceProvider?.GetRequiredService<RoomService>() ?? throw new InvalidOperationException("RoomService not registered");
            this._dialogService = App.ServiceProvider?.GetRequiredService<DialogService>() ?? new DialogService();
            this._navigationService = navigationService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            UpdateCommand = new RelayCommand<object>(_ => UpdateRoom(_selectedRoom));
            DeleteCommand = new RelayCommand<object>(_ => DeleteRoom(_selectedRoom));
            AddCommand = new AsyncRelayCommand(AddRoom);
            SearchCommand = new RelayCommand(FilterRooms);
            LoadedCommand = new RelayCommand(LoadRooms); // Assign LoadRooms to LoadedCommand
            NavigateBackCommand = new RelayCommand(() => _navigationService.NavigateTo<HotelManager.ViewModels.Admin.AdminViewModel>());
            NavigateProfileCommand = new RelayCommand(() => _navigationService.NavigateTo<ProfileViewModel>());
            LogoutCommand = new RelayCommand(() => _navigationService.NavigateTo<HotelManager.ViewModels.Common.LoginViewModel>());
            LoadRooms();
        }

        private async void LoadRooms()
        {
            try
            {
                var rooms = await roomService.GetAllAsync();
                _allRooms = rooms.ToList(); // Lưu trữ dữ liệu chưa lọc
                FilterRooms(); // Lọc dữ liệu ban đầu
            }
            catch (Exception ex)
            {
                // xử lý exceptions (e.g., log them, show a message to the user)
                Console.WriteLine($"Error loading rooms: {ex.Message}");
            }
        }

        private async Task AddRoom()
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(RoomNumber))
                {
                    MessageBox.Show("Vui lòng nhập số phòng!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PricePerNight <= 0)
                {
                    MessageBox.Show("Giá phòng phải lớn hơn 0!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if room number already exists
                var existingRoom = await roomService.GetByRoomNumberAsync(RoomNumber);
                if (existingRoom != null)
                {
                    MessageBox.Show($"Phòng số {RoomNumber} đã tồn tại!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new room
                var newRoom = new Room
                {
                    RoomNumber = RoomNumber,
                    RoomStatus = RoomStatus,
                    RoomType = RoomType,
                    PricePerNight = PricePerNight,
                    Bookings = new List<Booking>(),
                    InvoiceDetails = new List<InvoiceDetail>(),
                    MaintenanceReports = new List<MaintenanceReport>()
                };

                var result = await roomService.CreateAsync(newRoom);
                
                if (result != null)
                {
                    // Refresh the room list
                    LoadRooms();
                    
                    // Clear form
                    ClearForm();
                    
                    MessageBox.Show($"Thêm phòng {RoomNumber} thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Lỗi khi thêm phòng!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm phòng: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteRoom(Room? selectedRoom)
        {
            try
            {
                if (selectedRoom == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng cần xóa!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if room has active bookings
                var hasActiveBookings = selectedRoom.Bookings?.Any(b => 
                    b.Status == BookingStatus.Confirmed || 
                    b.Status == BookingStatus.CheckedIn) ?? false;

                if (hasActiveBookings)
                {
                    MessageBox.Show($"Không thể xóa phòng {selectedRoom.RoomNumber} vì đang có booking hoạt động!", 
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Confirm deletion
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa phòng {selectedRoom.RoomNumber}?\n\nLưu ý: Thao tác này không thể hoàn tác!",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await roomService.DeleteAsync(selectedRoom.RoomNumber);
                    
                    if (success)
                    {
                        // Remove from local collection
                        _allRooms.Remove(selectedRoom);
                        FilterRooms();
                        
                        // Clear selection
                        SelectedRoom = null;
                        
                        MessageBox.Show($"Xóa phòng {selectedRoom.RoomNumber} thành công!", 
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi xóa phòng {selectedRoom.RoomNumber}!", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phòng: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRoom(Room selectedRoom)
        {
            // Logic to update room
            if(selectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editVM = new RoomInfoEditViewModel(selectedRoom, roomService);
            var dialog = _dialogService.ShowDialog(editVM);
            if (dialog == true)
            {
                // Refresh the room list after update
                LoadRooms();
            }
            else
            {
                MessageBox.Show("Cập nhật phòng không thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FilterRooms()
        {
            var filtered = string.IsNullOrWhiteSpace(RoomFilter)
                ? _allRooms
                : _allRooms.Where(r => r.RoomNumber.Contains(RoomFilter, StringComparison.OrdinalIgnoreCase));

            Rooms = new ObservableCollection<Room>(filtered);
            // Đảm bảo cập nhật các property thống kê
            OnPropertyChanged(nameof(TotalRooms));
            OnPropertyChanged(nameof(AvailableRooms));
            OnPropertyChanged(nameof(OccupiedRooms));
            OnPropertyChanged(nameof(MaintenanceRooms));
        }

        private void ClearForm()
        {
            RoomNumber = string.Empty;
            RoomStatus = RoomStatus.Available;
            RoomType = RoomType.Standard;
            PricePerNight = 0m;
            SelectedRoom = null;
        }

        public void RefreshRooms() => LoadRooms();
    }
}

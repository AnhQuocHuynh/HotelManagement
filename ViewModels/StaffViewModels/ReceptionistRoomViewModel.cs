using CommunityToolkit.Mvvm.Input;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using HotelManager.Interfaces;
using HotelManager.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class ReceptionistRoomViewModel : BaseViewModel
    {
        private readonly RoomService _roomService;
        private readonly ILogger<ReceptionistRoomViewModel> _logger;
        private readonly INavigationService _navigationService;

        private ObservableCollection<Room> _rooms;
        private Room _selectedRoom;
        private string _selectedRoomType;
        private string _selectedRoomStatus;
        private string _searchText;
        private List<Room> _allRooms;

        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set { _rooms = value; OnPropertyChanged(nameof(Rooms)); }
        }

        public Room SelectedRoom
        {
            get => _selectedRoom;
            set { _selectedRoom = value; OnPropertyChanged(nameof(SelectedRoom)); }
        }

        public string SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                if (_selectedRoomType != value)
                {
                    _selectedRoomType = value;
                    OnPropertyChanged(nameof(SelectedRoomType));
                    FilterRooms();
                }
            }
        }

        public string SelectedRoomStatus
        {
            get => _selectedRoomStatus;
            set
            {
                if (_selectedRoomStatus != value)
                {
                    _selectedRoomStatus = value;
                    OnPropertyChanged(nameof(SelectedRoomStatus));
                    FilterRooms();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterRooms();
                }
            }
        }

        // Room count properties
        public int AvailableRoomsCount => _allRooms?.Count(r => r.RoomStatus == RoomStatus.Available) ?? 0;
        public int PendingRoomsCount => _allRooms?.Count(r => r.RoomStatus == RoomStatus.Pending) ?? 0;
        public int MaintainingRoomsCount => _allRooms?.Count(r => r.RoomStatus == RoomStatus.UnderMaintenance) ?? 0;

        public ICommand LoadedCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand UpdateRoomStatusCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }
        public ICommand BackToReceptionistViewCommand { get; }
        public ICommand NavigateProfileCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand NavigateMyScheduleCommand { get; private set; }

        public ReceptionistRoomViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Rooms = new ObservableCollection<Room>();
                _allRooms = new List<Room>();
                InitializeViewModel();
                return;
            }

            var roomService = App.ServiceProvider.GetRequiredService<RoomService>();
            var logger = App.ServiceProvider.GetRequiredService<ILogger<ReceptionistRoomViewModel>>();
            _roomService = roomService;
            _logger = logger;
            _allRooms = new List<Room>();

            InitializeViewModel();

            BackToReceptionistViewCommand = new RelayCommand(() => _navigationService?.NavigateTo<ReceptionistViewModel>());
            NavigateProfileCommand = new RelayCommand(() => _navigationService?.NavigateTo<ProfileViewModel>());
            LogoutCommand = new RelayCommand(Logout);
        }

        private void InitializeViewModel()
        {
            Rooms = new ObservableCollection<Room>();

            LoadedCommand = new AsyncRelayCommand(LoadDataAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            UpdateRoomStatusCommand = new AsyncRelayCommand<Room>(UpdateRoomStatusAsync);
            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            NavigateMyScheduleCommand = new RelayCommand(NavigateMySchedule);
        }

        protected override async Task OnLoadedAsync()
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                _logger?.LogInformation("Loading room data for receptionist");
                var rooms = await _roomService.GetAllAsync();
                _allRooms = rooms.ToList();
                await FilterRoomsAsync();
                _logger?.LogInformation("Successfully loaded {RoomCount} rooms", rooms.Count());
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error loading room data");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error loading room data");
                MessageBox.Show($"Lỗi khi tải dữ liệu phòng. {ex.Message}\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        private async Task UpdateRoomStatusAsync(Room room)
        {
            if (room == null) return;

            try
            {
                _logger?.LogInformation("Updating room status for room {RoomNumber}", room.RoomNumber);
                await _roomService.UpdateAsync(room);
                _logger?.LogInformation("Successfully updated room status for room {RoomNumber}", room.RoomNumber);
                MessageBox.Show("Cập nhật trạng thái phòng thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error updating room status");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error updating room status");
                MessageBox.Show("Lỗi khi cập nhật trạng thái phòng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FilterRoomsAsync()
        {
            try
            {
                if (_allRooms == null) return;

                var filteredRooms = _allRooms.AsEnumerable();

                // Filter by room type
                if (!string.IsNullOrEmpty(SelectedRoomType) && SelectedRoomType != "All Types")
                {
                    if (Enum.TryParse<RoomType>(SelectedRoomType, out var roomType))
                    {
                        filteredRooms = filteredRooms.Where(r => r.RoomType == roomType);
                    }
                }

                // Filter by room status
                if (!string.IsNullOrEmpty(SelectedRoomStatus) && SelectedRoomStatus != "All Status")
                {
                    if (Enum.TryParse<RoomStatus>(SelectedRoomStatus, out var roomStatus))
                    {
                        filteredRooms = filteredRooms.Where(r => r.RoomStatus == roomStatus);
                    }
                }

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredRooms = filteredRooms.Where(r => 
                        r.RoomNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                Rooms.Clear();
                foreach (var room in filteredRooms)
                {
                    Rooms.Add(room);
                }

                // Debug: Hiển thị số lượng phòng sau filter
                MessageBox.Show($"Số lượng phòng sau filter: {Rooms.Count}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                // Update count properties
                OnPropertyChanged(nameof(AvailableRoomsCount));
                OnPropertyChanged(nameof(PendingRoomsCount));
                OnPropertyChanged(nameof(MaintainingRoomsCount));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error filtering rooms");
                MessageBox.Show("Lỗi khi lọc danh sách phòng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClearFiltersAsync()
        {
            try
            {
                SelectedRoomType = null;
                SelectedRoomStatus = null;
                SearchText = string.Empty;
                await FilterRoomsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error clearing filters");
                MessageBox.Show("Lỗi khi xóa bộ lọc. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            var mainVM = App.Current.MainWindow.DataContext as HotelManager.ViewModels.MainViewModel;
            if (mainVM != null)
                mainVM.Logout();
        }

        private void FilterRooms()
        {
            try
            {
                if (_allRooms == null) return;

                var filteredRooms = _allRooms.AsEnumerable();

                // Filter by room type
                if (!string.IsNullOrEmpty(SelectedRoomType) && SelectedRoomType != "All Types")
                {
                    if (Enum.TryParse<RoomType>(SelectedRoomType, out var roomType))
                    {
                        filteredRooms = filteredRooms.Where(r => r.RoomType == roomType);
                    }
                }

                // Filter by room status
                if (!string.IsNullOrEmpty(SelectedRoomStatus) && SelectedRoomStatus != "All Status")
                {
                    if (Enum.TryParse<RoomStatus>(SelectedRoomStatus, out var roomStatus))
                    {
                        filteredRooms = filteredRooms.Where(r => r.RoomStatus == roomStatus);
                    }
                }

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredRooms = filteredRooms.Where(r => 
                        r.RoomNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                Rooms.Clear();
                foreach (var room in filteredRooms)
                {
                    Rooms.Add(room);
                }

                // Update count properties
                OnPropertyChanged(nameof(AvailableRoomsCount));
                OnPropertyChanged(nameof(PendingRoomsCount));
                OnPropertyChanged(nameof(MaintainingRoomsCount));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error filtering rooms");
                MessageBox.Show("Lỗi khi lọc danh sách phòng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateMySchedule()
        {
            _navigationService.NavigateTo<MyScheduleViewModel>();
        }
    }
} 
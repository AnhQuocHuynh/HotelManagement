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

        private ObservableCollection<Room> _rooms;
        private Room _selectedRoom;
        private RoomType _selectedRoomType;
        private RoomStatus _selectedRoomStatus;
        private string _searchText;

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

        public RoomType SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                _selectedRoomType = value;
                OnPropertyChanged(nameof(SelectedRoomType));
                _ = FilterRoomsAsync();
            }
        }

        public RoomStatus SelectedRoomStatus
        {
            get => _selectedRoomStatus;
            set
            {
                _selectedRoomStatus = value;
                OnPropertyChanged(nameof(SelectedRoomStatus));
                _ = FilterRoomsAsync();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _ = FilterRoomsAsync();
            }
        }

        public ICommand LoadedCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand UpdateRoomStatusCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }

        public ReceptionistRoomViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Rooms = new ObservableCollection<Room>();
                InitializeViewModel();
                return;
            }

            var roomService = App.ServiceProvider.GetRequiredService<RoomService>();
            var logger = App.ServiceProvider.GetRequiredService<ILogger<ReceptionistRoomViewModel>>();
            _roomService = roomService;
            _logger = logger;

            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Rooms = new ObservableCollection<Room>();

            LoadedCommand = new AsyncRelayCommand(LoadDataAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            UpdateRoomStatusCommand = new AsyncRelayCommand<Room>(UpdateRoomStatusAsync);
            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
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
                Rooms.Clear();
                foreach (var room in rooms)
                {
                    Rooms.Add(room);
                }
                _logger?.LogInformation("Successfully loaded {RoomCount} rooms", rooms.Count);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error loading room data");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error loading room data");
                MessageBox.Show("Lỗi khi tải dữ liệu phòng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var allRooms = await _roomService.GetAllAsync();
                var filteredRooms = allRooms.AsEnumerable();

                // Filter by room type
                if (SelectedRoomType != RoomType.None)
                {
                    filteredRooms = filteredRooms.Where(r => r.Type == SelectedRoomType);
                }

                // Filter by room status
                if (SelectedRoomStatus != RoomStatus.None)
                {
                    filteredRooms = filteredRooms.Where(r => r.Status == SelectedRoomStatus);
                }

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredRooms = filteredRooms.Where(r => 
                        r.RoomNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        r.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);
                }

                Rooms.Clear();
                foreach (var room in filteredRooms)
                {
                    Rooms.Add(room);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error filtering rooms");
                MessageBox.Show("Lỗi khi lọc danh sách phòng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClearFiltersAsync()
        {
            SelectedRoomType = RoomType.None;
            SelectedRoomStatus = RoomStatus.None;
            SearchText = string.Empty;
            await LoadDataAsync();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using RelayCommand = HotelManager.Utilities.RelayCommand;

namespace HotelManager.ViewModels
{
    internal class RoomViewModel : BaseViewModel
    {
        private readonly RoomService roomService;
        private readonly DialogService _dialogService = new DialogService();
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
                }
            }
        }

        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }

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

        

        public RoomViewModel()
        {
            this.roomService = new RoomService(new HotelDbContext());
            UpdateCommand = new RelayCommand(param => UpdateRoom(_selectedRoom));
            DeleteCommand = new RelayCommand(param => DeleteRoom(_selectedRoom));
            AddCommand = new RelayCommand(async param => AddRoom());
            LoadRooms();
        }

        public RoomViewModel(IService<Room> roomService)
        {
            this.roomService = (RoomService)roomService;
            UpdateCommand = new RelayCommand(param => UpdateRoom(_selectedRoom));
            DeleteCommand = new RelayCommand(param => DeleteRoom(_selectedRoom));
            AddCommand = new RelayCommand(async param => await AddRoom());
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
            throw new NotImplementedException();
        }

        private void DeleteRoom(Room? selectedRoom)
        {
            throw new NotImplementedException();
        }

        private void UpdateRoom(Room selectedRoom)
        {
            // Logic to update room
            if(selectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editVM = new RoomInfoEditViewModel(selectedRoom);
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
        }
    }
}

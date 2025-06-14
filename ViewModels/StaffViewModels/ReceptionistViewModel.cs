using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
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

namespace HotelManager.ViewModels.StaffViewModels
{
    public class ReceptionistViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private readonly RoomService _roomService;

        private string _customerFullName;
        private string _customerCCCD;
        private string _customerPhoneNumber;
        private CustomerType _selectedCustomerType;
        private RoomType _selectedRoomType;
        private string _selectedRoomNumber;
        private DateTime? _checkInDate;
        private DateTime? _checkOutDate;
        private BookingStatus _selectedStatus;
        private ObservableCollection<Booking> _bookings;
        private ObservableCollection<string> _availableRooms;

        public string CustomerFullName
        {
            get => _customerFullName;
            set { _customerFullName = value; OnPropertyChanged(nameof(CustomerFullName)); }
        }

        public string CustomerCCCD
        {
            get => _customerCCCD;
            set { _customerCCCD = value; OnPropertyChanged(nameof(CustomerCCCD)); }
        }

        public string CustomerPhoneNumber
        {
            get => _customerPhoneNumber;
            set { _customerPhoneNumber = value; OnPropertyChanged(nameof(CustomerPhoneNumber)); }
        }

        public CustomerType SelectedCustomerType
        {
            get => _selectedCustomerType;
            set { _selectedCustomerType = value; OnPropertyChanged(nameof(SelectedCustomerType)); }
        }

        public RoomType SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                _selectedRoomType = value;
                OnPropertyChanged(nameof(SelectedRoomType));
                // Gọi trực tiếp để đảm bảo chạy trên thread UI
                UpdateAvailableRoomsAsync().ConfigureAwait(false);
            }
        }

        public string SelectedRoomNumber
        {
            get => _selectedRoomNumber;
            set { _selectedRoomNumber = value; OnPropertyChanged(nameof(SelectedRoomNumber)); }
        }

        public DateTime? CheckInDate
        {
            get => _checkInDate;
            set { _checkInDate = value; OnPropertyChanged(nameof(CheckInDate)); }
        }

        public DateTime? CheckOutDate
        {
            get => _checkOutDate;
            set { _checkOutDate = value; OnPropertyChanged(nameof(CheckOutDate)); }
        }

        public BookingStatus SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        public ObservableCollection<Booking> Bookings
        {
            get => _bookings;
            set { _bookings = value; OnPropertyChanged(nameof(Bookings)); }
        }

        public ObservableCollection<string> AvailableRooms
        {
            get => _availableRooms;
            set { _availableRooms = value; OnPropertyChanged(nameof(AvailableRooms)); }
        }

        public ICommand AddNewBookingCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public ReceptionistViewModel()
        {
            var dbContext = new HotelDbContext();
            _bookingService = new BookingService(dbContext, new CustomerService(dbContext));
            _roomService = new RoomService(dbContext);

            Bookings = new ObservableCollection<Booking>();
            AvailableRooms = new ObservableCollection<string>();

            AddNewBookingCommand = new RelayCommand(async () => await AddNewBookingAsync());
            UpdateCommand = new RelayCommand<Booking>(async booking => await UpdateAsync(booking));
            DeleteCommand = new RelayCommand<Booking>(async booking => await DeleteAsync(booking));
        }

        public async Task LoadDataAsync()
        {
            try
            {
                Debug.WriteLine("ReceptionistViewModel: LoadDataAsync started");
                var bookings = await _bookingService.GetAllAsync();
                Bookings.Clear();
                foreach (var booking in bookings)
                {
                    Bookings.Add(booking);
                }

                await UpdateAvailableRoomsAsync();
                Debug.WriteLine("ReceptionistViewModel: LoadDataAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReceptionistViewModel: LoadDataAsync error - {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddNewBookingAsync()
        {
            try
            {
                Debug.WriteLine("ReceptionistViewModel: AddNewBookingAsync started");
                if (string.IsNullOrWhiteSpace(CustomerFullName) ||
                    string.IsNullOrWhiteSpace(CustomerCCCD) ||
                    string.IsNullOrWhiteSpace(CustomerPhoneNumber) ||
                    SelectedCustomerType == default ||
                    SelectedRoomType == default ||
                    string.IsNullOrWhiteSpace(SelectedRoomNumber) ||
                    CheckInDate == null ||
                    CheckOutDate == null ||
                    SelectedStatus == default)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var customer = new Customer
                {
                    FullName = CustomerFullName,
                    CCCD = CustomerCCCD,
                    PhoneNumber = CustomerPhoneNumber,
                    Type = SelectedCustomerType
                };

                var booking = new Booking
                {
                    Customer = customer,
                    RoomType = SelectedRoomType,
                    RoomNumber = SelectedRoomNumber,
                    CheckInDate = CheckInDate.Value,
                    CheckOutDate = CheckOutDate.Value,
                    Status = SelectedStatus
                };

                await _bookingService.CreateAsync(booking);
                Bookings.Add(booking);
                await UpdateAvailableRoomsAsync();

                ClearInputFields();
                MessageBox.Show("Đã thêm booking thành công!", "Thành công", MessageBoxButton.OK);
                Debug.WriteLine("ReceptionistViewModel: AddNewBookingAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReceptionistViewModel: AddNewBookingAsync error - {ex.Message}");
                MessageBox.Show($"Lỗi khi thêm booking: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync(Booking booking)
        {
            try
            {
                Debug.WriteLine("ReceptionistViewModel: UpdateAsync started");
                if (booking == null)
                {
                    MessageBox.Show("Vui lòng chọn một booking để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _bookingService.UpdateAsync(booking);
                await UpdateAvailableRoomsAsync();
                MessageBox.Show("Cập nhật booking thành công!", "Thành công", MessageBoxButton.OK);
                Debug.WriteLine("ReceptionistViewModel: UpdateAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReceptionistViewModel: UpdateAsync error - {ex.Message}");
                MessageBox.Show($"Lỗi khi cập nhật booking: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync(Booking booking)
        {
            try
            {
                Debug.WriteLine("ReceptionistViewModel: DeleteAsync started");
                if (booking == null)
                {
                    MessageBox.Show("Vui lòng chọn một booking để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa booking này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await _bookingService.DeleteAsync(booking.Id);
                    Bookings.Remove(booking);
                    await UpdateAvailableRoomsAsync();
                    MessageBox.Show("Xóa booking thành công!", "Thành công", MessageBoxButton.OK);
                    Debug.WriteLine("ReceptionistViewModel: DeleteAsync completed");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReceptionistViewModel: UpdateAsync error - {ex.Message}");
                MessageBox.Show($"Lỗi khi xóa booking: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAvailableRoomsAsync()
        {
            try
            {
                Debug.WriteLine("ReceptionistViewModel: UpdateAvailableRoomsAsync started");
                if (SelectedRoomType != default)
                {
                    var rooms = await _roomService.GetAvailableRoomsByTypeAsync(SelectedRoomType);
                    // Sử dụng Dispatcher để đảm bảo thay đổi trên thread UI
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        AvailableRooms.Clear();
                        foreach (var room in rooms)
                        {
                            AvailableRooms.Add(room.RoomNumber);
                        }
                    });
                }
                Debug.WriteLine("ReceptionistViewModel: UpdateAvailableRoomsAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReceptionistViewModel: UpdateAvailableRoomsAsync error - {ex.Message}");
                MessageBox.Show($"Lỗi khi cập nhật danh sách phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearInputFields()
        {
            CustomerFullName = string.Empty;
            CustomerCCCD = string.Empty;
            CustomerPhoneNumber = string.Empty;
            SelectedCustomerType = default;
            SelectedRoomType = default;
            SelectedRoomNumber = null;
            CheckInDate = null;
            CheckOutDate = null;
            SelectedStatus = default;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
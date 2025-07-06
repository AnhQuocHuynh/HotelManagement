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
    public class ReceptionistViewModel : BaseViewModel
    {
        private readonly BookingService _bookingService;
        private readonly RoomService _roomService;
        private readonly ILogger<ReceptionistViewModel> _logger;

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
                // Gọi trực tiếp để đảm bảo chạy trên thread UI (fire-and-forget)
                _ = UpdateAvailableRoomsAsync();
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

        public ICommand AddNewBookingCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand ReportsCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }

        // Constructor cho XAML (không tham số) – tự resolve qua DI
        public ReceptionistViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time: khởi tạo dữ liệu mẫu hoặc để trống
                Bookings = new ObservableCollection<Booking>();
                AvailableRooms = new ObservableCollection<string>();
                InitializeViewModel();
                return;
            }

            // Runtime: resolve qua DI
            var bookingService = App.ServiceProvider.GetRequiredService<BookingService>();
            var roomService = App.ServiceProvider.GetRequiredService<RoomService>();
            var logger = App.ServiceProvider.GetRequiredService<ILogger<ReceptionistViewModel>>();
            _bookingService = bookingService;
            _roomService = roomService;
            _logger = logger;

            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Bookings = new ObservableCollection<Booking>();
            AvailableRooms = new ObservableCollection<string>();

            AddNewBookingCommand = new AsyncRelayCommand(
                execute: () => AddNewBookingAsync(),
                canExecute: () => true);

            UpdateCommand = new AsyncRelayCommand<Booking?>(
                execute: b => UpdateAsync(b!),
                canExecute: b => b != null);

            DeleteCommand = new AsyncRelayCommand<Booking?>(
                execute: b => DeleteAsync(b!),
                canExecute: b => b != null);

            LoadedCommand = new AsyncRelayCommand(LoadDataAsync);

            ReportsCommand = new AsyncRelayCommand(ReportsAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            ClearFormCommand = new AsyncRelayCommand(ClearFormAsync);
        }

        protected override async Task OnLoadedAsync()
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                _logger?.LogInformation("Loading receptionist data");
                var bookings = await _bookingService.GetAllAsync();
                Bookings.Clear();
                foreach (var booking in bookings)
                {
                    Bookings.Add(booking);
                }

                await UpdateAvailableRoomsAsync();
                _logger?.LogInformation("Successfully loaded {BookingCount} bookings", bookings.Count);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error loading data");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error loading data");
                MessageBox.Show("Lỗi khi tải dữ liệu. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddNewBookingAsync()
        {
            try
            {
                _logger?.LogInformation("Adding new booking");
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
                _logger?.LogInformation("Successfully created booking for customer {CustomerName} in room {RoomNumber}", customer.FullName, booking.RoomNumber);
            }
            catch (DuplicateEntityException ex)
            {
                _logger?.LogWarning(ex, "Duplicate entity detected");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error creating booking");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error creating booking");
                MessageBox.Show("Lỗi khi thêm booking. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Updating booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Vui lòng chọn một booking để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _bookingService.UpdateAsync(booking);
                await UpdateAvailableRoomsAsync();
                MessageBox.Show("Cập nhật booking thành công!", "Thành công", MessageBoxButton.OK);
                _logger?.LogInformation("Successfully updated booking {BookingId}", booking.Id);
            }
            catch (EntityNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Entity not found");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error updating booking");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error updating booking");
                MessageBox.Show("Lỗi khi cập nhật booking. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Deleting booking {BookingId}", booking?.Id);
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
                    _logger?.LogInformation("Successfully deleted booking {BookingId}", booking.Id);
                }
            }
            catch (EntityNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Entity not found");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error deleting booking");
                MessageBox.Show(ex.UserMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error deleting booking");
                MessageBox.Show("Lỗi khi xóa booking. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAvailableRoomsAsync()
        {
            try
            {
                _logger?.LogDebug("Updating available rooms for type {RoomType}", SelectedRoomType);
                if (SelectedRoomType != default)
                {
                    var rooms = await _roomService.GetAvailableRoomsByTypeAsync(SelectedRoomType);
                    var roomNumbers = rooms.Select(r => r.RoomNumber).ToList();
                    var roomCount = roomNumbers.Count;

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        AvailableRooms = new ObservableCollection<string>(roomNumbers);
                    }));

                    _logger?.LogDebug("Found {RoomCount} available rooms", roomCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating available rooms");
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
            SelectedStatus = BookingStatus.Pending;
        }

        private async Task ReportsAsync()
        {
            try
            {
                var message = "Reports feature is available in Manager role.\n\nAs a Receptionist, you have access to:\n• View current bookings\n• Add new bookings\n• Update booking status\n• Check room availability";
                MessageBox.Show(message, "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing reports message");
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                await LoadDataAsync();
                MessageBox.Show("Data refreshed successfully!", "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error refreshing data");
                MessageBox.Show("Error refreshing data. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClearFormAsync()
        {
            try
            {
                ClearInputFields();
                MessageBox.Show("Form cleared successfully!", "Clear Form", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error clearing form");
            }
        }
    }
}
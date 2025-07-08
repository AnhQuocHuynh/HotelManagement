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
using HotelManager.Utilities;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class ReceptionistViewModel : BaseViewModel
    {
        private readonly BookingService _bookingService;
        private readonly RoomService _roomService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
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
        private List<Booking> _allBookings;
        private int _totalBookings;
        private int _availableRoomsCount;
        private ObservableCollection<string> _availableRooms;
        private string _searchText;

        // Properties for editing
        private Booking _selectedBookingForEdit;
        public Booking SelectedBookingForEdit
        {
            get => _selectedBookingForEdit;
            set
            {
                _selectedBookingForEdit = value;
                OnPropertyChanged(nameof(SelectedBookingForEdit));
                if (value != null)
                {
                    LoadBookingForEdit(value);
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(IsAddMode));
            }
        }

        public bool IsAddMode => !IsEditMode;

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

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public int TotalBookings
        {
            get => _totalBookings;
            set { _totalBookings = value; OnPropertyChanged(nameof(TotalBookings)); }
        }

        public int AvailableRoomsCount
        {
            get => _availableRoomsCount;
            set { _availableRoomsCount = value; OnPropertyChanged(nameof(AvailableRoomsCount)); }
        }

        public ICommand AddNewBookingCommand { get; private set; }
        public ICommand ShowBookingsCommand { get; private set; }
        public ICommand ShowAvailableRoomsCommand { get; private set; }
        public ICommand NavigateToRoomViewCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand EditBookingCommand { get; private set; }
        public ICommand SaveEditCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand DeleteBookingCommand { get; private set; }
        public ICommand CheckoutCommand { get; private set; }
        public ICommand CreateInvoiceCommand { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand ReportsCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand NavigateProfileCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

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
            var navigationService = App.ServiceProvider.GetRequiredService<INavigationService>();
            var notificationService = App.ServiceProvider.GetRequiredService<INotificationService>();
            var logger = App.ServiceProvider.GetRequiredService<ILogger<ReceptionistViewModel>>();
            _bookingService = bookingService;
            _roomService = roomService;
            _navigationService = navigationService;
            _notificationService = notificationService;
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
            ShowBookingsCommand = new RelayCommand(ShowBookings);
            ShowAvailableRoomsCommand = new RelayCommand(ShowAvailableRooms);
            NavigateToRoomViewCommand = new RelayCommand(NavigateToRoomView);

            UpdateCommand = new AsyncRelayCommand<Booking?>(
                execute: b => UpdateAsync(b!),
                canExecute: b => b != null);

            EditBookingCommand = new AsyncRelayCommand<Booking?>(
                execute: b => EditBookingAsync(b!),
                canExecute: b => b != null);

            SaveEditCommand = new AsyncRelayCommand<Booking?>(
                execute: b => SaveEditAsync(b!),
                canExecute: b => b != null);

            CancelEditCommand = new AsyncRelayCommand<Booking?>(
                execute: b => CancelEditAsync(b!),
                canExecute: b => b != null);

            DeleteBookingCommand = new AsyncRelayCommand<Booking?>(
                execute: b => DeleteAsync(b!),
                canExecute: b => b != null);

            CheckoutCommand = new AsyncRelayCommand<Booking?>(
                execute: b => CheckoutAsync(b!),
                canExecute: b => b != null && b.Status == BookingStatus.CheckedIn);

            CreateInvoiceCommand = new AsyncRelayCommand<Booking?>(
                execute: b => CreateInvoiceAsync(b!),
                canExecute: b => b != null);

            LoadedCommand = new AsyncRelayCommand(LoadDataAsync);

            ReportsCommand = new AsyncRelayCommand(ReportsAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            ClearFormCommand = new AsyncRelayCommand(ClearFormAsync);
            SearchCommand = new RelayCommand(PerformSearch);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);
            LogoutCommand = new RelayCommand(Logout);
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
                _allBookings = bookings.ToList();
                foreach (var booking in _allBookings)
                {
                    Bookings.Add(booking);
                }
                TotalBookings = Bookings.Count;

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
                    string.IsNullOrWhiteSpace(SelectedRoomNumber) ||
                    CheckInDate == null ||
                    CheckOutDate == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation cho ngày tháng
                if (CheckInDate.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("Check-in date cannot be in the past.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CheckOutDate.Value.Date <= CheckInDate.Value.Date)
                {
                    MessageBox.Show("Check-out date must be after check-in date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra xem phòng có available trong khoảng thời gian này không
                var conflictingBookings = await _bookingService.GetConflictingBookingsAsync(SelectedRoomNumber, CheckInDate.Value, CheckOutDate.Value);
                if (conflictingBookings.Any())
                {
                    MessageBox.Show($"Room {SelectedRoomNumber} is not available for the selected dates. Please choose different dates or another room.", 
                        "Room Not Available", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                TotalBookings = Bookings.Count;

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

                MessageBox.Show("Error while creating booking. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditBookingAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Editing booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Please select a booking to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedBookingForEdit = booking;
                IsEditMode = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error editing booking");
                MessageBox.Show("Error editing booking. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveEditAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Saving edited booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Please select a booking to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _bookingService.UpdateAsync(booking);
                await UpdateAvailableRoomsAsync();
                MessageBox.Show("Booking updated successfully!", "Success", MessageBoxButton.OK);
                _logger?.LogInformation("Successfully updated booking {BookingId}", booking.Id);
            }
            catch (EntityNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Entity not found");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error updating booking");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error updating booking");
                MessageBox.Show("Error while updating booking. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CancelEditAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Canceling edit for booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Please select a booking to cancel edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedBookingForEdit = null;
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error canceling edit");
                MessageBox.Show("Error canceling edit. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    TotalBookings = Bookings.Count;
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
                _logger?.LogDebug("Updating available rooms for type: {RoomType}", SelectedRoomType);

                if (!Enum.IsDefined(typeof(RoomType), SelectedRoomType))
                {
                    AvailableRooms.Clear();
                    AvailableRoomsCount = 0;
                    return;
                }

                var availableRooms = await _roomService.GetAvailableRoomsByTypeAsync(SelectedRoomType);

                var roomNumbers = availableRooms
                    .Select(r => r.RoomNumber)
                    .OrderBy(n => n)
                    .ToList();

                AvailableRooms = new ObservableCollection<string>(roomNumbers);
                AvailableRoomsCount = roomNumbers.Count;

                _logger?.LogInformation("Found {RoomCount} available rooms of type {RoomType}", roomNumbers.Count, SelectedRoomType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating available rooms.");
                MessageBox.Show("Lỗi khi tải danh sách phòng trống. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task CheckoutAsync(Booking booking)
        {
            try
            {
                _logger?.LogInformation("Processing checkout for booking {BookingId}", booking.Id);
                var result = MessageBox.Show(
                    $"Confirm checkout for customer {booking.Customer?.FullName} from room {booking.RoomNumber}?\n\n" +
                    "This will:\n• Navigate to Payment View for processing\n• Update room status to Pending (awaiting cleaning)",
                    "Confirm Checkout",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;
                booking.Status = BookingStatus.CheckedOut;
                booking.CheckOutDate = DateTime.Now;
                await _bookingService.UpdateAsync(booking);
                var room = await _roomService.GetByRoomNumberAsync(booking.RoomNumber);
                if (room != null)
                {
                    room.RoomStatus = RoomStatus.Pending;
                    await _roomService.UpdateAsync(room);
                    try
                    {
                        var workAssignmentService = App.ServiceProvider?.GetRequiredService<HotelManager.Interfaces.IWorkAssignmentService>();
                        if (workAssignmentService != null)
                        {
                            var currentUser = AppSession.GetCurrentUserAccount();
                            await workAssignmentService.AutoAssignWorkAsync(
                                room.RoomNumber,
                                HotelManager.Models.Enums.AssignmentType.Cleaning,
                                currentUser?.EmployeeId);
                        }
                    }
                    catch { }
                }
                await LoadDataAsync();
                // Tạo mới invoice cho booking
                var invoiceService = App.ServiceProvider.GetRequiredService<HotelManager.Services.InvoiceService>();
                var invoice = await invoiceService.CreateForBookingAsync(booking.Id, booking.TotalAmount);
                // Điều hướng sang PaymentViewModel, truyền invoice
                _navigationService?.NavigateTo<PaymentViewModel>(invoice);
                _notificationService?.ShowSuccess($"Checkout completed for room {booking.RoomNumber}. Navigated to Payment View.");
                _logger?.LogInformation("Successfully processed checkout for booking {BookingId}", booking.Id);
            }
            catch (EntityNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Entity not found during checkout");
                _notificationService?.ShowError(ex.UserMessage);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error during checkout");
                _notificationService?.ShowError(ex.UserMessage);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during checkout");
                _notificationService?.ShowError("Error during checkout. Please try again.");
            }
        }

        private void ShowBookings()
        {
            try
            {
                // Hiển thị danh sách bookings trong một MessageBox hoặc một cửa sổ mới
                var bookingsList = string.Join(Environment.NewLine, Bookings.Select(b => $"{b.Customer.FullName} - {b.RoomNumber} ({b.Status})"));
                MessageBox.Show(bookingsList, "Danh sách Booking", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing bookings");
                MessageBox.Show("Lỗi khi hiển thị danh sách booking. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAvailableRooms()
        {
            try
            {
                if (AvailableRooms.Count == 0)
                {
                    MessageBox.Show("Không có phòng nào khả dụng cho loại phòng đã chọn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var roomsList = string.Join(Environment.NewLine, AvailableRooms);
                MessageBox.Show(roomsList, "Danh sách Phòng Khả Dụng", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing available rooms");
                MessageBox.Show("Lỗi khi hiển thị danh sách phòng khả dụng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToRoomView()
        {
            _navigationService?.NavigateTo<ReceptionistRoomViewModel>();
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Bookings.Clear();
                foreach (var booking in _allBookings)
                    Bookings.Add(booking);
                return;
            }

            string searchText = SearchText.Trim().ToLowerInvariant();
            var filteredBookings = _allBookings
            .Where(b =>
                b.Customer.FullName.ToLowerInvariant().Contains(searchText) ||
                b.Customer.CCCD.ToLowerInvariant().Contains(searchText) ||
                b.Customer.PhoneNumber.Contains(searchText) ||
                b.RoomNumber.ToLowerInvariant().Contains(searchText) ||
                b.Status.ToString().ToLowerInvariant().Contains(searchText) ||
                b.Customer.Type.ToString().ToLowerInvariant().Contains(searchText) ||
                b.RoomType.ToString().ToLowerInvariant().Contains(searchText)
            )
            .ToList();

            Bookings.Clear();
            foreach (var booking in filteredBookings)
                Bookings.Add(booking);
        }

        private void NavigateProfile()
        {
            _navigationService?.NavigateTo<ProfileViewModel>();
        }

        private void Logout()
        {
            var mainVM = System.Windows.Application.Current.MainWindow?.DataContext as HotelManager.ViewModels.MainViewModel;
            mainVM?.Logout();
        }

        private void LoadBookingForEdit(Booking booking)
        {
            if (booking?.Customer != null)
            {
                CustomerFullName = booking.Customer.FullName;
                CustomerCCCD = booking.Customer.CCCD;
                CustomerPhoneNumber = booking.Customer.PhoneNumber;
                SelectedCustomerType = booking.Customer.Type;
            }
            
            SelectedRoomType = booking.RoomType;
            SelectedRoomNumber = booking.RoomNumber;
            CheckInDate = booking.CheckInDate;
            CheckOutDate = booking.CheckOutDate;
            SelectedStatus = booking.Status;
        }

        private async Task CreateInvoiceAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Creating invoice for booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Please select a booking to create invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var invoiceService = App.ServiceProvider.GetRequiredService<HotelManager.Services.InvoiceService>();
                var invoice = await invoiceService.CreateForBookingAsync(booking.Id, booking.TotalAmount);
                _navigationService?.NavigateTo<PaymentViewModel>(invoice);
                _notificationService?.ShowSuccess($"Invoice created for booking {booking.RoomNumber}. Navigated to Payment View.");
                _logger?.LogInformation("Successfully created invoice for booking {BookingId}", booking.Id);
            }
            catch (EntityNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Entity not found");
                _notificationService?.ShowError(ex.UserMessage);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error creating invoice");
                _notificationService?.ShowError(ex.UserMessage);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error creating invoice");
                _notificationService?.ShowError("Error creating invoice. Please try again.");
            }
        }
    }
}
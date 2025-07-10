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
        //private Booking? _selectedBooking;

        // Pagination properties
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages;
        private int _totalItems;
        private bool _isLoading;

        public string CustomerFullName
        {
            get => _customerFullName;
            set { _customerFullName = value; OnPropertyChanged(nameof(CustomerFullName));
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
            }
        }

        public string CustomerCCCD
        {
            get => _customerCCCD;
            set
            {
                _customerCCCD = value; OnPropertyChanged(nameof(CustomerCCCD));
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
            }
        }

        public string CustomerPhoneNumber
        {
            get => _customerPhoneNumber;
            set { _customerPhoneNumber = value; OnPropertyChanged(nameof(CustomerPhoneNumber));
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
            }
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
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await UpdateAvailableRoomsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error updating available rooms");
                        // Optionally dispatch to UI thread to show error
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Error updating room availability. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                    }
                });
            }
        }

        public string SelectedRoomNumber
        {
            get => _selectedRoomNumber;
            set { _selectedRoomNumber = value; OnPropertyChanged(nameof(SelectedRoomNumber));
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
            }
        }

        public DateTime? CheckInDate
        {
            get => _checkInDate;
            set { _checkInDate = value; OnPropertyChanged(nameof(CheckInDate));
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
            }
        }

        public DateTime? CheckOutDate
        {
            get => _checkOutDate;
            set { _checkOutDate = value; OnPropertyChanged(nameof(CheckOutDate));
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
            }
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

        // Pagination properties
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
                CurrentPage = 1; // Reset to first page when page size changes
                // Reload data when page size changes
                _ = LoadDataAsync();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        }

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                OnPropertyChanged(nameof(TotalItems));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        private void NotifyPaginationCommandsChanged()
        {
            if (GoToPreviousPageCommand is AsyncRelayCommand prevCmd) prevCmd.NotifyCanExecuteChanged();
            if (GoToNextPageCommand is AsyncRelayCommand nextCmd) nextCmd.NotifyCanExecuteChanged();
            if (GoToFirstPageCommand is AsyncRelayCommand firstCmd) firstCmd.NotifyCanExecuteChanged();
            if (GoToLastPageCommand is AsyncRelayCommand lastCmd) lastCmd.NotifyCanExecuteChanged();
        }

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
                ((AsyncRelayCommand)SaveEditCommand).NotifyCanExecuteChanged();
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
        // end of properties for editing

        public ICommand AddNewBookingCommand { get; private set; }
        public ICommand ShowBookingsCommand { get; private set; }
        public ICommand ShowAvailableRoomsCommand { get; private set; }
        public ICommand NavigateToRoomViewCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand CheckoutCommand { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand ReportsCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand NavigateProfileCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand DeleteBookingCommand { get;private set; }
        // edit booking commands
        public ICommand EditBookingCommand { get; private set; }
        public ICommand SaveEditCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        // end of edit booking commands
        // create invoice command
        public ICommand CreateInvoiceCommand { get; private set; }
        
        // Pagination commands
        public ICommand GoToPreviousPageCommand { get; private set; }
        public ICommand GoToNextPageCommand { get; private set; }
        public ICommand GoToFirstPageCommand { get; private set; }
        public ICommand GoToLastPageCommand { get; private set; }

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

            CheckoutCommand = new AsyncRelayCommand<Booking?>(
                execute: b => CheckoutAsync(b!),
                canExecute: b => b != null && b.Status == BookingStatus.CheckedIn);

            EditBookingCommand = new AsyncRelayCommand<Booking?>(
                execute: b => EditBookingAsync(b!),
                canExecute: b => b != null);


            SaveEditCommand = new AsyncRelayCommand(
               execute: () => SaveEditAsync(),
               canExecute: () => CanSaveEdit() );

            CancelEditCommand = new AsyncRelayCommand<Booking?>(
                execute: b => CancelEditAsync(b!),
                canExecute: b => b != null);

            LoadedCommand = new AsyncRelayCommand(LoadDataAsync);

            ReportsCommand = new AsyncRelayCommand(ReportsAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            ClearFormCommand = new AsyncRelayCommand(ClearFormAsync);
            SearchCommand = new RelayCommand(PerformSearch);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);
            LogoutCommand = new RelayCommand(Logout);
            // DeleteBookingCommand
            DeleteBookingCommand = new AsyncRelayCommand<Booking?>(
                execute: b => DeleteAsync(b!),
                canExecute: b => b != null);

            // CreateInvoiceCommand
            CreateInvoiceCommand = new AsyncRelayCommand<Booking?>(
                execute: b => CreateInvoiceAsync(b!),
                canExecute: b => b != null);

            // Pagination commands
            GoToPreviousPageCommand = new AsyncRelayCommand(
                execute: () => GoToPreviousPageAsync(),
                canExecute: () => CanGoToPreviousPage);

            GoToNextPageCommand = new AsyncRelayCommand(
                execute: () => GoToNextPageAsync(),
                canExecute: () => CanGoToNextPage);

            GoToFirstPageCommand = new AsyncRelayCommand(
                execute: () => GoToFirstPageAsync(),
                canExecute: () => CanGoToPreviousPage);

            GoToLastPageCommand = new AsyncRelayCommand(
                execute: () => GoToLastPageAsync(),
                canExecute: () => CanGoToNextPage);
        }

        protected override async Task OnLoadedAsync()
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                _logger?.LogInformation("Loading receptionist data");
                var bookings = await _bookingService.GetAllAsync();
                
                // Order bookings by check-in date (recent first) then by check-out date (recent first)
                _allBookings = bookings
                    .OrderByDescending(b => b.CheckInDate)
                    .ThenByDescending(b => b.CheckOutDate)
                    .ToList();
                
                TotalItems = _allBookings.Count;
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                
                // Notify pagination command changes
                NotifyPaginationCommandsChanged();
                
                // Load first page
                await LoadCurrentPageAsync();
                
                await UpdateAvailableRoomsAsync();
                _logger?.LogInformation("Successfully loaded {BookingCount} bookings ordered by check-in date (recent first)", bookings.Count);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error loading data");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error loading data");
                MessageBox.Show("Error loading data. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
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
                    MessageBox.Show("Please fill in all required information.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //Vailadte date 
                if (CheckInDate >= CheckOutDate)
                {
                    MessageBox.Show("Check-out date must be after check-in date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (CheckInDate < DateTime.Now)
                {
                    MessageBox.Show("Check-in date cannot be in the past.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check for conflicts
                var conflicting = await _bookingService.GetConflictingBookingsAsync(
                    SelectedRoomNumber, CheckInDate.Value, CheckOutDate.Value
                );
                if (conflicting.Any())
                {
                    MessageBox.Show($"Room {SelectedRoomNumber} is not available during the selected period.",
                        "Room Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var customer = new Customer
                {
                    FullName = CustomerFullName,
                    CCCD = CustomerCCCD,
                    PhoneNumber = CustomerPhoneNumber,
                    Type = SelectedCustomerType
                };

                //Calculate total amount based on room type and dates
                var room = await _roomService.GetByRoomNumberAsync(SelectedRoomNumber);
                if (room == null)
                {
                    MessageBox.Show("Selected room number does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                decimal totalAmount = room.PricePerNight * (CheckOutDate.Value.Date - CheckInDate.Value.Date).Days;
                
                var employee = AppSession.GetCurrentUserAccount()?.Employee;

                var booking = new Booking
                {
                    Customer = customer,
                    RoomType = SelectedRoomType,
                    RoomNumber = SelectedRoomNumber,
                    CheckInDate = CheckInDate.Value,
                    CheckOutDate = CheckOutDate.Value,
                    Status = SelectedStatus,
                    Room = room,
                    TotalAmount = totalAmount,
                };
                // Setting employee IDs based on booking status
                // Always set the booking employee ID (the person who created the booking)
                booking.BookingEmployeeId = employee?.Id;
                
                // Set check-in/check-out employee IDs based on status
                if (SelectedStatus == BookingStatus.CheckedIn)
                {
                    booking.CheckInEmployeeID = employee?.Id;
                }
                else if (SelectedStatus == BookingStatus.CheckedOut)
                {
                    booking.CheckOutEmployeeID = employee?.Id;
                }

                await _bookingService.CreateAsync(booking);
                
                // Refresh the data to maintain proper ordering
                await LoadDataAsync();
                await UpdateAvailableRoomsAsync();

                ClearInputFields();
                MessageBox.Show("Booking added successfully!", "Success", MessageBoxButton.OK);
                _logger?.LogInformation("Successfully created booking for customer {CustomerName} in room {RoomNumber}", customer.FullName, booking.RoomNumber);
            }
            catch (DuplicateEntityException ex)
            {
                _logger?.LogWarning(ex, "Duplicate entity detected");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error creating booking");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error creating booking");
                MessageBox.Show("Error adding booking. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Updating booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Please select a booking to update.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error while creating booking. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync(Booking? booking)
        {
            try
            {
                _logger?.LogInformation("Deleting booking {BookingId}", booking?.Id);
                if (booking == null)
                {
                    MessageBox.Show("Please select a booking to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this booking?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Check if there's an associated invoice and delete it first
                    var invoiceService = App.ServiceProvider.GetRequiredService<HotelManager.Services.InvoiceService>();
                    var existingInvoice = await invoiceService.GetByBookingIdAsync(booking.Id);
                    if (existingInvoice != null)
                    {
                        int paymentCount = await invoiceService.DeleteAsync(existingInvoice.Id);
                        _logger?.LogInformation("Deleted invoice {InvoiceId} and {PaymentCount} associated payments for booking {BookingId}", 
                            existingInvoice.Id, paymentCount, booking.Id);
                    }
                    
                    await _bookingService.DeleteAsync(booking.Id);
                    Bookings.Remove(booking);
                    await UpdateAvailableRoomsAsync();
                    MessageBox.Show("Booking deleted successfully!", "Success", MessageBoxButton.OK);
                    _logger?.LogInformation("Successfully deleted booking {BookingId}", booking.Id);
                    TotalBookings = Bookings.Count;
                }
            }
            catch (EntityNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Entity not found");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BusinessException ex)
            {
                _logger?.LogWarning(ex, "Business error deleting booking");
                MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error deleting booking");
                MessageBox.Show("Error when deleting booking. Please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Error while loading available rooms. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                IsEditMode = false;
                MessageBox.Show("Form cleared successfully!", "Clear Form", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error clearing form");
            }
        }

        private async Task CheckoutAsync(Booking booking)
        {
            if (booking == null)
            {
                _logger?.LogWarning("Checkout attempted with null booking");
                MessageBox.Show("Please select a booking to checkout.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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

                var invoiceService = App.ServiceProvider.GetRequiredService<HotelManager.Services.InvoiceService>();

                // Try fetch existing invoice
                var invoice = await invoiceService.GetByBookingIdAsync(booking.Id);
                if (invoice == null)
                {
                    _logger?.LogInformation("No invoice found for booking {BookingId}, creating new one", booking.Id);
                    invoice = await invoiceService.CreateForBookingAsync(booking, booking.TotalAmount);
                }
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
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Work assignment during checkout failed");
                    }
                }
                await LoadDataAsync();

                //Đảm bảo phải pay hêt khi checkout
                // Tạo mới invoice cho booking
                // Điều hướng sang PaymentViewModel, truyền invoice
                Debug.WriteLine($"Navigating to PaymentViewModel with Invoice's Booking ID: {invoice.BookingId}, TotalAmount: {invoice.TotalAmount}");
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
                var bookingsList = string.Join(Environment.NewLine, Bookings.Select(b => 
                    $"{b.Customer?.FullName ?? "Unknown"} - {b.RoomNumber} ({b.Status})"));
                MessageBox.Show(bookingsList, "Bookings List", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing bookings");
                MessageBox.Show("Error while displaying bookings list. Please try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAvailableRooms()
        {
            try
            {
                if (AvailableRooms.Count == 0)
                {
                    MessageBox.Show("No available room for selected room type!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var roomsList = string.Join(Environment.NewLine, AvailableRooms);
                MessageBox.Show(roomsList, "Available Rooms List", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing available rooms");
                MessageBox.Show("Error while displaying available rooms. Please try again!.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToRoomView()
        {
            _navigationService?.NavigateTo<ReceptionistRoomViewModel>();
        }

        private async void PerformSearch()
        {
            try
            {
                IsLoading = true;
                
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // Reset to all bookings
                    _allBookings = (await _bookingService.GetAllAsync())
                        .OrderByDescending(b => b.CheckInDate)
                        .ThenByDescending(b => b.CheckOutDate)
                        .ToList();
                }
                else
                {
                    string searchText = SearchText.Trim().ToLowerInvariant();
                    var allBookings = await _bookingService.GetAllAsync();
                    
                    _allBookings = allBookings
                        .Where(b =>
                            (b.Customer?.FullName?.ToLowerInvariant().Contains(searchText) ?? false) ||
                            (b.Customer?.CCCD?.ToLowerInvariant().Contains(searchText) ?? false) ||
                            (b.Customer?.PhoneNumber?.Contains(searchText) ?? false) ||
                            b.RoomNumber.ToLowerInvariant().Contains(searchText) ||
                            b.Status.ToString().ToLowerInvariant().Contains(searchText) ||
                            (b.Customer?.Type.ToString().ToLowerInvariant().Contains(searchText) ?? false) ||
                            b.RoomType.ToString().ToLowerInvariant().Contains(searchText)
                        )
                        .OrderByDescending(b => b.CheckInDate)
                        .ThenByDescending(b => b.CheckOutDate)
                        .ToList();
                }

                TotalItems = _allBookings.Count;
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                CurrentPage = 1; // Reset to first page when searching
                
                await LoadCurrentPageAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing search");
                MessageBox.Show("Error performing search. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
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

        // Method to load booking details for editing
        private void LoadBookingForEdit(Booking booking)
        {
            if (booking?.Customer != null)
            {
                CustomerFullName = booking.Customer.FullName ?? string.Empty;
                CustomerCCCD = booking.Customer.CCCD ?? string.Empty;
                CustomerPhoneNumber = booking.Customer.PhoneNumber ?? string.Empty;
                SelectedCustomerType = booking.Customer.Type;
            }
            else
            {
                CustomerFullName = string.Empty;
                CustomerCCCD = string.Empty;
                CustomerPhoneNumber = string.Empty;
                SelectedCustomerType = default;
            }

            SelectedRoomType = booking?.RoomType ?? default;
            SelectedRoomNumber = booking?.RoomNumber ?? string.Empty;
            CheckInDate = booking?.CheckInDate ?? DateTime.Now;
            CheckOutDate = booking?.CheckOutDate ?? DateTime.Now.AddDays(1);
            SelectedStatus = booking?.Status ?? BookingStatus.Pending;
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

        private bool CanSaveEdit()
        {
            return !string.IsNullOrWhiteSpace(CustomerFullName) &&
                   !string.IsNullOrWhiteSpace(CustomerCCCD) &&
                   !string.IsNullOrWhiteSpace(CustomerPhoneNumber) &&
                   !string.IsNullOrWhiteSpace(SelectedRoomNumber) &&
                   CheckInDate != null &&
                   CheckOutDate != null &&
                   CheckInDate < CheckOutDate;
        }

        private async Task SaveEditAsync()
        {
            try
            {
                _logger?.LogInformation("Saving edited booking {BookingId}", SelectedBookingForEdit.Id);
                if (SelectedBookingForEdit == null)
                {
                    MessageBox.Show("Please select a booking to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CustomerFullName) ||
                    string.IsNullOrWhiteSpace(CustomerCCCD) ||
                    string.IsNullOrWhiteSpace(CustomerPhoneNumber) ||
                    string.IsNullOrWhiteSpace(SelectedRoomNumber) ||
                    CheckInDate == null ||
                    CheckOutDate == null)
                {
                    MessageBox.Show("Please fill in all required information.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //Vailadte date 
                if (CheckInDate >= CheckOutDate)
                {
                    MessageBox.Show("Check-out date must be after check-in date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Conflict check excluding itself
                var conflicting = await _bookingService.GetConflictingBookingsAsync(
                    SelectedRoomNumber, CheckInDate.Value, CheckOutDate.Value, SelectedBookingForEdit.Id
                );
                if (conflicting.Any())
                {
                    MessageBox.Show($"Room {SelectedRoomNumber} is not available during the selected period.",
                        "Room Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Recalculate total amount
                var room = await _roomService.GetByRoomNumberAsync(SelectedRoomNumber);
                if (room == null)
                {
                    MessageBox.Show("Selected room number does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int numberOfNights = (CheckOutDate.Value.Date - CheckInDate.Value.Date).Days;
                decimal totalAmount = room.PricePerNight * numberOfNights;

                // Update the booking's properties
                if (SelectedBookingForEdit.Customer == null)
                {
                    SelectedBookingForEdit.Customer = new Customer();
                }
                SelectedBookingForEdit.Customer.FullName = CustomerFullName;
                SelectedBookingForEdit.Customer.CCCD = CustomerCCCD;
                SelectedBookingForEdit.Customer.PhoneNumber = CustomerPhoneNumber;
                SelectedBookingForEdit.Customer.Type = SelectedCustomerType;

                SelectedBookingForEdit.RoomType = SelectedRoomType;
                SelectedBookingForEdit.RoomNumber = SelectedRoomNumber;
                SelectedBookingForEdit.CheckInDate = CheckInDate.Value;
                SelectedBookingForEdit.CheckOutDate = CheckOutDate.Value;
                SelectedBookingForEdit.Status = SelectedStatus;
                SelectedBookingForEdit.TotalAmount = totalAmount;
                SelectedBookingForEdit.Room = room;
                //Setting conditional booking employee
                var employee = AppSession.GetCurrentUserAccount()?.Employee;
                if(SelectedStatus == BookingStatus.CheckedIn)
                {
                    SelectedBookingForEdit.CheckInEmployeeID = employee?.Id;
                }
                else if (SelectedStatus == BookingStatus.CheckedOut)
                {
                    SelectedBookingForEdit.CheckOutEmployeeID = employee?.Id;
                }

                await _bookingService.UpdateAsync(SelectedBookingForEdit);
                
                // Update the corresponding invoice if it exists
                var invoiceService = App.ServiceProvider.GetRequiredService<HotelManager.Services.InvoiceService>();
                var existingInvoice = await invoiceService.GetByBookingIdAsync(SelectedBookingForEdit.Id);
                if (existingInvoice != null)
                {
                    existingInvoice.TotalAmount = totalAmount;
                    await invoiceService.UpdateAsync(existingInvoice);
                    _logger?.LogInformation("Updated invoice {InvoiceId} total amount to {TotalAmount} for booking {BookingId}", 
                        existingInvoice.Id, totalAmount, SelectedBookingForEdit.Id);
                }
                
                await UpdateAvailableRoomsAsync();
                await LoadDataAsync();
                MessageBox.Show("Booking updated successfully!", "Success", MessageBoxButton.OK);
                _logger?.LogInformation("Successfully updated booking {BookingId}", SelectedBookingForEdit.Id);
                // Clear the edit fields
                ClearInputFields();
                SelectedBookingForEdit = null;
                IsEditMode = false;
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


        private async Task CreateInvoiceAsync(Booking? booking)
        {
            if (booking == null)
            {
                MessageBox.Show("Please select a booking to create invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                _logger?.LogInformation("Creating invoice for booking {BookingId}", booking?.Id);

                var invoiceService = App.ServiceProvider.GetRequiredService<HotelManager.Services.InvoiceService>();

                var invoice = await invoiceService.GetByBookingIdAsync(booking.Id);
                if (invoice == null)
                {
                    _logger?.LogInformation("No invoice found for booking {BookingId}, creating new one", booking.Id);
                    invoice = await invoiceService.CreateForBookingAsync(booking, booking.TotalAmount);
                }

                _navigationService?.NavigateTo<PaymentViewModel>(invoice);
                _notificationService?.ShowSuccess($"Invoice ready for booking {booking.RoomNumber}. Navigated to Payment View.");
                _logger?.LogInformation("Invoice ready for booking {BookingId}", booking.Id);
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

        // Pagination methods
        private async Task LoadCurrentPageAsync()
        {
            try
            {
                if (_allBookings == null || !_allBookings.Any())
                {
                    Bookings.Clear();
                    TotalBookings = 0;
                    return;
                }

                var startIndex = (CurrentPage - 1) * PageSize;
                var pageBookings = _allBookings
                    .Skip(startIndex)
                    .Take(PageSize)
                    .ToList();

                Bookings.Clear();
                foreach (var booking in pageBookings)
                {
                    Bookings.Add(booking);
                }
                TotalBookings = Bookings.Count;

                _logger?.LogDebug("Loaded page {CurrentPage} of {TotalPages} with {BookingCount} bookings", 
                    CurrentPage, TotalPages, pageBookings.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading current page");
                MessageBox.Show("Error loading page. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GoToPreviousPageAsync()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
                await LoadCurrentPageAsync();
                NotifyPaginationCommandsChanged();
            }
        }

        private async Task GoToNextPageAsync()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
                await LoadCurrentPageAsync();
                NotifyPaginationCommandsChanged();
            }
        }

        private async Task GoToFirstPageAsync()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage = 1;
                await LoadCurrentPageAsync();
                NotifyPaginationCommandsChanged();
            }
        }

        private async Task GoToLastPageAsync()
        {
            if (CanGoToNextPage)
            {
                CurrentPage = TotalPages;
                await LoadCurrentPageAsync();
                NotifyPaginationCommandsChanged();
            }
        }
    }
}
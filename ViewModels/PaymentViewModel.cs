using CommunityToolkit.Mvvm.Input;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using HotelManager.Data;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Interfaces;

namespace HotelManager.ViewModels
{
    public class PaymentViewModel : BaseViewModel, INavigationAware
    {
        private readonly PaymentService _paymentService;
        private readonly InvoiceService _invoiceService;
        private readonly INavigationService _navigationService;

        private DateTime _paymentDate;
        private decimal _amount;
        private PaymentMethod _paymentMethod;
        private decimal _remainingAmount;
        private ObservableCollection<Payment> _payments;
        private Payment _selectedPayment;
        private Invoice _currentInvoice;
        private Invoice _editingInvoice; // Invoice for the payment being edited
        private Booking currentBooking;
        private string _searchText;
        private List<Payment> _allPayments; // Store all payments for search functionality

        // Properties for displaying booking information
        public string CustomerName => currentBooking?.Customer?.FullName ?? "N/A";
        public string CustomerId => currentBooking?.CustomerId.ToString() ?? "N/A";
        public string BookingEmployeeId => currentBooking?.BookingEmployeeId?.ToString() ?? "N/A";
        public string RoomNumber => currentBooking?.RoomNumber ?? "N/A";
        public int TotalDaysOfStay => currentBooking != null ? (currentBooking.CheckOutDate - currentBooking.CheckInDate).Days : 0;
        public decimal TotalAmount => currentBooking?.TotalAmount ?? 0m;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public ObservableCollection<Invoice> AvailableInvoices { get; set; } = new();



        public DateTime PaymentDate
        {
            get => _paymentDate;
            set
            {
                _paymentDate = value;
                OnPropertyChanged(nameof(PaymentDate));
                OnCanExecuteChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
                OnCanExecuteChanged();

                // Update remaining amount when editing a payment
                if (IsEditing && EditingInvoice != null)
                {
                    UpdateRemainingAmountForEditing();
                }
            }
        }

        public PaymentMethod PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
                OnCanExecuteChanged();
            }
        }

        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set
            {
                _remainingAmount = value;
                OnPropertyChanged(nameof(RemainingAmount));
                //MessageBox.Show($"Remaining Amount: {RemainingAmount:C}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                //if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<Payment> Payments
        {
            get => _payments;
            set { _payments = value; OnPropertyChanged(nameof(Payments)); }
        }

        public Payment SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                _selectedPayment = value;
                Debug.WriteLine($"SelectedPayment changed: {(_selectedPayment != null ? $"ID={_selectedPayment.Id}, Amount={_selectedPayment.Amount}" : "null")}");
                OnPropertyChanged(nameof(SelectedPayment));

                // Don't automatically set IsEditing when payment is selected
                // IsEditing should only be set when explicitly editing via the edit button

                if (_selectedPayment == null)
                {
                    ClearInputFields();
                }
                OnCanExecuteChanged();
            }
        }

        public Invoice CurrentInvoice
        {
            get => _currentInvoice;
            set
            {
                _currentInvoice = value;
                OnPropertyChanged(nameof(CurrentInvoice));
                //currentBooking = value.Booking;
                //MessageBox.Show($"Current Invoice: {value.Id}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
        }

        public Invoice EditingInvoice
        {
            get => _editingInvoice;
            set
            {
                _editingInvoice = value;
                OnPropertyChanged(nameof(EditingInvoice));
                OnCanExecuteChanged();

                // Update remaining amount when editing invoice changes
                if (IsEditing && value != null)
                {
                    UpdateRemainingAmountForEditing();
                }
            }
        }



        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                OnCanExecuteChanged(); // So commands like Add/Save update availability


            }
        }
        public ICommand AddPaymentCommand { get; set; }
        public ICommand SaveEditedPaymentCommand { get; set; }
        public ICommand DeletePaymentCommand { get; set; }
        public ICommand NavigateBackCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand CancelPaymentCommand { get; set; }
        public ICommand EnableEditPayment { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ShowAllPaymentsCommand { get; set; }
        public ICommand ShowPaymentDetailsCommand { get; set; }

        public PaymentViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Initialize with design-time data
                Payments = new ObservableCollection<Payment>();
                PaymentDate = DateTime.Now;
                PaymentMethod = PaymentMethod.Cash;

                // Initialize commands with empty implementations for design-time
                AddPaymentCommand = new RelayCommand(() => { }, () => false);
                SaveEditedPaymentCommand = new RelayCommand<Payment>(_ => { }, _ => false);
                DeletePaymentCommand = new RelayCommand<Payment>(_ => { }, _ => false);
                NavigateBackCommand = new RelayCommand(() => { });
                LoadedCommand = new RelayCommand(() => { });
                CancelPaymentCommand = new RelayCommand(() => { });
                EnableEditPayment = new RelayCommand(() => { });
                return;
            }

            // Runtime initialization
            _paymentService = App.ServiceProvider?.GetRequiredService<PaymentService>() ?? throw new InvalidOperationException("PaymentService not registered");
            _invoiceService = App.ServiceProvider?.GetRequiredService<InvoiceService>() ?? throw new InvalidOperationException("InvoiceService not registered");
            _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? throw new InvalidOperationException("INavigationService not registered");

            InitializeViewModel();

        }

        //không sử dụng constructor này nữa, vì đã sử dụng OnNavigatedTo để nhận Invoice từ NavigationService
        //public PaymentViewModel(Invoice invoice)
        //{
        //    _paymentService = App.ServiceProvider?.GetRequiredService<PaymentService>() ?? throw new InvalidOperationException("PaymentService not registered");
        //    _invoiceService = App.ServiceProvider?.GetRequiredService<InvoiceService>() ?? throw new InvalidOperationException("InvoiceService not registered");
        //    _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? throw new InvalidOperationException("INavigationService not registered");
        //    InitializeViewModel();

        //    CurrentInvoice = invoice;

        //}

        private void InitializeViewModel()
        {
            AddPaymentCommand = new RelayCommand(async () => await AddPaymentAsync(), CanAddPayment);


            SaveEditedPaymentCommand = new AsyncRelayCommand<Payment>(
               execute: async (payment) => await SavePaymentAsync(),
               canExecute: (payment) => CanSavePayment()
            );

            DeletePaymentCommand = new RelayCommand<Payment>(async (payment) => await DeletePaymentAsync(payment), CanDeletePayment);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            LoadedCommand = new RelayCommand(async () => await OnLoadedAsync());
            CancelPaymentCommand = new RelayCommand(() => CancelInput());
            EnableEditPayment = new RelayCommand<Payment>(p => EditPaymentAsync(p!), p => p != null && !IsEditing);
            SearchCommand = new RelayCommand(PerformSearch);
            ShowAllPaymentsCommand = new RelayCommand(async () => await ShowAllPaymentsAsync());
            ShowPaymentDetailsCommand = new RelayCommand<Payment>(p => ShowPaymentDetailsAsync(p!), p => p != null);

            Payments = new ObservableCollection<Payment>();
            PaymentDate = DateTime.Now;
            PaymentMethod = PaymentMethod.Cash;


            Debug.WriteLine("PaymentViewModel: Constructor called, SavePaymentCommand initialized.");
        }

        private void CancelInput()
        {
            ClearInputFields();
            IsEditing = false;
            SelectedPayment = null; // Clear selected payment
            EditingInvoice = null; // Clear editing invoice

            // Reset remaining amount to current invoice's remaining amount
            if (CurrentInvoice != null)
            {
                RemainingAmount = CurrentInvoice.TotalAmount;
            }

            OnCanExecuteChanged();
        }

        public void OnNavigatedFrom()
        {
            Debug.WriteLine("PaymentViewModel.OnNavigatedFrom called");
            // Clear the current invoice when navigating away
            ClearInputFields();
        }

        public void OnNavigatedTo(object parameter)
        {
            Debug.WriteLine($"PaymentViewModel.OnNavigatedTo called with parameter: {parameter?.GetType().Name}");

            if (parameter is Invoice invoice)
            {
                CurrentInvoice = invoice;
                RemainingAmount = invoice.TotalAmount; //testing
                Amount = invoice.TotalAmount; // Set initial amount to total amount of the invoice
                currentBooking = invoice.Booking;
                Debug.WriteLine($"Assigned CurrentInvoice's Booking ID: {invoice.BookingId}");

                // Trigger property change notifications for booking information properties
                OnPropertyChanged(nameof(CustomerName));
                OnPropertyChanged(nameof(CustomerId));
                OnPropertyChanged(nameof(BookingEmployeeId));
                OnPropertyChanged(nameof(RoomNumber));
                OnPropertyChanged(nameof(TotalDaysOfStay));
                OnPropertyChanged(nameof(TotalAmount));
            }
            else
            {
                Debug.WriteLine("Parameter passed to PaymentViewModel was not an Invoice.");
            }
        }

        protected override async Task OnLoadedAsync()
        {
            await LoadDataAsync();
        }



        private async void LoadData()
        {
            var payments = await _paymentService.GetAllAsync();
            Payments = new ObservableCollection<Payment>(payments.Where(p => p.InvoiceId == CurrentInvoice.Id));
            //RemainingAmount = 10000000;
        }

        private async Task RefreshPaymentsAsync()
        {
            try
            {
                var payments = await _paymentService.GetAllAsync();

                // Update _allPayments list to keep it in sync
                _allPayments = payments.ToList();

                var filteredPayments = payments.Where(p => p.InvoiceId == CurrentInvoice.Id).ToList();

                Payments.Clear();
                foreach (var payment in filteredPayments)
                {
                    Payments.Add(payment);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to refresh payments");
                Debug.WriteLine($"RefreshPaymentsAsync error: {ex.Message}");
            }
        }

        private async Task AddPaymentAsync()
        {
            try
            {
                LogInformation("Adding new payment: Amount {Amount:C}, Method {PaymentMethod}, InvoiceId {InvoiceId}",
                    Amount, PaymentMethod, CurrentInvoice.Id);
                Debug.WriteLine("PaymentViewModel: AddPaymentAsync started");

                var payment = new Payment
                {
                    PaymentDate = PaymentDate,
                    Amount = Amount,
                    PaymentMethod = PaymentMethod,
                    InvoiceId = CurrentInvoice.Id
                };

                await _paymentService.CreateAsync(payment);
                Payments.Add(payment);

                // Also add to _allPayments list for "Show All" functionality
                if (_allPayments != null)
                {
                    _allPayments.Add(payment);
                }

                // Update remaining amount and invoice total
                RemainingAmount -= Amount;
                CurrentInvoice.TotalAmount -= Amount;
                await _invoiceService.UpdateAsync(CurrentInvoice);

                // Refresh the payments list to show updated data
                await RefreshPaymentsAsync();

                ClearInputFields();
                IsEditing = false;

                // Log user activity for audit
                await LogUserActivityAsync("CREATE", "Payment", payment.Id.ToString(),
                    $"Added payment: {payment.Amount:C} via {payment.PaymentMethod}");

                LogInformation("Payment added successfully with ID {PaymentId}", payment.Id);
                MessageBox.Show("Payment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Debug.WriteLine("PaymentViewModel: Payment added successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to add payment");
                Debug.WriteLine($"PaymentViewModel: AddPaymentAsync error - {ex.Message}");
                MessageBox.Show($"Failed to add payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Save Edit
        private async Task SavePaymentAsync()
        {
            try
            {
                if (SelectedPayment != null)
                {
                    LogInformation("Updating payment with ID {PaymentId}", SelectedPayment.Id);

                    // Store original amount for calculation
                    var originalAmount = SelectedPayment.Amount;

                    SelectedPayment.PaymentDate = PaymentDate;
                    SelectedPayment.Amount = Amount;
                    SelectedPayment.PaymentMethod = PaymentMethod;

                    // Update the editing invoice (not the current invoice)
                    if (EditingInvoice != null)
                    {
                        EditingInvoice.TotalAmount += originalAmount - Amount;
                        await _invoiceService.UpdateAsync(EditingInvoice);

                        // Also update the current invoice if it's the same invoice
                        if (CurrentInvoice != null && CurrentInvoice.Id == EditingInvoice.Id)
                        {
                            CurrentInvoice.TotalAmount = EditingInvoice.TotalAmount;
                        }
                    }

                    Debug.WriteLine($"Saving: SelectedPayment - ID={SelectedPayment.Id}, Amount={SelectedPayment.Amount}");
                    await _paymentService.UpdateAsync(SelectedPayment);

                    // Store payment info before clearing
                    var paymentId = SelectedPayment.Id;
                    var paymentAmount = SelectedPayment.Amount;
                    var paymentMethod = SelectedPayment.PaymentMethod;

                    // Log user activity for audit
                    await LogUserActivityAsync("UPDATE", "Payment", paymentId.ToString(),
                        $"Updated payment: {paymentAmount:C} via {paymentMethod}");

                    LogInformation("Payment updated successfully with ID {PaymentId}", paymentId);

                    IsEditing = false;
                    ClearInputFields();
                    SelectedPayment = null; // Clear selection after saving
                    EditingInvoice = null; // Clear editing invoice

                    // Refresh the payments list to show updated data
                    await RefreshPaymentsAsync();

                    // Update remaining amount to reflect the new state after saving
                    if (CurrentInvoice != null)
                    {
                        RemainingAmount = CurrentInvoice.TotalAmount;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show("Payment saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    );
                    Debug.WriteLine("PaymentViewModel: Payment saved successfully");
                }
                else
                {
                    LogWarning("Attempted to save null payment or no payment selected");
                    Debug.WriteLine("SavePaymentAsync: payment or SelectedPayment is null");
                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show("Please select a payment to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    );
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to save payment");
                Debug.WriteLine($"SavePaymentAsync error: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"Failed to save payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                );
            }
        }

        private async Task DeletePaymentAsync(Payment payment)
        {
            try
            {
                LogInformation("Deleting payment with ID {PaymentId}", payment?.Id);
                Debug.WriteLine("PaymentViewModel: DeletePaymentAsync started");

                if (payment != null)
                {
                    await _paymentService.DeleteAsync(payment.Id);
                    Payments.Remove(payment);

                    // Also remove from _allPayments list for "Show All" functionality
                    if (_allPayments != null)
                    {
                        _allPayments.Remove(payment);
                    }

                    RemainingAmount += payment.Amount;

                    // Update the invoice for this payment
                    var paymentInvoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                    if (paymentInvoice != null)
                    {
                        paymentInvoice.TotalAmount += payment.Amount;
                        await _invoiceService.UpdateAsync(paymentInvoice);

                        // Also update the current invoice if it's the same invoice
                        if (CurrentInvoice != null && CurrentInvoice.Id == paymentInvoice.Id)
                        {
                            CurrentInvoice.TotalAmount = paymentInvoice.TotalAmount;
                        }
                    }


                    ClearInputFields();
                    SelectedPayment = null;
                    OnCanExecuteChanged();
                    IsEditing = false;

                    // Clear editing invoice if the deleted payment was being edited
                    if (EditingInvoice != null && EditingInvoice.Id == payment.InvoiceId)
                    {
                        EditingInvoice = null;
                    }

                    // Refresh the payments list to show updated data
                    await RefreshPaymentsAsync();

                    await LogUserActivityAsync("DELETE", "Payment", payment.Id.ToString(),
                        $"Deleted payment: {payment.Amount:C} via {payment.PaymentMethod}");

                    LogInformation("Payment deleted successfully with ID {PaymentId}", payment.Id);
                    MessageBox.Show("Payment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Debug.WriteLine("PaymentViewModel: Payment deleted successfully");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to delete payment");
                Debug.WriteLine($"PaymentViewModel: DeletePaymentAsync error - {ex.Message}");
                MessageBox.Show($"Failed to delete payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddPayment()
        {
            Debug.WriteLine($"CanAddPayment: Amount={Amount}, RemainingAmount={RemainingAmount}, IsEditing={IsEditing}");
            return CurrentInvoice != null && Amount > 0 && RemainingAmount >= Amount && !IsEditing;
        }

        private bool CanSavePayment()
        {
            if (SelectedPayment == null || !IsEditing || EditingInvoice == null)
                return false;

            // Optional: prevent saving with zero/negative amounts
            if (Amount <= 0)
                return false;

            // Calculate the original invoice amount by adding back all payments to the current total
            var allPaymentsForInvoice = Payments.Where(p => p.InvoiceId == EditingInvoice.Id).ToList();
            var originalInvoiceAmount = EditingInvoice.TotalAmount + allPaymentsForInvoice.Sum(p => p.Amount);

            // Calculate the total amount already paid for this invoice (excluding the payment being edited)
            var totalPaidExcludingCurrent = allPaymentsForInvoice.Where(p => p.Id != SelectedPayment.Id)
                                                                 .Sum(p => p.Amount);
            var newTotalPaid = totalPaidExcludingCurrent + Amount;

            Debug.WriteLine($"CanSavePayment: OriginalInvoiceAmount={originalInvoiceAmount:C}, " +
                           $"TotalPaidExcludingCurrent={totalPaidExcludingCurrent:C}, " +
                           $"NewAmount={Amount:C}, NewTotalPaid={newTotalPaid:C}");

            // Optional: prevent overpaying - check against original invoice amount
            if (newTotalPaid > originalInvoiceAmount)
            {
                Debug.WriteLine($"CanSavePayment: Overpayment detected - {newTotalPaid:C} > {originalInvoiceAmount:C}");
                return false;
            }

            // Optional: disable save if no real changes
            bool hasChanges =
                SelectedPayment.Amount != Amount ||
                SelectedPayment.PaymentDate != PaymentDate ||
                SelectedPayment.PaymentMethod != PaymentMethod;

            Debug.WriteLine($"CanSavePayment: HasChanges={hasChanges}, CanSave={hasChanges}");
            return hasChanges;
        }

        private bool CanDeletePayment(Payment payment)
        {
            return payment != null;
        }

        private void ClearInputFields()
        {
            PaymentDate = DateTime.Now;
            Amount = 0;
            PaymentMethod = PaymentMethod.Cash;
        }

        private void OnCanExecuteChanged()
        {
            if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
            if (SaveEditedPaymentCommand is AsyncRelayCommand<Payment> command2) command2.NotifyCanExecuteChanged();
            if (DeletePaymentCommand is RelayCommand<Payment> command3) command3.NotifyCanExecuteChanged();
            if (EnableEditPayment is RelayCommand<Payment> command4) command4.NotifyCanExecuteChanged();
        }

        private void NavigateBack()
        {
            _navigationService?.GoBack();
        }

        //Set textboxes for editing
        private async Task EditPaymentAsync(Payment payment)
        {
            try
            {
                if (payment == null)
                {
                    MessageBox.Show("No payment selected for editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Fetch the specific invoice for this payment
                var paymentInvoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                if (paymentInvoice == null)
                {
                    MessageBox.Show($"Could not find invoice with ID {payment.InvoiceId} for this payment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Set the editing invoice (separate from current invoice)
                EditingInvoice = paymentInvoice;

                // Set the selected payment and load its data into the form
                SelectedPayment = payment;
                LoadPaymentForEdit(payment);
                IsEditing = true;

                // Update command states
                OnCanExecuteChanged();

                Debug.WriteLine($"EditPaymentAsync: Editing payment ID={payment.Id}, Amount={payment.Amount}, Invoice ID={paymentInvoice.Id}, Invoice TotalAmount={paymentInvoice.TotalAmount:C}");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to edit payment");
                Debug.WriteLine($"PaymentViewModel: EditPaymentAsync error - {ex.Message}");
                MessageBox.Show($"Failed to edit payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPaymentForEdit(Payment payment)
        {
            if (payment == null) return;
            PaymentDate = payment.PaymentDate;
            Amount = payment.Amount;
            PaymentMethod = payment.PaymentMethod;
            Debug.WriteLine($"LoadPaymentForEdit: Loaded payment ID={payment.Id}, Amount={payment.Amount}");

            // Update command states after loading payment data
            OnCanExecuteChanged();

            // Update remaining amount for editing
            UpdateRemainingAmountForEditing();
        }

        private void UpdateRemainingAmountForEditing()
        {
            if (!IsEditing || EditingInvoice == null) return;

            // Calculate the original invoice amount by adding back all payments
            var allPaymentsForInvoice = Payments.Where(p => p.InvoiceId == EditingInvoice.Id).ToList();
            var originalInvoiceAmount = EditingInvoice.TotalAmount + allPaymentsForInvoice.Sum(p => p.Amount);

            // Calculate the total amount already paid (excluding the payment being edited)
            var totalPaidExcludingCurrent = allPaymentsForInvoice.Where(p => p.Id != SelectedPayment?.Id)
                                                                 .Sum(p => p.Amount);

            // Calculate remaining amount after the current payment edit
            var newRemainingAmount = originalInvoiceAmount - totalPaidExcludingCurrent - Amount;

            RemainingAmount = newRemainingAmount;

            Debug.WriteLine($"UpdateRemainingAmountForEditing: OriginalAmount={originalInvoiceAmount:C}, " +
                           $"TotalPaidExcludingCurrent={totalPaidExcludingCurrent:C}, " +
                           $"NewAmount={Amount:C}, RemainingAmount={RemainingAmount:C}");
        }

        // Search functionality
        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // If search is empty, show current invoice payments
                if (CurrentInvoice != null)
                {
                    var currentPayments = _allPayments?.Where(p => p.InvoiceId == CurrentInvoice.Id).ToList() ?? new List<Payment>();
                    Payments.Clear();
                    foreach (var payment in currentPayments)
                    {
                        Payments.Add(payment);
                    }
                }
                return;
            }

            string searchText = SearchText.Trim().ToLowerInvariant();
            var filteredPayments = _allPayments?.Where(p =>
                p.Id.ToString().Contains(searchText) ||
                p.PaymentDate.ToString("dd/MM/yyyy").Contains(searchText) ||
                p.Amount.ToString().Contains(searchText) ||
                p.PaymentMethod.ToString().ToLowerInvariant().Contains(searchText) ||
                p.InvoiceId.ToString().Contains(searchText)
            ).ToList() ?? new List<Payment>();

            Payments.Clear();
            foreach (var payment in filteredPayments)
            {
                Payments.Add(payment);
            }
        }

        // Show all payments
        private async Task ShowAllPaymentsAsync()
        {
            try
            {
                // Always refresh the data to ensure it's up to date
                var all = await _paymentService.GetAllAsync();
                _allPayments = all.ToList();

                Payments.Clear();
                foreach (var payment in _allPayments)
                {
                    Payments.Add(payment);
                }

                SearchText = string.Empty;
                MessageBox.Show("Showing all payments", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to show all payments");
                MessageBox.Show($"Failed to show all payments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Show payment details
        private async Task ShowPaymentDetailsAsync(Payment payment)
        {
            try
            {
                if (payment == null) return;

                // Get the invoice for this payment
                var paymentInvoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                if (paymentInvoice?.Booking == null) return;

                var booking = paymentInvoice.Booking;
                var customer = booking.Customer;

                var details = $"Payment Details:\n\n" +
                             $"Payment ID: {payment.Id}\n" +
                             $"Payment Date: {payment.PaymentDate:dd/MM/yyyy}\n" +
                             $"Amount: {payment.Amount:C}\n" +
                             $"Payment Method: {payment.PaymentMethod}\n" +
                             $"Invoice ID: {payment.InvoiceId}\n\n" +
                             $"Booking Information:\n" +
                             $"Customer Name: {customer?.FullName ?? "N/A"}\n" +
                             $"Customer ID: {(customer?.Id != null ? customer.Id.ToString() : "N/A")}\n" +
                             $"Booking Employee ID: {(booking.BookingEmployeeId.HasValue ? booking.BookingEmployeeId.Value.ToString() : "N/A")}\n" +
                             $"Room Number: {booking.RoomNumber ?? "N/A"}\n" +
                             $"Total Days: {(booking.CheckOutDate - booking.CheckInDate).Days}\n" +
                             $"Total Amount: {booking.TotalAmount:C}\n" +
                             $"Check-in: {booking.CheckInDate:dd/MM/yyyy}\n" +
                             $"Check-out: {booking.CheckOutDate:dd/MM/yyyy}\n" +
                             $"Status: {booking.Status}";

                MessageBox.Show(details, "Payment Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to show payment details");
                MessageBox.Show($"Failed to show payment details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Update LoadDataAsync to store all payments
        public async Task LoadDataAsync()
        {
            try
            {
                LogInformation("Loading payment data");
                Debug.WriteLine("PaymentViewModel: LoadDataAsync started");
                var invoices = await _invoiceService.GetAllAsync();
                var payments = await _paymentService.GetAllAsync();

                // Store all payments for search functionality
                _allPayments = payments.ToList();

                Payments.Clear();
                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                LogInformation("Successfully loaded {InvoiceCount} invoices and {PaymentCount} payments",
                    invoices.Count, Payments.Count);
                Debug.WriteLine($"PaymentViewModel: LoadDataAsync completed - {invoices.Count} invoices, {Payments.Count} payments");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to load payment data");
                Debug.WriteLine($"PaymentViewModel: LoadDataAsync error - {ex.Message}");
                throw;
            }
        }
    }
}
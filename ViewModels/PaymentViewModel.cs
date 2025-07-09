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
        private Booking currentBooking;

        public ObservableCollection<Invoice> AvailableInvoices { get; set; } = new();



        public DateTime PaymentDate
        {
            get => _paymentDate;
            set { _paymentDate = value; OnPropertyChanged(nameof(PaymentDate)); }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
                if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
            }
        }

        public PaymentMethod PaymentMethod
        {
            get => _paymentMethod;
            set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod));
                if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
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

                IsEditing = _selectedPayment != null;

                if (_selectedPayment == null)
                {
                    ClearInputFields();
                }
                if (SaveEditedPaymentCommand is RelayCommand<Payment> command) command.NotifyCanExecuteChanged();
                if (DeletePaymentCommand is RelayCommand<Payment> command2) command2.NotifyCanExecuteChanged();
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
                MessageBox.Show($"Current Invoice: {value.Id}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
        }

        //Editing properties
        private Payment _selectedPaymentForEdit;
        public Payment SelectedPaymentForEdit
        {
            get => _selectedPaymentForEdit;
            set
            {
                _selectedPaymentForEdit = value;
                OnPropertyChanged(nameof(SelectedPaymentForEdit));
                if (_selectedPaymentForEdit != null)
                {
                    LoadPaymentForEdit(value);
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
            AddPaymentCommand = new RelayCommand(async () => await AddPaymentAsync(),() => true);


            SaveEditedPaymentCommand = new AsyncRelayCommand<Payment>(
               execute: async (payment) => await SavePaymentAsync(),
               canExecute: (payment) => CanSavePayment()
            );

            DeletePaymentCommand = new RelayCommand<Payment>(async (payment) => await DeletePaymentAsync(payment), CanDeletePayment);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            LoadedCommand = new RelayCommand(async () => await OnLoadedAsync());
            CancelPaymentCommand = new RelayCommand(() => CancelInput());
            EnableEditPayment = new RelayCommand<Payment>(p => EditPaymentAsync(p!), p => p != null);

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

        public async Task LoadDataAsync()
        {
            try
            {
                LogInformation("Loading payment data");
                Debug.WriteLine("PaymentViewModel: LoadDataAsync started");
                var invoices = await _invoiceService.GetAllAsync();
                var payments = await _paymentService.GetAllAsync();

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

        private async void LoadData()
        {
            var payments = await _paymentService.GetAllAsync();
            Payments = new ObservableCollection<Payment>(payments.Where(p => p.InvoiceId == CurrentInvoice.Id));
            //RemainingAmount = 10000000;
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
                RemainingAmount -= Amount;
                CurrentInvoice.TotalAmount -= Amount; // Update invoice total amount
                await _invoiceService.UpdateAsync(CurrentInvoice); // Save updated invoice
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

                    SelectedPayment.PaymentDate = PaymentDate;
                    SelectedPayment.Amount = Amount;
                    SelectedPayment.PaymentMethod = PaymentMethod;

                    RemainingAmount += SelectedPayment.Amount - Amount; // Adjust remaining amount based on changes
                    CurrentInvoice.TotalAmount += SelectedPayment.Amount - Amount; // Update invoice total amount
                    await _invoiceService.UpdateAsync(CurrentInvoice); // Save updated invoice

                    Debug.WriteLine($"Saving: SelectedPayment - ID={SelectedPayment.Id}, Amount={SelectedPayment.Amount}");
                    await _paymentService.UpdateAsync(SelectedPayment);

                    IsEditing = false;
                    ClearInputFields();
                    SelectedPayment = null; // Clear selection after saving
                    // Log user activity for audit
                    await LogUserActivityAsync("UPDATE", "Payment", SelectedPayment.Id.ToString(),
                        $"Updated payment: {SelectedPayment.Amount:C} via {SelectedPayment.PaymentMethod}");

                    LogInformation("Payment updated successfully with ID {PaymentId}", SelectedPayment.Id);
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
                    RemainingAmount += payment.Amount;

                    CurrentInvoice.TotalAmount += payment.Amount; // Update invoice total amount
                    await _invoiceService.UpdateAsync(CurrentInvoice); // Save updated invoice


                    ClearInputFields();
                    SelectedPayment = null;
                    OnCanExecuteChanged();
                    IsEditing = false;

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
            Debug.WriteLine($"CanAddPayment: Amount={Amount}, RemainingAmount={RemainingAmount}");
            return CurrentInvoice != null && Amount > 0 && RemainingAmount >= Amount && !IsEditing;
        }

        private bool CanSavePayment()
        {
            if (SelectedPayment == null)
                return false;

            // Optional: prevent saving with zero/negative amounts
            if (Amount <= 0)
                return false;

            // Optional: prevent overpaying
            if (Amount > RemainingAmount + SelectedPayment.Amount) // allow reusing the original amount
                return false;

            // Optional: disable save if no real changes
            bool hasChanges =
                SelectedPayment.Amount != Amount ||
                SelectedPayment.PaymentDate != PaymentDate ||
                SelectedPayment.PaymentMethod != PaymentMethod;

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
            if (SaveEditedPaymentCommand is RelayCommand<Payment> command2) command2.NotifyCanExecuteChanged();
            if (DeletePaymentCommand is RelayCommand<Payment> command3) command3.NotifyCanExecuteChanged();
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
                SelectedPaymentForEdit = payment;
                IsEditing = true;
                Debug.WriteLine($"EditPaymentAsync: Editing payment ID={payment.Id}, Amount={payment.Amount}");
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
            IsEditing = true;
            Debug.WriteLine($"LoadPaymentForEdit: Loaded payment ID={payment.Id}, Amount={payment.Amount}");
        }
    }
}
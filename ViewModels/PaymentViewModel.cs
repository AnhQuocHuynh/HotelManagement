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
    public class PaymentViewModel : BaseViewModel
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
            set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); }
        }

        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set
            {
                _remainingAmount = value;
                OnPropertyChanged(nameof(RemainingAmount));
                if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
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
                if (_selectedPayment == null)
                {
                    ClearInputFields();
                }
                if (SavePaymentCommand is RelayCommand<Payment> command) command.NotifyCanExecuteChanged();
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
                currentBooking = value.Booking;
                MessageBox.Show($"Current Booking: {currentBooking?.Id}", "Booking Info", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
        }

        public ICommand AddPaymentCommand { get; set; }
        public ICommand SavePaymentCommand { get; set; }
        public ICommand DeletePaymentCommand { get; set; }
        public ICommand NavigateBackCommand { get; set; }
        public ICommand LoadedCommand { get; set; }

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
                SavePaymentCommand = new RelayCommand<Payment>(_ => { }, _ => false);
                DeletePaymentCommand = new RelayCommand<Payment>(_ => { }, _ => false);
                NavigateBackCommand = new RelayCommand(() => { });
                LoadedCommand = new RelayCommand(() => { });
                return;
            }

            // Runtime initialization
            _paymentService = App.ServiceProvider?.GetRequiredService<PaymentService>() ?? throw new InvalidOperationException("PaymentService not registered");
            _invoiceService = App.ServiceProvider?.GetRequiredService<InvoiceService>() ?? throw new InvalidOperationException("InvoiceService not registered");
            _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? throw new InvalidOperationException("INavigationService not registered");
            
            InitializeViewModel();
        }

        public PaymentViewModel(Invoice invoice)
        {
            _paymentService = App.ServiceProvider?.GetRequiredService<PaymentService>() ?? throw new InvalidOperationException("PaymentService not registered");
            _invoiceService = App.ServiceProvider?.GetRequiredService<InvoiceService>() ?? throw new InvalidOperationException("InvoiceService not registered");
            _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? throw new InvalidOperationException("INavigationService not registered");
            CurrentInvoice = invoice;
 
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Payments = new ObservableCollection<Payment>();
            PaymentDate = DateTime.Now;
            PaymentMethod = PaymentMethod.Cash;

            AddPaymentCommand = new RelayCommand(async () => await AddPaymentAsync(), CanAddPayment);
            SavePaymentCommand = new RelayCommand<Payment>(async (payment) => await SavePaymentAsync(payment), CanSavePayment);
            DeletePaymentCommand = new RelayCommand<Payment>(async (payment) => await DeletePaymentAsync(payment), CanDeletePayment);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            LoadedCommand = new RelayCommand(async () => await OnLoadedAsync());

            Debug.WriteLine("PaymentViewModel: Constructor called, SavePaymentCommand initialized.");
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
            RemainingAmount = CurrentInvoice.TotalAmount - Payments.Sum(p => p.Amount);
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
                ClearInputFields();
                
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

        private async Task SavePaymentAsync(Payment payment)
        {
            Debug.WriteLine($"SavePaymentAsync called: payment={(payment != null ? $"ID={payment.Id}, Amount={payment.Amount}" : "null")}");
            try
            {
                if (payment != null && SelectedPayment != null)
                {
                    LogInformation("Updating payment with ID {PaymentId}", SelectedPayment.Id);
                    
                    SelectedPayment.PaymentDate = payment.PaymentDate;
                    SelectedPayment.Amount = payment.Amount;
                    SelectedPayment.PaymentMethod = payment.PaymentMethod;

                    Debug.WriteLine($"Saving: SelectedPayment - ID={SelectedPayment.Id}, Amount={SelectedPayment.Amount}");
                    await _paymentService.UpdateAsync(SelectedPayment);
                    
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
                    ClearInputFields();
                    SelectedPayment = null;
                    OnCanExecuteChanged();
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
            return CurrentInvoice != null && Amount > 0 && RemainingAmount >= Amount;
        }

        private bool CanSavePayment(Payment payment)
        {
            Debug.WriteLine($"CanSavePayment called: payment={(payment != null ? $"ID={payment.Id}, Amount={payment.Amount}" : "null")}, SelectedPayment={(SelectedPayment != null ? $"ID={SelectedPayment.Id}, Amount={SelectedPayment.Amount}" : "null")}");
            if (payment == null || SelectedPayment == null)
            {
                Debug.WriteLine("CanSavePayment: payment or SelectedPayment is null");
                return false;
            }
            return true;
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
            if (SavePaymentCommand is RelayCommand<Payment> command2) command2.NotifyCanExecuteChanged();
            if (DeletePaymentCommand is RelayCommand<Payment> command3) command3.NotifyCanExecuteChanged();
        }

        private void NavigateBack()
        {
            _navigationService?.NavigateTo<HotelManager.ViewModels.StaffViewModels.ReceptionistViewModel>();
        }
    }
}
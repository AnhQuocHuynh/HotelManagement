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

namespace HotelManager.ViewModels
{
    public class PaymentViewModel : BaseViewModel
    {
        private readonly PaymentService _paymentService;
        private readonly InvoiceService _invoiceService;

        private DateTime _paymentDate;
        private decimal _amount;
        private PaymentMethod _paymentMethod;
        private int _selectedInvoiceId;
        private decimal _remainingAmount;
        private ObservableCollection<Payment> _payments;
        private ObservableCollection<Invoice> _availableInvoices;
        private Payment _selectedPayment;

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

        public int SelectedInvoiceId
        {
            get => _selectedInvoiceId;
            set
            {
                _selectedInvoiceId = value;
                OnPropertyChanged(nameof(SelectedInvoiceId));
                UpdateRemainingAmount();
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
                if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<Payment> Payments
        {
            get => _payments;
            set { _payments = value; OnPropertyChanged(nameof(Payments)); }
        }

        public ObservableCollection<Invoice> AvailableInvoices
        {
            get => _availableInvoices;
            set { _availableInvoices = value; OnPropertyChanged(nameof(AvailableInvoices)); }
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

        public ICommand AddPaymentCommand { get; set; }
        public ICommand SavePaymentCommand { get; set; }
        public ICommand DeletePaymentCommand { get; set; }

        public PaymentViewModel() : this(App.ServiceProvider?.GetRequiredService<PaymentService>() ?? throw new InvalidOperationException("PaymentService not registered"),
                                         App.ServiceProvider?.GetRequiredService<InvoiceService>() ?? throw new InvalidOperationException("InvoiceService not registered"))
        {
        }

        public PaymentViewModel(PaymentService paymentService, InvoiceService invoiceService)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Payments = new ObservableCollection<Payment>();
            AvailableInvoices = new ObservableCollection<Invoice>();
            PaymentDate = DateTime.Now;
            PaymentMethod = PaymentMethod.Cash;

            AddPaymentCommand = new RelayCommand(async () => await AddPaymentAsync(), CanAddPayment);
            SavePaymentCommand = new RelayCommand<Payment>(async (payment) => await SavePaymentAsync(payment), CanSavePayment);
            DeletePaymentCommand = new RelayCommand<Payment>(async (payment) => await DeletePaymentAsync(payment), CanDeletePayment);

            Debug.WriteLine("PaymentViewModel: Constructor called, SavePaymentCommand initialized.");
        }

        public async Task LoadDataAsync()
        {
            try
            {
                LogInformation("Loading payment data");
                Debug.WriteLine("PaymentViewModel: LoadDataAsync started");
                var invoices = await _invoiceService.GetAllAsync();
                var payments = await _paymentService.GetAllAsync();

                AvailableInvoices.Clear();
                foreach (var invoice in invoices)
                {
                    var totalPaid = payments.Where(p => p.InvoiceId == invoice.Id).Sum(p => p.Amount);
                    if (invoice.TotalAmount - totalPaid > 0)
                    {
                        AvailableInvoices.Add(invoice);
                    }
                }

                Payments.Clear();
                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                LogInformation("Successfully loaded {InvoiceCount} invoices and {PaymentCount} payments", 
                    AvailableInvoices.Count, Payments.Count);
                Debug.WriteLine($"PaymentViewModel: LoadDataAsync completed - {AvailableInvoices.Count} invoices, {Payments.Count} payments");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to load payment data");
                Debug.WriteLine($"PaymentViewModel: LoadDataAsync error - {ex.Message}");
                throw;
            }
        }

        private async Task AddPaymentAsync()
        {
            try
            {
                LogInformation("Adding new payment: Amount {Amount:C}, Method {PaymentMethod}, InvoiceId {InvoiceId}", 
                    Amount, PaymentMethod, SelectedInvoiceId);
                Debug.WriteLine("PaymentViewModel: AddPaymentAsync started");
                
                var payment = new Payment
                {
                    PaymentDate = PaymentDate,
                    Amount = Amount,
                    PaymentMethod = PaymentMethod,
                    InvoiceId = SelectedInvoiceId
                };
                
                await _paymentService.CreateAsync(payment);
                Payments.Add(payment);
                await UpdateDataAfterChange(SelectedInvoiceId);
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
                    await UpdateDataAfterChange(SelectedPayment.InvoiceId);
                    
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
                    await UpdateDataAfterChange(payment.InvoiceId);
                    ClearInputFields();
                    SelectedPayment = null;
                    
                    // Log user activity for audit
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

        private async Task UpdateDataAfterChange(int invoiceId)
        {
            await _paymentService.UpdateRemainingAmountAsync(invoiceId);
            var payments = await _paymentService.GetAllAsync();
            var invoices = await _invoiceService.GetAllAsync();

            Payments.Clear();
            foreach (var payment in payments)
            {
                var invoice = invoices.FirstOrDefault(i => i.Id == payment.InvoiceId);
                if (invoice != null)
                {
                    var totalPaid = payments.Where(p => p.InvoiceId == payment.InvoiceId).Sum(p => p.Amount);
                    payment.RemainingAmount = invoice.TotalAmount - totalPaid;
                }
                Payments.Add(payment);
            }

            AvailableInvoices.Clear();
            foreach (var invoice in invoices)
            {
                var totalPaid = payments.Where(p => p.InvoiceId == invoice.Id).Sum(p => p.Amount);
                if (invoice.TotalAmount - totalPaid > 0)
                {
                    AvailableInvoices.Add(invoice);
                }
            }

            UpdateRemainingAmount();
        }

        private bool CanAddPayment()
        {
            Debug.WriteLine($"CanAddPayment: Amount={Amount}, SelectedInvoiceId={SelectedInvoiceId}, RemainingAmount={RemainingAmount}");
            return Amount > 0 && SelectedInvoiceId > 0 && Amount <= RemainingAmount;
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

        private void UpdateRemainingAmountForPayments()
        {
            if (SelectedInvoiceId > 0)
            {
                var invoice = AvailableInvoices.FirstOrDefault(i => i.Id == SelectedInvoiceId);
                if (invoice != null)
                {
                    var totalPaid = Payments.Where(p => p.InvoiceId == SelectedInvoiceId).Sum(p => p.Amount);
                    RemainingAmount = invoice.TotalAmount - totalPaid;
                    foreach (var payment in Payments.Where(p => p.InvoiceId == SelectedInvoiceId))
                    {
                        payment.RemainingAmount = RemainingAmount;
                    }
                }
            }
            else
            {
                RemainingAmount = 0;
            }
            if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
        }

        private void UpdateRemainingAmount()
        {
            if (SelectedInvoiceId > 0)
            {
                var invoice = AvailableInvoices.FirstOrDefault(i => i.Id == SelectedInvoiceId);
                if (invoice != null)
                {
                    var totalPaid = Payments.Where(p => p.InvoiceId == SelectedInvoiceId).Sum(p => p.Amount);
                    RemainingAmount = invoice.TotalAmount - totalPaid;
                }
            }
            else
            {
                RemainingAmount = 0;
            }
            if (AddPaymentCommand is RelayCommand command) command.NotifyCanExecuteChanged();
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


    }
}
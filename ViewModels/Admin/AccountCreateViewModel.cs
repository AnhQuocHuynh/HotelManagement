using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using HotelManager.Helpers;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace HotelManager.ViewModels.Admin
{
    public class AccountCreateViewModel : ValidatableBase
    {
        private readonly UserAccountService _userAccountService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AccountCreateViewModel> _logger;

        private readonly Dictionary<string, string> _errors = new();
        public string UsernameError => GetError(nameof(Username));
        public string PasswordError => GetError(nameof(Password));
        public string ConfirmPasswordError => GetError(nameof(ConfirmPassword));

        // Event for successful account creation
        public event Action? AccountCreatedSuccessfully;

        public Employee Employee { get; set; }
        
        private Action? _closeAction;
        public Action? CloseAction 
        { 
            get => _closeAction;
            set
            {
                _closeAction = value;
                Console.WriteLine($"AccountCreateViewModel: CloseAction set to {(value != null ? "non-null" : "null")}");
            }
        }

        private bool? _dialogResult;
        public bool? DialogResult 
        { 
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set 
            { 
                _username = value; 
                OnPropertyChanged();
                ValidateProperty(value, nameof(Username));
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set 
            { 
                _password = value; 
                //_logger?.LogDebug("Password set to: {Password}", value);
                OnPropertyChanged();
                ValidateProperty(value, nameof(Password));
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set 
            { 
                _confirmPassword = value; 
                _logger?.LogDebug("ConfirmPassword set to: {ConfirmPassword}", value);
                OnPropertyChanged();
                ValidateProperty(value, nameof(ConfirmPassword));
            }
        }

        private UserRole _selectedRole = UserRole.Staff;
        public UserRole SelectedRole
        {
            get => _selectedRole;
            set 
            { 
                _selectedRole = value; 
                OnPropertyChanged();
            }
        }

        public List<UserRole> AvailableRoles { get; } = new List<UserRole>
        {
            UserRole.Staff,
            UserRole.Manager,
            UserRole.Admin
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AccountCreateViewModel(Employee employee, UserAccountService userAccountService, 
            INotificationService notificationService, ILogger<AccountCreateViewModel> logger)
        {
            Employee = employee;
            _userAccountService = userAccountService;
            _notificationService = notificationService;
            _logger = logger;

            // Auto-fill Username from Employee.Email if possible
            if (string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Employee?.Email))
            {
                var atIdx = Employee.Email.IndexOf('@');
                if (atIdx > 0)
                {
                    Username = Employee.Email.Substring(0, atIdx);
                }
            }

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);

            Console.WriteLine("AccountCreateViewModel: Constructor called");
        }

        private string GetError(string propertyName) =>
            _errors.TryGetValue(propertyName, out var message) ? message : string.Empty;

        public void ForcePasswordUpdate(string password, string confirmPassword)
        {
            Password = password;
            ConfirmPassword = confirmPassword;
            ValidateProperty(password, nameof(Password));
            ValidateProperty(confirmPassword, nameof(ConfirmPassword));
        }

        private bool ValidateAllProperties()
        {
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(Username))
                _errors[nameof(Username)] = "Username is required";

            if (string.IsNullOrEmpty(Password))
                _errors[nameof(Password)] = "Password is required";

            if (!string.IsNullOrEmpty(Password) && Password.Length < 6)
                _errors[nameof(Password)] = "Password must be at least 6 characters";

            if (Password != ConfirmPassword)
                _errors[nameof(ConfirmPassword)] = "Passwords do not match";

            OnPropertyChanged(nameof(UsernameError));
            OnPropertyChanged(nameof(PasswordError));
            OnPropertyChanged(nameof(ConfirmPasswordError));

            if (_errors.Any())
            {
                _notificationService?.ShowError(string.Join(Environment.NewLine, _errors.Values));
                return false;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!ValidateAllProperties())
                    return;

                // Check if employee already has an account
                
                var existingAccounts = await _userAccountService.GetAllAsync();
                var existingByEmployee = existingAccounts.FirstOrDefault(x => x.EmployeeId == Employee.Id);
                if (existingByEmployee != null)
                {
                    MessageBox.Show("Employee already has an account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _notificationService?.ShowError("This employee already has an account!");
                    return;
                }

                if (existingAccounts.Any(x => x.Username.Equals(Username, StringComparison.OrdinalIgnoreCase)))
                {
                    _notificationService?.ShowError("Username already exists!");
                    return;
                }

                var userAccount = new UserAccount
                {
                    Username = Username,
                    PasswordHash = HashHelper.HashPassword(Password),
                    Role = SelectedRole,
                    EmployeeId = Employee.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _userAccountService.CreateAsync(userAccount);
                
                _notificationService?.ShowSuccess($"Account '{Username}' has been created successfully!");
                _logger?.LogInformation("Account created successfully for employee {EmployeeId} with username {Username}", Employee.Id, Username);
                
                // Raise the success event
                AccountCreatedSuccessfully?.Invoke();
                
                DialogResult = true;
                Console.WriteLine("AccountCreateViewModel: Setting DialogResult to true and calling CloseAction");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating account for employee {EmployeeId}", Employee.Id);
                _notificationService?.ShowError($"Error creating account: {ex.Message}");
            }
        }

        private void Cancel()
        {
            Console.WriteLine("AccountCreateViewModel: Cancel method called");
            DialogResult = false;
            Console.WriteLine($"AccountCreateViewModel: DialogResult set to false, CloseAction is {(CloseAction != null ? "not null" : "null")}");
            if (CloseAction != null)
            {
                Console.WriteLine("AccountCreateViewModel: Calling CloseAction");
                CloseAction.Invoke();
                Console.WriteLine("AccountCreateViewModel: CloseAction called successfully");
            }
            else
            {
                Console.WriteLine("AccountCreateViewModel: CloseAction is null, cannot close dialog");
            }
        }
    }
} 
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
        public Action? CloseAction { get; set; }
        public bool? DialogResult { get; set; }

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

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);

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
            DialogResult = false;
            CloseAction?.Invoke();
        }
    }
} 
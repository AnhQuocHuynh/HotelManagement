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

namespace HotelManager.ViewModels.Admin
{
    public class AccountCreateViewModel : ValidatableBase
    {
        private readonly UserAccountService _userAccountService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AccountCreateViewModel> _logger;
        
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

            SetupValidation();
        }

        private void SetupValidation()
        {
            // Simple validation setup without ValidatableBase methods
            // We'll handle validation manually in SaveAsync
        }

        private bool ValidateAllProperties()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required");

            if (Password != ConfirmPassword)
                errors.Add("Passwords do not match");

            // UserRole default value is Staff (0), so we check if it's been explicitly set
            // For validation, we might want to ensure all roles are valid choices
            // Since all enum values are valid, we don't need to validate SelectedRole
            
            if (errors.Any())
            {
                _notificationService?.ShowError(string.Join(Environment.NewLine, errors));
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

                var existingAccounts = await _userAccountService.GetAllAsync();
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
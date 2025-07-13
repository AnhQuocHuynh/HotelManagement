using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HotelManager.Extensions;
using HotelManager.Models.Enums;
using HotelManager.Models;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Services;
using HotelManager.Helpers;

namespace HotelManager.ViewModels
{
    internal class EmployeeEditViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        public IEnumerable<EmployeePosition> Positions { get; } = EnumHelper.EmployeePositions;
        public Employee EditableEmployee { get; set; }
        private readonly Employee _originalEmp;

        private readonly Dictionary<string, string> _errors = new();

        public string FullNameError => GetError(nameof(EditableEmployee.FullName));
        public string EmailError => GetError(nameof(EditableEmployee.Email));
        public string PhoneNumberError => GetError(nameof(EditableEmployee.PhoneNumber));
        public string CCCDError => GetError(nameof(EditableEmployee.CCCD));

        private string GetError(string propertyName) =>
            _errors.TryGetValue(propertyName, out var msg) ? msg : string.Empty;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action CloseAction { get; set; } // Action to close the dialog if needed
        
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

        public EmployeeEditViewModel(Employee employee, EmployeeService employeeService)
        {
            _originalEmp = employee;
            EditableEmployee = new Employee
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                CCCD = employee.CCCD,
                HireDate = employee.HireDate,
                Position = employee.Position,
                UserAccount = employee.UserAccount
            };

            _employeeService = employeeService;
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);
        }

        private async Task<bool> ValidateAllAsync()
        {
            _errors.Clear();

            var e = EditableEmployee;

            if (string.IsNullOrWhiteSpace(e.FullName))
                _errors[nameof(e.FullName)] = "Full name is required.";

            if (string.IsNullOrWhiteSpace(e.Email))
                _errors[nameof(e.Email)] = "Email is required.";
            else if (!e.Email.IsValidEmail())
                _errors[nameof(e.Email)] = "Invalid email format.";

            if (string.IsNullOrWhiteSpace(e.PhoneNumber))
                _errors[nameof(e.PhoneNumber)] = "Phone number is required.";
            else if (!e.PhoneNumber.IsValidPhoneNumber())
                _errors[nameof(e.PhoneNumber)] = "Invalid phone number format.";

            if (string.IsNullOrWhiteSpace(e.CCCD))
                _errors[nameof(e.CCCD)] = "ID number (CCCD) is required.";
            else if (!System.Text.RegularExpressions.Regex.IsMatch(e.CCCD, @"^\d{12}$"))
                _errors[nameof(e.CCCD)] = "CCCD must be exactly 12 digits.";

            // Uniqueness check (excluding current)
            var all = await _employeeService.GetAllAsync();

            if (all.Any(emp => emp.Id != _originalEmp.Id && emp.Email.Equals(e.Email, StringComparison.OrdinalIgnoreCase)))
                _errors[nameof(e.Email)] = "This email is already in use.";

            if (all.Any(emp => emp.Id != _originalEmp.Id && emp.CCCD == e.CCCD))
                _errors[nameof(e.CCCD)] = "This ID number (CCCD) is already in use.";

            // Notify bindings
            OnPropertyChanged(nameof(FullNameError));
            OnPropertyChanged(nameof(EmailError));
            OnPropertyChanged(nameof(PhoneNumberError));
            OnPropertyChanged(nameof(CCCDError));

            return !_errors.Any();
        }

        private async Task SaveAsync()
        {
            if(!await ValidateAllAsync())
            {
                return;
            }

            // Apply changes to the original object
            _originalEmp.FullName = EditableEmployee.FullName;
            _originalEmp.Email = EditableEmployee.Email;
            _originalEmp.PhoneNumber = EditableEmployee.PhoneNumber;
            _originalEmp.CCCD = EditableEmployee.CCCD;
            _originalEmp.HireDate = EditableEmployee.HireDate;
            _originalEmp.Position = EditableEmployee.Position;

            await _employeeService.UpdateAsync(_originalEmp);
            MessageBox.Show("Employee updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true; // Indicate success
            Console.WriteLine("EmployeeEditViewModel: Setting DialogResult to true and calling CloseAction");
            CloseAction?.Invoke(); // Close the dialog if applicable
        }

        private void Cancel()
        {
            DialogResult = false; // Indicate cancellation
            Console.WriteLine("EmployeeEditViewModel: Setting DialogResult to false and calling CloseAction");
            CloseAction?.Invoke(); // Close the dialog if applicable
        }
    }
}

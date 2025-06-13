using HotelManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Services;
using RelayCommand = HotelManager.Utilities.RelayCommand;
using HotelManager.Helpers;
using HotelManager.Models.Enums;
using HotelManager.Extensions;
using System.Windows;

namespace HotelManager.ViewModels
{
    //Tuấn
    //Todo: 1. Hiển thị danh sách tài khoản, 2. Tạo tài khoản mới, 3. Sửa tài khoản, 4. Xóa tài khoản, 5. Ràng buộc phân quyền
    internal class AdminViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private readonly DialogService _dialogService = new DialogService();
        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddNewEmployeeCommand { get; set; }

        private ObservableCollection<Employee> _employees = new();
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employees));
            }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));
            }
        }

        //New employee data getter
        public IEnumerable<EmployeePosition> EmployeePositions { get; } = EnumHelper.EmployeePositions; //To be fixed

        private EmployeePosition _selectedPosition;
        public EmployeePosition SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                _selectedPosition = value;
                OnPropertyChanged(nameof(SelectedPosition));
            }
        }

        private String _employeeFullName;
        public String EmployeeFullName
        {
            get => _employeeFullName;
            set
            {
                _employeeFullName = value;
                OnPropertyChanged(nameof(EmployeeFullName));
            }
        }
        private String _employeeEmail;
        public String EmployeeEmail
        {
            get => _employeeEmail;
            set
            {
                _employeeEmail = value;
                OnPropertyChanged(nameof(EmployeeEmail));
            }
        }
        private String _employeePhoneNumber;
        public String EmployeePhoneNumber
        {
            get => _employeePhoneNumber;
            set
            {
                _employeePhoneNumber = value;
                OnPropertyChanged(nameof(EmployeePhoneNumber));
            }
        }
        private DateTime _employeeHireDate = DateTime.Today;
        public DateTime EmployeeHireDate
        {
            get => _employeeHireDate;
            set
            {
                _employeeHireDate = value;
                OnPropertyChanged(nameof(EmployeeHireDate));
            }
        }


        public AdminViewModel()
        {
            _employeeService = new EmployeeService(new HotelDbContext());
            AddCommand = new RelayCommand(async param => await AddAsync(SelectedEmployee));
            UpdateCommand = new RelayCommand(async param => Update(SelectedEmployee));
            DeleteCommand = new RelayCommand(async param => await DeleteAsync(SelectedEmployee));
            AddNewEmployeeCommand = new RelayCommand(param => AddNewEmployee());

            LoadEmployees();
            //SetDummyEmployees();
        }
        public AdminViewModel(IService<Employee> employeeService)
        {
            _employeeService = (EmployeeService)employeeService;

            AddCommand = new RelayCommand(async param => await AddAsync(SelectedEmployee));
            UpdateCommand = new RelayCommand(async param => Update(SelectedEmployee));
            DeleteCommand = new RelayCommand(async param => await DeleteAsync(SelectedEmployee));
            AddNewEmployeeCommand = new RelayCommand(param => AddNewEmployee());

            LoadEmployees();
            //SetDummyEmployees();
        }
        public async void LoadEmployees()
        {
            var employees = await _employeeService.GetAllAsync();
            Employees.Clear(); 
            foreach (var emp in employees)
                Employees.Add(emp); //Trigger UI update
        }

    //    private void SetDummyEmployees()
    //    {
    //        var dummyEmployees = new List<(string? Username, string FullName, string Email, string Phone, EmployeePosition Position)>
    //{
    //    ("user1", "Nguyen Test 1", "user1@example.com", "0901111111", EmployeePosition.Receptionist),
    //    (null, "Nguyen Test 2", "user2@example.com", "0902222222", EmployeePosition.Technician),
    //    ("user3", "Nguyen Test 3", "user3@example.com", "0903333333", EmployeePosition.Receptionist)
    //};

    //        foreach (var (username, fullName, email, phone, position) in dummyEmployees)
    //        {
    //            var employee = new Employee
    //            {
    //                FullName = fullName,
    //                Email = email,
    //                PhoneNumber = phone,
    //                HireDate = DateTime.UtcNow,
    //                Position = position,
    //                UserAccount = username != null ? new UserAccount
    //                {
    //                    Username = username,
    //                    PasswordHash = HashHelper.HashPassword(username),
    //                    Role = UserRole.Staff,
    //                    CreatedAt = DateTime.UtcNow,
    //                    IsActive = true
    //                } : null
    //            };

    //            Employees.Add(employee);
    //        }
    //    }
        private async Task AddAsync(Employee employee)
        {
            var added = await _employeeService.CreateAsync(employee);
            Employees.Add(added);
        }

        private void Update(Employee employee)
        {
            if(employee == null)
            {
                MessageBox.Show("Vui lòng chọn 1 nhân viên để chỉnh sửa!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editVM = new EmployeeEditViewModel(employee);
            var result = _dialogService.ShowDialog(editVM);

            if (result == true)
            {
                LoadEmployees(); // refresh data if needed
            }
        }

        private async Task DeleteAsync(Employee employee)
        {
            Employees.Remove(employee);
            await _employeeService.DeleteAsync(employee.Id);
        }

        private void AddNewEmployee()
        {
            try
            {
                //Validate employee details
                if (String.IsNullOrEmpty(_employeeFullName) || String.IsNullOrEmpty(_employeeEmail)
                    || String.IsNullOrEmpty(_employeePhoneNumber))
                {
                    throw new ArgumentException("Employee details cannot be empty.");
                }
                //Validate email
                if (StringExtensions.IsValidEmail(_employeeEmail) == false)
                {
                    throw new ArgumentException("Invalid email format.");
                }
                //Validate phone number
                if (StringExtensions.IsValidPhoneNumber(_employeePhoneNumber) == false)
                {
                    throw new ArgumentException("Invalid phone number format.");
                }
                //validate Uniqueness via email
                bool emailExists = Employees.Any(e => e.Email.Equals(_employeeEmail, StringComparison.OrdinalIgnoreCase));
                if (emailExists)
                {
                    throw new InvalidOperationException("An employee with this email already exists.");
                }

                var newEmployee = new Employee
                {
                    FullName = _employeeFullName,
                    Email = _employeeEmail,
                    PhoneNumber = _employeePhoneNumber,
                    HireDate = _employeeHireDate,
                    Position = _selectedPosition,
                    UserAccount = null
                };

                MessageBox.Show("Employee added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                AddAsync(newEmployee).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Handle error
                        Console.WriteLine($"Error adding employee: {task.Exception?.Message}");
                    }
                    else
                    {
                        // Successfully added
                        Console.WriteLine("Employee added successfully.");
                    }
                });
            
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        
    }

}

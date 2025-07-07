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
using HotelManager.Helpers;
using HotelManager.Models.Enums;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HotelManager.ViewModels.Common;
using HotelManager.ViewModels.Admin;
using HotelManager.Extensions;
using HotelManager.Interfaces;

namespace HotelManager.ViewModels.Admin
{
    //Tuấn
    //Todo: 1. Hiển thị danh sách tài khoản, 2. Tạo tài khoản mới, 3. Sửa tài khoản, 4. Xóa tài khoản, 5. Ràng buộc phân quyền
    internal class AdminViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddNewEmployeeCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand CreateAccountCommand { get; set; }
        public ICommand ReportsCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ClearFormCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand NavigateRoomManagementCommand { get; set; }
        public ICommand NavigateProfileCommand { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<Employee> _employees = new();
        //_allEmployees should be used to store all employees for search/filtering purposes
        private List<Employee> _allEmployees = new();
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
                ((RelayCommand)CreateAccountCommand).NotifyCanExecuteChanged();
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

        private string _employeeFullName;
        public string EmployeeFullName
        {
            get => _employeeFullName;
            set
            {
                _employeeFullName = value;
                OnPropertyChanged(nameof(EmployeeFullName));
            }
        }
        private string _employeeEmail;
        public string EmployeeEmail
        {
            get => _employeeEmail;
            set
            {
                _employeeEmail = value;
                OnPropertyChanged(nameof(EmployeeEmail));
            }
        }
        private string _employeePhoneNumber;
        public string EmployeePhoneNumber
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
        private string _employeeCCCD;
        public string EmployeeCCCD
        {
            get => _employeeCCCD;
            set { _employeeCCCD = value; OnPropertyChanged(nameof(EmployeeCCCD)); }
        }

        private string _greeting = "Hello, Admin";
        public string Greeting
        {
            get => _greeting;
            set { _greeting = value; OnPropertyChanged(nameof(Greeting)); }
        }

        public AdminViewModel(EmployeeService employeeService, IDialogService dialogService,
            INotificationService notificationService, ILogger<AdminViewModel> logger,
            IServiceProvider serviceProvider, IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _employeeService = employeeService;
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
            _navigationService = navigationService;
            AddCommand = new AsyncRelayCommand<Employee>(AddAsync);
            UpdateCommand = new RelayCommand<Employee>(Update);
            DeleteCommand = new AsyncRelayCommand<Employee>(DeleteAsync);
            AddNewEmployeeCommand = new RelayCommand(AddNewEmployee);
            LogoutCommand = new RelayCommand(Logout);
            CreateAccountCommand = new RelayCommand(CreateAccount, CanCreateAccount);
            ReportsCommand = new RelayCommand(Reports);
            RefreshCommand = new RelayCommand(Refresh);
            ClearFormCommand = new RelayCommand(ClearForm);
            SearchCommand = new RelayCommand(PerformSearch);
            NavigateRoomManagementCommand = new RelayCommand(NavigateRoomManagement);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);

            LoadEmployees();

            var user = AppSession.GetCurrentUserAccount();
            if (user != null)
            {
                Greeting = $"Hello, {user.Username}";
            }
        }

        public AdminViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Initialize with design-time data
                Greeting = "Hello, Admin (Design Mode)";
                Employees = new ObservableCollection<Employee>();
                EmployeePositions = EnumHelper.EmployeePositions;

                // Initialize commands with empty implementations for design-time
                AddCommand = new AsyncRelayCommand<Employee>(_ => Task.CompletedTask);
                UpdateCommand = new RelayCommand<Employee>(_ => { });
                DeleteCommand = new AsyncRelayCommand<Employee>(_ => Task.CompletedTask);
                AddNewEmployeeCommand = new RelayCommand(() => { });
                LogoutCommand = new RelayCommand(() => { });
                CreateAccountCommand = new RelayCommand(() => { }, () => false);
                ReportsCommand = new RelayCommand(() => { });
                RefreshCommand = new RelayCommand(() => { });
                ClearFormCommand = new RelayCommand(() => { });
                NavigateRoomManagementCommand = new RelayCommand(() => { });
                NavigateProfileCommand = new RelayCommand(() => { });
                return;
            }

            // Runtime initialization
            _employeeService = App.ServiceProvider?.GetRequiredService<EmployeeService>() ?? throw new InvalidOperationException("EmployeeService not registered");
            _dialogService = App.ServiceProvider?.GetRequiredService<IDialogService>() ?? throw new InvalidOperationException("DialogService not registered");
            _notificationService = App.ServiceProvider?.GetRequiredService<INotificationService>() ?? throw new InvalidOperationException("NotificationService not registered");
            _logger = App.ServiceProvider?.GetRequiredService<ILogger<AdminViewModel>>() ?? throw new InvalidOperationException("Logger not registered");
            _serviceProvider = App.ServiceProvider;
            _unitOfWork = App.ServiceProvider.GetRequiredService<IUnitOfWork>();
            _navigationService = App.ServiceProvider.GetRequiredService<INavigationService>();

            InitializeCommands();

            LoadEmployees();

            var user = AppSession.GetCurrentUserAccount();
            if (user != null)
            {
                Greeting = $"Hello, {user.Username}";
            }
        }

        private void InitializeCommands()
        {
            AddCommand = new AsyncRelayCommand<Employee>(AddAsync);
            UpdateCommand = new RelayCommand<Employee>(Update);
            DeleteCommand = new AsyncRelayCommand<Employee>(DeleteAsync);
            AddNewEmployeeCommand = new RelayCommand(AddNewEmployee);
            LogoutCommand = new RelayCommand(Logout);
            CreateAccountCommand = new RelayCommand(CreateAccount, CanCreateAccount);
            ReportsCommand = new RelayCommand(Reports);
            RefreshCommand = new RelayCommand(Refresh);
            ClearFormCommand = new RelayCommand(ClearForm);
            SearchCommand = new RelayCommand(PerformSearch);
            NavigateRoomManagementCommand = new RelayCommand(NavigateRoomManagement);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);

        }

        public async void LoadEmployees()
        {
            var employees = await _employeeService.GetAllAsync();

            _allEmployees = employees.ToList();
            Employees.Clear();
            foreach (var emp in _allEmployees)
                Employees.Add(emp);
        }

        private async Task AddAsync(Employee employee)
        {
            var added = await _employeeService.CreateAsync(employee);
            Employees.Add(added);
        }

        private void Update(Employee employee)
        {
            if (employee == null)
            {
                MessageBox.Show("Please select an employee to edit!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editVM = new EmployeeEditViewModel(employee, _employeeService);
            var result = _dialogService.ShowDialog(editVM);

            if (result == true)
            {
                LoadEmployees(); // refresh data if needed
            }
        }

        private async Task DeleteAsync(Employee employee)
        {
            if (employee == null)
            {
                MessageBox.Show("Please select an employee to delete!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete employee {employee.FullName}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await _employeeService.DeleteAsync(employee.Id);
                Employees.Remove(employee);
                MessageBox.Show("Employee deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa nhân viên: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewEmployee()
        {
            try
            {
                //Validate employee details
                if (string.IsNullOrEmpty(_employeeFullName) || string.IsNullOrEmpty(_employeeEmail)
                    || string.IsNullOrEmpty(_employeePhoneNumber) || string.IsNullOrEmpty(_employeeCCCD))
                {
                    throw new ArgumentException("Employee details cannot be empty.");
                }
                //Validate email
                if (_employeeEmail.IsValidEmail() == false)
                {
                    throw new ArgumentException("Invalid email format.");
                }
                //Validate phone number
                if (_employeePhoneNumber.IsValidPhoneNumber() == false)
                {
                    throw new ArgumentException("Invalid phone number format.");
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(_employeeCCCD, @"^\d{12}$"))
                {
                    throw new ArgumentException("ID number (CCCD) must consist of 12 digits.");
                }
                //validate Uniqueness via email & CCCD
                bool emailExists = Employees.Any(e => e.Email.Equals(_employeeEmail, StringComparison.OrdinalIgnoreCase));
                if (emailExists)
                {
                    throw new InvalidOperationException("An employee with this email already exists.");
                }
                if (Employees.Any(e => e.CCCD == _employeeCCCD))
                {
                    throw new InvalidOperationException("An employee with this ID number already exists.");
                }

                var newEmployee = new Employee
                {
                    FullName = _employeeFullName,
                    Email = _employeeEmail,
                    PhoneNumber = _employeePhoneNumber,
                    HireDate = _employeeHireDate,
                    Position = _selectedPosition,
                    CCCD = _employeeCCCD,
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

        private void Logout()
        {
            try
            {
                // Clear session
                AppSession.Clear();

                // Invoke logout on MainViewModel to navigate to LoginView
                var mainVM = Application.Current.MainWindow?.DataContext as MainViewModel;
                mainVM?.Logout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Logout failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCreateAccount()
        {
            return SelectedEmployee != null;
        }

        private void CreateAccount()
        {
            if (SelectedEmployee == null) return;

            try
            {
                var userAccountService = new UserAccountService(_unitOfWork);
                var logger = _serviceProvider.GetService<ILogger<AccountCreateViewModel>>();

                var viewModel = new AccountCreateViewModel(
                    SelectedEmployee,
                    userAccountService,
                    _notificationService,
                    logger);

                var result = _dialogService.ShowDialog(viewModel);

                if (result == true)
                {
                    // Refresh employee list or update UI as needed
                    LoadEmployees();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening create account dialog");
                _notificationService.ShowError($"Lỗi mở dialog tạo tài khoản: {ex.Message}");
            }
        }

        private void Reports()
        {
            MessageBox.Show("Reports feature will be implemented in future versions.\n\nYou will be able to view:\n• Employee statistics\n• Room occupancy reports\n• Revenue analytics\n• Performance metrics",
                "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Refresh()
        {
            LoadEmployees();
            MessageBox.Show("Employee list refreshed successfully!", "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearForm()
        {
            EmployeeFullName = string.Empty;
            EmployeeEmail = string.Empty;
            EmployeePhoneNumber = string.Empty;
            EmployeeCCCD = string.Empty;
            SelectedPosition = EmployeePosition.Receptionist;
            EmployeeHireDate = DateTime.Today;
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Show full list if search is empty
                Employees.Clear();
                foreach (var emp in _allEmployees)
                    Employees.Add(emp);
                return;
            }

            string keyword = SearchText.Trim().ToLowerInvariant();

            //Fileter employees based on search criteria
            var filtered = _allEmployees.Where(emp =>
                (emp.FullName != null && emp.FullName.ToLower().Contains(keyword)) ||
                (emp.Email != null && emp.Email.ToLower().Contains(keyword)) ||
                (emp.PhoneNumber != null && emp.PhoneNumber.ToLower().Contains(keyword)) ||
                (emp.CCCD != null && emp.CCCD.ToLower().Contains(keyword)) ||
                emp.Position.ToString().ToLower().Contains(keyword)
            ).ToList();

            Employees.Clear();
            foreach (var emp in filtered)
                Employees.Add(emp);

        }

        private void NavigateRoomManagement()
        {
            try
            {
                _logger.LogInformation("Attempting to navigate to RoomViewModel");
                _navigationService.NavigateTo<RoomViewModel>();
                _logger.LogInformation("Successfully navigated to RoomViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to RoomViewModel");
                _notificationService?.ShowError($"Navigation error: {ex.Message}");
            }
        }

        private void NavigateProfile()
        {
            try
            {
                _logger.LogInformation("Attempting to navigate to ProfileViewModel");
                _navigationService.NavigateTo<ProfileViewModel>();
                _logger.LogInformation("Successfully navigated to ProfileViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to ProfileViewModel");
                _notificationService?.ShowError($"Navigation error: {ex.Message}");
            }
        }
    }

}

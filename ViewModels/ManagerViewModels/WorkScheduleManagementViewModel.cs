using CommunityToolkit.Mvvm.Input;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Models;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.AspNetCore.Mvc.Filters;
using HotelManager.Core.Data;
using System.ComponentModel;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace HotelManager.ViewModels.ManagerViewModels
{
    /// <summary>
    /// ViewModel cho Manager quản lý lịch làm việc
    /// TODO (Bảo): Implement tất cả properties và commands với proper data binding
    /// </summary>
    public class WorkScheduleManagementViewModel : BaseViewModel
    {
        // TODO (Bảo): Inject các services cần thiết
        private readonly IWorkScheduleService _workScheduleService;
        private readonly IEmployeeService _employeeService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<WorkScheduleManagementViewModel> _logger;

        #region Properties cho Schedule Assignment

        private ObservableCollection<Employee> _employees = new();
        /// <summary>
        /// TODO (Bảo): Bind vào ComboBox để chọn nhân viên
        /// </summary>
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
        }

        private ObservableCollection<WorkSchedule> _weeklySchedules = new();
        /// <summary>
        /// TODO (Bảo): Bind vào DataGrid/Calendar control để hiển thị lịch tuần
        /// </summary>
        public ObservableCollection<WorkSchedule> WeeklySchedules
        {
            get => _weeklySchedules;
            set { _weeklySchedules = value; OnPropertyChanged(); }
        }

        private Employee? _selectedEmployee;
        /// <summary>
        /// TODO (Bảo): Two-way binding cho employee selection
        /// </summary>
        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set 
            { 
                _selectedEmployee = value; 
                OnPropertyChanged();
                // TODO (Bảo): Update CanExecute cho AssignScheduleCommand
                ((AsyncRelayCommand)AssignScheduleCommand).NotifyCanExecuteChanged();
            }
        }

        private WorkDay _selectedDay = WorkDay.Monday;
        /// <summary>
        /// TODO (Bảo): Bind vào ComboBox cho WorkDay selection
        /// </summary>
        public WorkDay SelectedDay
        {
            get => _selectedDay;
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        private WorkShift _selectedShift = WorkShift.Morning;
        /// <summary>
        /// TODO (Bảo): Bind vào ComboBox cho WorkShift selection
        /// </summary>
        public WorkShift SelectedShift
        {
            get => _selectedShift;
            set { _selectedShift = value; OnPropertyChanged(); }
        }

        private DateTime _selectedDate = DateTime.Today;
        /// <summary>
        /// TODO (Bảo): Bind vào DatePicker
        /// </summary>
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        private DateTime _selectedWeek = DateTime.Today;
        /// <summary>
        /// TODO (Bảo): Bind vào week navigation controls
        /// </summary>
        public DateTime SelectedWeek
        {
            get => _selectedWeek;
            set 
            { 
                _selectedWeek = value; 
                OnPropertyChanged();
                // TODO (Bảo): Auto-load weekly schedules khi week thay đổi
                _ = LoadWeeklySchedulesAsync();
            }
        }

        private string _assignmentNotes = string.Empty;
        /// <summary>
        /// TODO (Bảo): Bind vào TextBox cho ghi chú
        /// </summary>
        public string AssignmentNotes
        {
            get => _assignmentNotes;
            set { _assignmentNotes = value; OnPropertyChanged(); }
        }

        #endregion


        #region Commands

        /// <summary>
        /// TODO (Bảo): Implement command để phân công lịch làm việc
        /// </summary>
        public ICommand AssignScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để load lịch tuần
        /// </summary>
        public ICommand LoadWeeklyScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để update schedule
        /// </summary>
        public ICommand UpdateScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để delete schedule
        /// </summary>
        public ICommand DeleteScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement navigation commands cho tuần trước/sau
        /// </summary>
        public ICommand NavigatePreviousWeekCommand { get; private set; }
        public ICommand NavigateNextWeekCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để refresh data
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Command to load data when view is loaded
        /// </summary>
        public ICommand LoadedCommand { get; private set; }

        #endregion

        #region Constructor

        public WorkScheduleManagementViewModel(
            IWorkScheduleService workScheduleService,
            IEmployeeService employeeService,
            INotificationService notificationService,
            ILogger<WorkScheduleManagementViewModel> logger)
        {
            _workScheduleService = workScheduleService;
            _employeeService = employeeService;
            _notificationService = notificationService;
            _logger = logger;

            // TODO (Bảo): Initialize commands
            InitializeCommands();
        }

        // TODO (Bảo): Parameterless constructor for design-time
        public WorkScheduleManagementViewModel() : this(null!, null!, null!, null!)
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// TODO (Bảo): Initialize tất cả commands
        /// </summary>
        private async Task InitializeCommands()
        {
            AssignScheduleCommand = new AsyncRelayCommand(AssignScheduleAsync, CanAssignSchedule);
            LoadWeeklyScheduleCommand = new AsyncRelayCommand(LoadWeeklySchedulesAsync);
            UpdateScheduleCommand = new AsyncRelayCommand<WorkSchedule>(UpdateScheduleAsync);
            DeleteScheduleCommand = new AsyncRelayCommand<WorkSchedule>(DeleteScheduleAsync);
            NavigatePreviousWeekCommand = new AsyncRelayCommand(NavigateToPreviousWeek);
            NavigateNextWeekCommand = new AsyncRelayCommand(NavigateToNextWeek);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);
            LoadedCommand = new AsyncRelayCommand(LoadInitialDataAsync);

            // TODO (Bảo): Load initial data
            await LoadInitialDataAsync();
        }

        /// <summary>
        /// TODO (Bảo): Implement validation cho assignment
        /// </summary>
        private bool CanAssignSchedule()
        {
            // TODO: Check if employee selected và basic validation
            return SelectedEmployee != null;
        }

        /// <summary>
        /// TODO (Bảo): Implement schedule assignment với error handling
        /// </summary>
        private async Task AssignScheduleAsync()
        {
            try
            {
                // TODO (Bảo): Implement assignment logic
                // 1. Validate input
                bool isConfilct = await _workScheduleService.ValidateScheduleConflictAsync(
                    SelectedEmployee.Id,
                    SelectedDay,
                    SelectedShift,
                    SelectedDate
                );

                if (isConfilct)
                {
                    _notificationService?.ShowError("Lịch làm việc đã có xung đột. Vui lòng chọn lại.");
                    return;
                }

                // 2. Call _workScheduleService.AssignScheduleAsync
                await _workScheduleService.AssignScheduleAsync(
                    SelectedEmployee.Id,
                    SelectedDay,
                    SelectedShift,
                    SelectedDate,
                    null, // End date can be null for single day assignments
                    0, // Assigned by employee ID (can be set to current user)
                    AssignmentNotes
                );

                // 3. Refresh UI
                await RefreshDataAsync();

                // 4. Show success notification
                _notificationService?.ShowSuccess("Thêm lịch làm việc thành công");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assigning schedule");
                _notificationService?.ShowError("Lỗi phân công lịch làm việc");
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement load weekly schedules
        /// </summary>
        private async Task LoadWeeklySchedulesAsync()
        {
            try
            {
                if (_workScheduleService == null) return; // design-time

                // Tính ngày đầu tuần (thứ 2)
                int diff = (7 + (SelectedWeek.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime weekStart = SelectedWeek.AddDays(-diff).Date;

                var result = await _workScheduleService.GetWeeklyScheduleAsync(weekStart);
                WeeklySchedules = new ObservableCollection<WorkSchedule>(result);

                if (WeeklySchedules.Count == 0)
                    _notificationService?.ShowInfo("Không có lịch làm việc cho tuần này");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading weekly schedules");
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement update schedule
        /// </summary>
        private async Task UpdateScheduleAsync(WorkSchedule? schedule)
        {
            if (_workScheduleService == null) return;

            // TODO (Bảo): Implement update logic            
            bool isConfilct = await _workScheduleService.ValidateScheduleConflictAsync(
                SelectedEmployee.Id,
                SelectedDay,
                SelectedShift,
                SelectedDate
                );

            if (isConfilct)
            {
                _notificationService?.ShowError("Lịch làm việc đã có xung đột. Vui lòng chọn lại.");
                return;
            }

            schedule = await _workScheduleService.UpdateAsync(
                new WorkSchedule
                {
                    EmployeeId = SelectedEmployee.Id,
                    WorkDay = SelectedDay,
                    Shift = SelectedShift,
                    StartDate = SelectedDate,
                    Notes = AssignmentNotes
                }
            );

            await LoadWeeklySchedulesAsync();

            _notificationService.ShowSuccess("Thay đổi lịch làm việc thành công");
        }

        /// <summary>
        /// TODO (Bảo): Implement delete schedule với confirmation
        /// </summary>
        private async Task DeleteScheduleAsync(WorkSchedule? schedule)
        {
            if (_workScheduleService == null) return;

            // TODO (Bảo): Implement delete với user confirmation
            _notificationService.ShowActionSnackbar(
                "Bạn có chắc chắn muốn xóa lịch làm việc này?",
                "Xóa",
                async () =>
                {
                    if (schedule != null)
                    {
                        try
                        {
                            // Call service to delete schedule
                            await _workScheduleService.DeleteAsync(schedule.Id);
                            _notificationService.ShowSuccess("Đã xóa lịch làm việc thành công.");
                            // Refresh data after deletion
                            await LoadWeeklySchedulesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error deleting schedule");
                            _notificationService.ShowError("Lỗi khi xóa lịch làm việc.");
                        }
                    }
                }
            );
        }

        /// <summary>
        /// TODO (Bảo): Implement week navigation
        /// </summary>
        private async Task NavigateToPreviousWeek()
        {
            SelectedWeek = SelectedWeek.AddDays(-7);
        }

        private async Task NavigateToNextWeek()
        {
            SelectedWeek = SelectedWeek.AddDays(7);
        }

        /// <summary>
        /// TODO (Bảo): Implement data refresh
        /// </summary>
        private async Task RefreshDataAsync()
        {
            try
            {
                if (_workScheduleService == null || _employeeService == null) 
                {
                    _logger?.LogWarning("Services are null during RefreshDataAsync - design-time mode");
                    return; // design-time
                }

                //Employees = new ObservableCollection<Employee>(await _employeeService.GetAllAsync());
                //WeeklySchedules = new ObservableCollection<WorkSchedule>(await _workScheduleService.GetWeeklyScheduleAsync(SelectedWeek));

                //_notificationService.ShowSuccess("làm mới thành công");

                _logger?.LogInformation("Starting RefreshDataAsync...");

                // Lấy tuần hiện tại (bắt đầu thứ 2)
                int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
                SelectedWeek = DateTime.Today.AddDays(-diff).Date;

                _logger?.LogInformation("Loading employees from database...");
                var employees = await _employeeService.GetAllAsync();
                _logger?.LogInformation($"Loaded {employees?.Count() ?? 0} employees from database");
                
                Employees = new ObservableCollection<Employee>(employees ?? new List<Employee>());
                
                _logger?.LogInformation("Loading weekly schedules...");
                var schedules = await _workScheduleService.GetWeeklyScheduleAsync(SelectedWeek);
                _logger?.LogInformation($"Loaded {schedules?.Count() ?? 0} schedules for week {SelectedWeek:yyyy-MM-dd}");
                
                WeeklySchedules = new ObservableCollection<WorkSchedule>(schedules ?? new List<WorkSchedule>());

                _notificationService?.ShowInfo($"Đã tải {Employees.Count} nhân viên và {WeeklySchedules.Count} lịch làm việc");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in RefreshDataAsync");
                _notificationService?.ShowError($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement initial data loading
        /// </summary>
        private async Task LoadInitialDataAsync()
        {
            try
            {
                if (_workScheduleService == null || _employeeService == null)
                {
                    _logger?.LogWarning("Services are null during LoadInitialDataAsync - design-time mode");
                    // Design-time sample only
                    return;
                }

                _logger?.LogInformation("Starting LoadInitialDataAsync...");
                await RefreshDataAsync();
                _logger?.LogInformation("LoadInitialDataAsync completed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in LoadInitialDataAsync");
                _notificationService?.ShowError($"Lỗi khởi tạo dữ liệu: {ex.Message}");
            }
        }

        #endregion

        // Helper collections for XAML binding
        public IEnumerable<WorkDay> WorkDayValues => Enum.GetValues(typeof(WorkDay)).Cast<WorkDay>();
        public IEnumerable<WorkShift> WorkShiftValues => Enum.GetValues(typeof(WorkShift)).Cast<WorkShift>();
    }
} 
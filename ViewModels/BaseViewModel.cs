using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using HotelManager.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace HotelManager.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected ILogger _logger;
        protected IAuditService _auditService;
        protected INotificationService _notificationService;
        protected INavigationService _navigationService;

        public ICommand LoadedCommand { get; protected set; }

        public BaseViewModel()
        {
            // Try to get services from DI container
            try
            {
                if (App.ServiceProvider != null)
                {
                    var loggerType = typeof(ILogger<>).MakeGenericType(this.GetType());
                    _logger = App.ServiceProvider.GetService(loggerType) as ILogger;
                    _auditService = (IAuditService)App.ServiceProvider.GetService(typeof(IAuditService));
                    _notificationService = (INotificationService)App.ServiceProvider.GetService(typeof(INotificationService));
                    _navigationService = (INavigationService)App.ServiceProvider.GetService(typeof(INavigationService));
                }
            }
            catch (Exception ex)
            {
                // Fallback - continue without services if DI is not available
                System.Diagnostics.Debug.WriteLine($"BaseViewModel: Could not initialize services: {ex.Message}");
            }

            LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);

            InitializeCommands();
        }

        public BaseViewModel(ILogger logger, IAuditService auditService = null, INotificationService notificationService = null, INavigationService navigationService = null)
        {
            _logger = logger;
            _auditService = auditService;
            _notificationService = notificationService;
            _navigationService = navigationService;

            LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);

            InitializeCommands();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Sets property value and raises PropertyChanged event if value changed
        /// </summary>
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #region Logging Methods

        /// <summary>
        /// Log information message
        /// </summary>
        protected void LogInformation(string message, params object[] args)
        {
            try
            {
                _logger?.LogInformation($"[{GetType().Name}] {message}", args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BaseViewModel logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        protected void LogWarning(string message, params object[] args)
        {
            try
            {
                _logger?.LogWarning($"[{GetType().Name}] {message}", args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BaseViewModel logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Log error message
        /// </summary>
        protected void LogError(Exception exception, string message, params object[] args)
        {
            try
            {
                _logger?.LogError(exception, $"[{GetType().Name}] {message}", args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BaseViewModel logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Log user activity for audit trail
        /// </summary>
        protected async Task LogUserActivityAsync(string action, string entity, string entityId, string details = null)
        {
            try
            {
                if (_auditService != null)
                {
                    await _auditService.LogUserActivityAsync(null, action, entity, entityId, details);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to log user activity");
            }
        }

        #endregion

        #region Notification Methods

        /// <summary>
        /// Show info notification
        /// </summary>
        protected void ShowInfo(string message)
        {
            _notificationService?.ShowInfo(message);
        }

        /// <summary>
        /// Show success notification
        /// </summary>
        protected void ShowSuccess(string message)
        {
            _notificationService?.ShowSuccess(message);
        }

        /// <summary>
        /// Show warning notification
        /// </summary>
        protected void ShowWarning(string message)
        {
            _notificationService?.ShowWarning(message);
        }

        /// <summary>
        /// Show error notification
        /// </summary>
        protected void ShowError(string message)
        {
            _notificationService?.ShowError(message);
        }

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        protected async Task<bool> ShowConfirmAsync(string title, string message)
        {
            if (_notificationService != null)
            {
                return await _notificationService.ShowConfirmAsync(title, message);
            }
            return false;
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigate to ViewModel
        /// </summary>
        protected void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            _navigationService?.NavigateTo<TViewModel>();
        }

        /// <summary>
        /// Navigate to ViewModel with parameter
        /// </summary>
        protected void NavigateTo<TViewModel>(object parameter) where TViewModel : BaseViewModel
        {
            _navigationService?.NavigateTo<TViewModel>(parameter);
        }

        /// <summary>
        /// Go back to previous ViewModel
        /// </summary>
        protected void GoBack()
        {
            _navigationService?.GoBack();
        }

        #endregion

        #region Command Helpers

        /// <summary>
        /// Creates a RelayCommand
        /// </summary>
        protected RelayCommand CreateCommand(Action execute, Func<bool> canExecute = null)
        {
            return new RelayCommand(execute, canExecute);
        }

        /// <summary>
        /// Creates an AsyncRelayCommand
        /// </summary>
        protected AsyncRelayCommand CreateAsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            return new AsyncRelayCommand(execute, canExecute);
        }

        /// <summary>
        /// Creates a RelayCommand with parameter
        /// </summary>
        protected RelayCommand<T> CreateCommand<T>(Action<T> execute, Predicate<T> canExecute = null)
        {
            return new RelayCommand<T>(execute, canExecute);
        }

        /// <summary>
        /// Creates an AsyncRelayCommand with parameter
        /// </summary>
        protected AsyncRelayCommand<T> CreateAsyncCommand<T>(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            return new AsyncRelayCommand<T>(execute, canExecute);
        }

        /// <summary>
        /// Initialize commands - override in derived classes
        /// </summary>
        protected virtual void InitializeCommands()
        {
            // Override in derived classes to initialize commands
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Handle exception with logging and notification
        /// </summary>
        protected void HandleException(Exception ex, string operation = "")
        {
            var message = string.IsNullOrEmpty(operation) 
                ? $"Đã xảy ra lỗi: {ex.Message}" 
                : $"Lỗi khi {operation}: {ex.Message}";

            LogError(ex, operation);
            ShowError(message);
        }

        /// <summary>
        /// Execute action with exception handling
        /// </summary>
        protected void ExecuteSafely(Action action, string operation = "")
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                HandleException(ex, operation);
            }
        }

        /// <summary>
        /// Execute async action with exception handling
        /// </summary>
        protected async Task ExecuteSafelyAsync(Func<Task> action, string operation = "")
        {
            try
            {
                if (action != null)
                    await action();
            }
            catch (Exception ex)
            {
                HandleException(ex, operation);
            }
        }

        #endregion

        protected virtual async Task OnLoadedAsync()
        {
            // Override in derived ViewModels for initialization logic
            await Task.CompletedTask;
        }
    }
}

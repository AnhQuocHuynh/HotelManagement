using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HotelManager.Interfaces;

namespace HotelManager.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected ILogger _logger;
        protected IAuditService _auditService;

        public BaseViewModel()
        {
            // Try to get logger and audit service from DI container
            try
            {
                if (App.ServiceProvider != null)
                {
                    var loggerType = typeof(ILogger<>).MakeGenericType(this.GetType());
                    _logger = App.ServiceProvider.GetService(loggerType) as ILogger;
                    _auditService = (IAuditService)App.ServiceProvider.GetService(typeof(IAuditService));
                }
            }
            catch (Exception ex)
            {
                // Fallback - continue without logging if DI is not available
                System.Diagnostics.Debug.WriteLine($"BaseViewModel: Could not initialize logging: {ex.Message}");
            }
        }

        public BaseViewModel(ILogger logger, IAuditService auditService = null)
        {
            _logger = logger;
            _auditService = auditService;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
    }
}

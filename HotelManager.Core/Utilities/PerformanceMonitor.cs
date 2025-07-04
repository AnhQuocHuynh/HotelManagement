using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManager.Utilities
{
    public class PerformanceMonitor : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly IAuditService _auditService;
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly string _details;
        private bool _disposed = false;

        public PerformanceMonitor(IAuditService auditService, ILogger logger, string operationName, string details = null)
        {
            _auditService = auditService;
            _logger = logger;
            _operationName = operationName;
            _details = details;
            _stopwatch = Stopwatch.StartNew();
            
            _logger.LogDebug("🚀 Starting operation: {OperationName}", operationName);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                var duration = _stopwatch.Elapsed;
                
                _logger.LogDebug("✅ Completed operation: {OperationName} in {Duration}ms", 
                    _operationName, duration.TotalMilliseconds);

                // Log performance metric async (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _auditService.LogPerformanceMetricAsync(_operationName, duration, true, _details);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log performance metric for {OperationName}", _operationName);
                    }
                });

                _disposed = true;
            }
        }

        public void MarkAsFailure(string errorDetails = null)
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                var duration = _stopwatch.Elapsed;
                
                _logger.LogWarning("❌ Failed operation: {OperationName} in {Duration}ms - {ErrorDetails}", 
                    _operationName, duration.TotalMilliseconds, errorDetails);

                // Log failed performance metric async
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var fullDetails = string.IsNullOrEmpty(_details) 
                            ? errorDetails 
                            : $"{_details} | Error: {errorDetails}";
                        await _auditService.LogPerformanceMetricAsync(_operationName, duration, false, fullDetails);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log failed performance metric for {OperationName}", _operationName);
                    }
                });

                _disposed = true;
            }
        }

        public TimeSpan ElapsedTime => _stopwatch.Elapsed;
    }

    public static class PerformanceMonitorExtensions
    {
        public static PerformanceMonitor StartPerformanceMonitor(this IServiceProvider serviceProvider, 
            string operationName, ILogger logger, string details = null)
        {
            var auditService = serviceProvider.GetService(typeof(IAuditService)) as IAuditService;
            return new PerformanceMonitor(auditService, logger, operationName, details);
        }

        public static async Task<T> MonitorAsync<T>(this Task<T> task, PerformanceMonitor monitor)
        {
            try
            {
                var result = await task;
                return result;
            }
            catch (Exception ex)
            {
                monitor.MarkAsFailure(ex.Message);
                throw;
            }
            finally
            {
                monitor.Dispose();
            }
        }

        public static async Task MonitorAsync(this Task task, PerformanceMonitor monitor)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                monitor.MarkAsFailure(ex.Message);
                throw;
            }
            finally
            {
                monitor.Dispose();
            }
        }
    }
} 
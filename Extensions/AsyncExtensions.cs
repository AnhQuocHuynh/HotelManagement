using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HotelManager.Extensions
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Safe fire-and-forget async method with proper exception handling
        /// </summary>
        public static void FireAndForget(this Task task, ILogger logger = null, string operationName = "Unknown")
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Fire-and-forget operation '{OperationName}' failed", operationName);
                    // Optional: Add additional error handling like telemetry
                    System.Diagnostics.Debug.WriteLine($"Fire-and-forget operation '{operationName}' failed: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Safe fire-and-forget async method with custom error handler
        /// </summary>
        public static void FireAndForget(this Task task, Action<Exception> onError, string operationName = "Unknown")
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    try
                    {
                        onError?.Invoke(ex);
                    }
                    catch (Exception handlerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error handler for '{operationName}' also failed: {handlerEx.Message}");
                    }
                }
            });
        }
    }
} 
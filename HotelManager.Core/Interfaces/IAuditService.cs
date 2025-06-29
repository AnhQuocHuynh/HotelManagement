using System;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Interfaces
{
    public interface IAuditService
    {
        /// <summary>
        /// Log user activity for audit trail
        /// </summary>
        Task LogUserActivityAsync(string userId, string action, string entity, string entityId, string details = null);
        
        /// <summary>
        /// Log business operation for audit trail
        /// </summary>
        Task LogBusinessOperationAsync(string operation, string entityType, string entityId, string performedBy, bool success, string details = null);
        
        /// <summary>
        /// Log system event for monitoring
        /// </summary>
        Task LogSystemEventAsync(string eventType, string message, string details = null);
        
        /// <summary>
        /// Log performance metrics
        /// </summary>
        Task LogPerformanceMetricAsync(string operation, TimeSpan duration, bool success, string details = null);
        
        /// <summary>
        /// Log security event
        /// </summary>
        Task LogSecurityEventAsync(string eventType, string userId, string ipAddress, bool success, string details = null);
    }
} 
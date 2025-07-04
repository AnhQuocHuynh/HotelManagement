using System;
using System.Threading.Tasks;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using HotelManager.Utilities;

namespace HotelManager.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly ICurrentUserProvider _userProvider;

        public AuditService(ILogger<AuditService> logger, ICurrentUserProvider userProvider)
        {
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task LogUserActivityAsync(string userId, string action, string entity, string entityId, string details = null)
        {
            var contextUserId = userId ?? _userProvider.GetCurrentUsername() ?? "Anonymous";
            
            _logger.LogInformation("👤 USER_ACTIVITY: {UserId} performed {Action} on {Entity} {EntityId} | Details: {Details}",
                contextUserId, action, entity, entityId, details ?? "N/A");

            // In future, this could also write to a dedicated audit database table
            await Task.CompletedTask;
        }

        public async Task LogBusinessOperationAsync(string operation, string entityType, string entityId, string performedBy, bool success, string details = null)
        {
            var performer = performedBy ?? _userProvider.GetCurrentUsername() ?? "System";
            var status = success ? "SUCCESS" : "FAILED";
            
            _logger.LogInformation("🏢 BUSINESS_OPERATION: {Operation} on {EntityType} {EntityId} by {PerformedBy} - {Status} | Details: {Details}",
                operation, entityType, entityId, performer, status, details ?? "N/A");

            if (!success)
            {
                _logger.LogWarning("⚠️ BUSINESS_OPERATION_FAILED: {Operation} on {EntityType} {EntityId} by {PerformedBy} | Details: {Details}",
                    operation, entityType, entityId, performer, details ?? "N/A");
            }

            await Task.CompletedTask;
        }

        public async Task LogSystemEventAsync(string eventType, string message, string details = null)
        {
            _logger.LogInformation("🔧 SYSTEM_EVENT: {EventType} - {Message} | Details: {Details}",
                eventType, message, details ?? "N/A");

            await Task.CompletedTask;
        }

        public async Task LogPerformanceMetricAsync(string operation, TimeSpan duration, bool success, string details = null)
        {
            var status = success ? "SUCCESS" : "FAILED";
            var durationMs = duration.TotalMilliseconds;
            
            if (durationMs > 5000) // Log slow operations (>5 seconds)
            {
                _logger.LogWarning("🐌 SLOW_OPERATION: {Operation} took {Duration}ms - {Status} | Details: {Details}",
                    operation, durationMs, status, details ?? "N/A");
            }
            else if (durationMs > 1000) // Log medium operations (>1 second)
            {
                _logger.LogInformation("⏱️ PERFORMANCE_METRIC: {Operation} took {Duration}ms - {Status} | Details: {Details}",
                    operation, durationMs, status, details ?? "N/A");
            }
            else
            {
                _logger.LogDebug("⚡ FAST_OPERATION: {Operation} took {Duration}ms - {Status} | Details: {Details}",
                    operation, durationMs, status, details ?? "N/A");
            }

            await Task.CompletedTask;
        }

        public async Task LogSecurityEventAsync(string eventType, string userId, string ipAddress, bool success, string details = null)
        {
            var status = success ? "SUCCESS" : "FAILED";
            
            if (!success)
            {
                _logger.LogWarning("🔒 SECURITY_EVENT: {EventType} for user {UserId} from {IpAddress} - {Status} | Details: {Details}",
                    eventType, userId ?? "Anonymous", ipAddress ?? "Unknown", status, details ?? "N/A");
            }
            else
            {
                _logger.LogInformation("🔓 SECURITY_EVENT: {EventType} for user {UserId} from {IpAddress} - {Status} | Details: {Details}",
                    eventType, userId ?? "Anonymous", ipAddress ?? "Unknown", status, details ?? "N/A");
            }

            await Task.CompletedTask;
        }
    }
} 
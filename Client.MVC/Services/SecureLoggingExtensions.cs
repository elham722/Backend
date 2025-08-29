using Microsoft.Extensions.Logging;

namespace Client.MVC.Services
{
    /// <summary>
    /// Extension methods for secure logging practices
    /// </summary>
    public static class SecureLoggingExtensions
    {
        /// <summary>
        /// Log user authentication operation securely
        /// </summary>
        public static void LogUserAuthentication(this ILogger logger, string operation, string email, bool success, string? errorMessage = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(email, null);
            var operationStatus = LogSanitizer.GetSafeOperationStatus(operation, success, errorMessage);
            
            if (success)
            {
                logger.LogInformation("User authentication {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
            else
            {
                logger.LogWarning("User authentication {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
        }

        /// <summary>
        /// Log user operation securely
        /// </summary>
        public static void LogUserOperation(this ILogger logger, string operation, string email, string? userId, bool success, string? errorMessage = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(email, userId);
            var operationStatus = LogSanitizer.GetSafeOperationStatus(operation, success, errorMessage);
            
            if (success)
            {
                logger.LogInformation("User operation {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
            else
            {
                logger.LogWarning("User operation {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
        }

        /// <summary>
        /// Log token operation securely (without logging actual token values)
        /// </summary>
        public static void LogTokenOperation(this ILogger logger, string operation, string? userId, bool success, string? errorMessage = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(null, userId);
            var operationStatus = LogSanitizer.GetSafeOperationStatus($"Token{operation}", success, errorMessage);
            
            if (success)
            {
                logger.LogInformation("Token operation {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
            else
            {
                logger.LogWarning("Token operation {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
        }

        /// <summary>
        /// Log API request securely
        /// </summary>
        public static void LogApiRequest(this ILogger logger, string method, string endpoint, string? userId, bool success, string? errorMessage = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(null, userId);
            var operationStatus = LogSanitizer.GetSafeOperationStatus($"API{method}", success, errorMessage);
            
            if (success)
            {
                logger.LogInformation("API request {OperationStatus}: {Endpoint}, {UserInfo}", operationStatus, endpoint, userInfo);
            }
            else
            {
                logger.LogWarning("API request {OperationStatus}: {Endpoint}, {UserInfo}", operationStatus, endpoint, userInfo);
            }
        }

        /// <summary>
        /// Log security event securely
        /// </summary>
        public static void LogSecurityEvent(this ILogger logger, string eventType, string? userId, string? ipAddress, string? userAgent, string? details = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(null, userId);
            var sanitizedDetails = LogSanitizer.Sanitize(details ?? "");
            
            logger.LogWarning("Security event: {EventType}, {UserInfo}, IP={IpAddress}, UserAgent={UserAgent}, Details={Details}", 
                eventType, userInfo, ipAddress ?? "Unknown", userAgent ?? "Unknown", sanitizedDetails);
        }

        /// <summary>
        /// Log error securely (without sensitive data)
        /// </summary>
        public static void LogErrorSecurely(this ILogger logger, Exception exception, string operation, string? userId, string? additionalInfo = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(null, userId);
            var sanitizedInfo = LogSanitizer.Sanitize(additionalInfo ?? "");
            
            logger.LogError(exception, "Error during {Operation}: {UserInfo}, AdditionalInfo={AdditionalInfo}", 
                operation, userInfo, sanitizedInfo);
        }

        /// <summary>
        /// Log debug information securely (development only)
        /// </summary>
        public static void LogDebugSecurely(this ILogger logger, string message, object? data = null)
        {
            if (data != null)
            {
                var sanitizedData = LogSanitizer.SanitizeObject(data);
                logger.LogDebug("{Message}: {Data}", message, sanitizedData);
            }
            else
            {
                logger.LogDebug("{Message}", message);
            }
        }

        /// <summary>
        /// Log performance information securely
        /// </summary>
        public static void LogPerformance(this ILogger logger, string operation, TimeSpan duration, string? userId = null, bool success = true)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(null, userId);
            var status = success ? "completed" : "failed";
            
            logger.LogInformation("Performance: {Operation} {Status} in {Duration}ms: {UserInfo}", 
                operation, status, duration.TotalMilliseconds, userInfo);
        }

        /// <summary>
        /// Log audit trail securely
        /// </summary>
        public static void LogAuditTrail(this ILogger logger, string action, string resource, string? userId, string? details = null)
        {
            var userInfo = LogSanitizer.GetSafeUserInfo(null, userId);
            var sanitizedDetails = LogSanitizer.Sanitize(details ?? "");
            
            logger.LogInformation("Audit: {Action} on {Resource} by {UserInfo}, Details={Details}", 
                action, resource, userInfo, sanitizedDetails);
        }
    }
} 
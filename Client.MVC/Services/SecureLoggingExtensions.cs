using Microsoft.Extensions.Logging;

namespace Client.MVC.Services
{
    /// <summary>
    /// Extension methods for secure logging practices
    /// </summary>
    public static class SecureLoggingExtensions
    {
        private static readonly LogSanitizer _logSanitizer = new();

        /// <summary>
        /// Log user authentication operation securely
        /// </summary>
        public static void LogUserAuthentication(this ILogger logger, string operation, string email, bool success, string? errorMessage = null)
        {
            var userInfo = _logSanitizer.GetSafeUserInfo(email, null);
            var operationStatus = _logSanitizer.GetSafeOperationStatus(operation, success, errorMessage);
            
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
            var userInfo = _logSanitizer.GetSafeUserInfo(email, userId);
            var operationStatus = _logSanitizer.GetSafeOperationStatus(operation, success, errorMessage);
            
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
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var operationStatus = _logSanitizer.GetSafeOperationStatus($"Token{operation}", success, errorMessage);
            
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
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var operationStatus = _logSanitizer.GetSafeOperationStatus($"API{method}", success, errorMessage);
            
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
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var sanitizedDetails = _logSanitizer.Sanitize(details ?? "");
            
            logger.LogWarning("Security event: {EventType}, {UserInfo}, IP={IpAddress}, UserAgent={UserAgent}, Details={Details}", 
                eventType, userInfo, ipAddress ?? "Unknown", userAgent ?? "Unknown", sanitizedDetails);
        }

        /// <summary>
        /// Log error securely (without sensitive data)
        /// </summary>
        public static void LogErrorSecurely(this ILogger logger, Exception exception, string operation, string? userId, string? additionalInfo = null)
        {
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var sanitizedInfo = _logSanitizer.Sanitize(additionalInfo ?? "");
            
            logger.LogError(exception, "Error in {Operation}: {UserInfo}, AdditionalInfo={AdditionalInfo}", 
                operation, userInfo, sanitizedInfo);
        }

        /// <summary>
        /// Log object securely (sanitizing sensitive data)
        /// </summary>
        public static void LogObjectSecurely(this ILogger logger, object obj, string operation, string? userId)
        {
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var sanitizedObject = _logSanitizer.SanitizeObject(obj);
            
            logger.LogInformation("Object logged for {Operation}: {UserInfo}, Object={Object}", 
                operation, userInfo, sanitizedObject);
        }

        /// <summary>
        /// Log user session operation securely
        /// </summary>
        public static void LogUserSessionOperation(this ILogger logger, string operation, string? userId, bool success, string? errorMessage = null)
        {
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var operationStatus = _logSanitizer.GetSafeOperationStatus($"Session{operation}", success, errorMessage);
            
            if (success)
            {
                logger.LogInformation("User session {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
            else
            {
                logger.LogWarning("User session {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
        }

        /// <summary>
        /// Log cache operation securely
        /// </summary>
        public static void LogCacheOperation(this ILogger logger, string operation, string? userId, bool success, string? errorMessage = null)
        {
            var userInfo = _logSanitizer.GetSafeUserInfo(null, userId);
            var operationStatus = _logSanitizer.GetSafeOperationStatus($"Cache{operation}", success, errorMessage);
            
            if (success)
            {
                logger.LogInformation("Cache {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
            else
            {
                logger.LogWarning("Cache {OperationStatus}: {UserInfo}", operationStatus, userInfo);
            }
        }
    }
} 
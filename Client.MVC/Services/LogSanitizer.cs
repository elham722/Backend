using System.Text.RegularExpressions;

namespace Client.MVC.Services
{
    /// <summary>
    /// Utility class for sanitizing sensitive data from log messages
    /// </summary>
    public class LogSanitizer
    {
        private static readonly string[] SensitivePatterns = {
            @"password\s*=\s*[^\s&]+",
            @"token\s*=\s*[^\s&]+",
            @"secret\s*=\s*[^\s&]+",
            @"apikey\s*=\s*[^\s&]+",
            @"authorization\s*:\s*bearer\s+[^\s]+",
            @"connectionstring\s*=\s*[^\s&]+",
            @"privatekey\s*=\s*[^\s&]+"
        };

        private static readonly string[] SensitiveFieldNames = {
            "password",
            "token",
            "secret",
            "apikey",
            "authorization",
            "connectionstring",
            "privatekey",
            "clientsecret",
            "accesskey",
            "refreshkey"
        };

        /// <summary>
        /// Sanitize sensitive data from a log message
        /// </summary>
        /// <param name="message">The message to sanitize</param>
        /// <returns>Sanitized message with sensitive data replaced with ***</returns>
        public string SanitizeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            var sanitized = message;

            // Replace patterns like "password=value" with "password=***"
            foreach (var pattern in SensitivePatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, match =>
                {
                    var keyValue = match.Value.Split('=');
                    if (keyValue.Length == 2)
                    {
                        return $"{keyValue[0].Trim()}=***";
                    }
                    return match.Value;
                }, RegexOptions.IgnoreCase);
            }

            // Replace sensitive field names in JSON-like structures
            foreach (var fieldName in SensitiveFieldNames)
            {
                sanitized = Regex.Replace(sanitized, 
                    $@"""{fieldName}""\s*:\s*""[^""]*""", 
                    $"\"{fieldName}\":\"***\"", 
                    RegexOptions.IgnoreCase);
            }

            return sanitized;
        }

        /// <summary>
        /// Sanitize stack trace to remove sensitive information
        /// </summary>
        /// <param name="stackTrace">The stack trace to sanitize</param>
        /// <returns>Sanitized stack trace</returns>
        public string SanitizeStackTrace(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return stackTrace ?? string.Empty;

            var sanitized = stackTrace;

            // Remove file paths that might contain sensitive information
            sanitized = Regex.Replace(sanitized, @"at\s+[^:]+:\s*line\s+\d+", "at [REDACTED]");
            
            // Remove connection strings and other sensitive data
            sanitized = Regex.Replace(sanitized, @"connection\s*string\s*=\s*[^\s]+", "connection string=***");
            sanitized = Regex.Replace(sanitized, @"password\s*=\s*[^\s]+", "password=***");
            sanitized = Regex.Replace(sanitized, @"token\s*=\s*[^\s]+", "token=***");

            return sanitized;
        }

        /// <summary>
        /// Sanitize sensitive data from a log message (legacy method)
        /// </summary>
        /// <param name="message">The message to sanitize</param>
        /// <returns>Sanitized message with sensitive data replaced with ***</returns>
        public string Sanitize(string message)
        {
            return SanitizeMessage(message);
        }

        /// <summary>
        /// Sanitize sensitive data from an object for logging
        /// </summary>
        /// <param name="obj">Object to sanitize</param>
        /// <returns>Sanitized object representation</returns>
        public string SanitizeObject(object obj)
        {
            if (obj == null)
                return "null";

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(obj);
                return SanitizeMessage(json);
            }
            catch
            {
                return SanitizeMessage(obj.ToString() ?? "null");
            }
        }

        /// <summary>
        /// Check if a message contains sensitive data
        /// </summary>
        /// <param name="message">Message to check</param>
        /// <returns>True if sensitive data is detected</returns>
        public bool ContainsSensitiveData(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            var sanitized = SanitizeMessage(message);
            return sanitized != message;
        }

        /// <summary>
        /// Get a safe representation of user information for logging
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="userId">User ID</param>
        /// <returns>Safe user information string</returns>
        public string GetSafeUserInfo(string? email, string? userId)
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(email))
                parts.Add($"Email={email}");
                
            if (!string.IsNullOrEmpty(userId))
                parts.Add($"UserId={userId}");

            return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
        }

        /// <summary>
        /// Get a safe representation of operation status for logging
        /// </summary>
        /// <param name="operation">Operation name</param>
        /// <param name="success">Success status</param>
        /// <param name="errorMessage">Error message (will be sanitized)</param>
        /// <returns>Safe operation status string</returns>
        public string GetSafeOperationStatus(string operation, bool success, string? errorMessage = null)
        {
            var status = success ? "successful" : "failed";
            var result = $"Operation={operation}, Status={status}";
            
            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                result += $", Error={SanitizeMessage(errorMessage)}";
            }

            return result;
        }
    }
} 
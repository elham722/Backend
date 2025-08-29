using System.Text.RegularExpressions;

namespace Client.MVC.Services
{
    /// <summary>
    /// Utility class for sanitizing sensitive data from log messages
    /// </summary>
    public static class LogSanitizer
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
        public static string Sanitize(string message)
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
        /// Sanitize sensitive data from an object for logging
        /// </summary>
        /// <param name="obj">Object to sanitize</param>
        /// <returns>Sanitized object representation</returns>
        public static string SanitizeObject(object obj)
        {
            if (obj == null)
                return "null";

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(obj);
                return Sanitize(json);
            }
            catch
            {
                return Sanitize(obj.ToString() ?? "null");
            }
        }

        /// <summary>
        /// Check if a message contains sensitive data
        /// </summary>
        /// <param name="message">Message to check</param>
        /// <returns>True if sensitive data is detected</returns>
        public static bool ContainsSensitiveData(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            var sanitized = Sanitize(message);
            return sanitized != message;
        }

        /// <summary>
        /// Get a safe representation of user information for logging
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="userId">User ID</param>
        /// <returns>Safe user information string</returns>
        public static string GetSafeUserInfo(string? email, string? userId)
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
        public static string GetSafeOperationStatus(string operation, bool success, string? errorMessage = null)
        {
            var status = success ? "successful" : "failed";
            var result = $"Operation={operation}, Status={status}";
            
            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                result += $", Error={Sanitize(errorMessage)}";
            }

            return result;
        }
    }
} 
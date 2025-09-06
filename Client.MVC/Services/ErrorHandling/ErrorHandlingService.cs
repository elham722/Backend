using Microsoft.Extensions.Logging;
using System.Text.Json;
using Client.MVC.Services.Abstractions;
using Client.MVC.Services.Infrastructure;

namespace Client.MVC.Services.ErrorHandling
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly LogSanitizer _logSanitizer;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger, LogSanitizer logSanitizer)
        {
            _logger = logger;
            _logSanitizer = logSanitizer;
        }

        public async Task LogErrorAsync(Exception exception, string context = "", object? additionalData = null)
        {
            if (!ShouldLogException(exception))
                return;

            var sanitizedMessage = _logSanitizer.SanitizeMessage(exception.Message);
            var sanitizedStackTrace = _logSanitizer.SanitizeStackTrace(exception.StackTrace);

            var logData = new
            {
                Context = context,
                Message = sanitizedMessage,
                ExceptionType = exception.GetType().Name,
                StackTrace = sanitizedStackTrace,
                AdditionalData = additionalData,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogError(exception, "Error in {Context}: {Message}", context, sanitizedMessage);
            
            // Additional structured logging
            await Task.Run(() => _logger.LogError("Structured Error Data: {LogData}", 
                JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true })));
        }

        public async Task LogWarningAsync(string message, object? additionalData = null)
        {
            var sanitizedMessage = _logSanitizer.SanitizeMessage(message);
            
            var logData = new
            {
                Message = sanitizedMessage,
                AdditionalData = additionalData,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogWarning("Warning: {Message}", sanitizedMessage);
            
            await Task.Run(() => _logger.LogWarning("Structured Warning Data: {LogData}", 
                JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true })));
        }

        public async Task LogInformationAsync(string message, object? additionalData = null)
        {
            var sanitizedMessage = _logSanitizer.SanitizeMessage(message);
            
            var logData = new
            {
                Message = sanitizedMessage,
                AdditionalData = additionalData,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Info: {Message}", sanitizedMessage);
            
            await Task.Run(() => _logger.LogInformation("Structured Info Data: {LogData}", 
                JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true })));
        }

        public string SanitizeErrorMessage(string errorMessage)
        {
            return _logSanitizer.SanitizeMessage(errorMessage);
        }

        public bool ShouldLogException(Exception exception)
        {
            // Don't log certain types of exceptions that are expected
            var exceptionType = exception.GetType();
            
            // Skip logging for common expected exceptions
            if (exceptionType.Name.Contains("TaskCanceledException") ||
                exceptionType.Name.Contains("OperationCanceledException") ||
                exceptionType.Name.Contains("HttpRequestException"))
            {
                return false;
            }

            return true;
        }
    }
} 
namespace Client.MVC.Services.Abstractions
{
    public interface IErrorHandlingService
    {
        Task LogErrorAsync(Exception exception, string context = "", object? additionalData = null);
        Task LogWarningAsync(string message, object? additionalData = null);
        Task LogInformationAsync(string message, object? additionalData = null);
        string SanitizeErrorMessage(string errorMessage);
        bool ShouldLogException(Exception exception);
    }
}

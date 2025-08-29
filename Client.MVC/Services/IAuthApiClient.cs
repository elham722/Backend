using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Services
{
    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
        public int? StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create successful response
        /// </summary>
        public static ApiResponse<T> Success(T data, int? statusCode = 200)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create error response
        /// </summary>
        public static ApiResponse<T> Error(string errorMessage, int? statusCode = 400)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create cancelled response
        /// </summary>
        public static ApiResponse<T> Cancelled(string? errorMessage = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage ?? "Operation was cancelled",
                StatusCode = 499, // Client Closed Request
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Base result for API operations (for backward compatibility)
    /// </summary>
    public class ApiResultDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int? StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Result of logout operation
    /// </summary>
    public class LogoutResultDto : ApiResultDto
    {
        public DateTime LogoutTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Authentication API client for user authentication operations
    /// </summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        Task<ApiResponse<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Login user
        /// </summary>
        Task<ApiResponse<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout user
        /// </summary>
        Task<ApiResponse<LogoutResultDto>> LogoutAsync(LogoutDto? logoutDto = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<ApiResponse<AuthResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
} 
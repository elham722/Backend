namespace Backend.Application.Common.Results;

/// <summary>
/// Generic API response wrapper for consistent response handling across all layers
/// </summary>
/// <typeparam name="T">Type of the data to return</typeparam>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
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
    public static ApiResponse<T> Error(string errorMessage, int? statusCode = 400, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode,
            ErrorCode = errorCode,
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
            ErrorCode = "CANCELLED",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create failed response from exception
    /// </summary>
    public static ApiResponse<T> Failure(Exception exception, int? statusCode = 500)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = exception.Message,
            StatusCode = statusCode,
            ErrorCode = exception.GetType().Name,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convert from Result<T> to ApiResponse<T>
    /// </summary>
    public static ApiResponse<T> FromResult(Result<T> result, int? statusCode = null)
    {
        if (result.IsSuccess)
        {
            return Success(result.Data!, statusCode);
        }
        
        return Error(result.ErrorMessage ?? "Operation failed", statusCode, result.ErrorCode);
    }

    /// <summary>
    /// Convert from Result to ApiResponse<T>
    /// </summary>
    public static ApiResponse<T> FromResult(Result result, int? statusCode = null)
    {
        if (result.IsSuccess)
        {
            return Error("Cannot convert successful Result to ApiResponse<T> without data", statusCode ?? 400);
        }
        
        return Error(result.ErrorMessage ?? "Operation failed", statusCode, result.ErrorCode);
    }
}

/// <summary>
/// Non-generic API response for operations that don't return data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Create successful response without data
    /// </summary>
    public static new ApiResponse Success(int? statusCode = 200)
    {
        return new ApiResponse
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create error response without data
    /// </summary>
    public static new ApiResponse Error(string errorMessage, int? statusCode = 400, string? errorCode = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create cancelled response without data
    /// </summary>
    public static new ApiResponse Cancelled(string? errorMessage = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage ?? "Operation was cancelled",
            StatusCode = 499,
            ErrorCode = "CANCELLED",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create failed response from exception without data
    /// </summary>
    public static new ApiResponse Failure(Exception exception, int? statusCode = 500)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            ErrorMessage = exception.Message,
            StatusCode = statusCode,
            ErrorCode = exception.GetType().Name,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convert from Result to ApiResponse
    /// </summary>
    public static new ApiResponse FromResult(Result result, int? statusCode = null)
    {
        if (result.IsSuccess)
        {
            return Success(statusCode);
        }
        
        return Error(result.ErrorMessage ?? "Operation failed", statusCode, result.ErrorCode);
    }
} 
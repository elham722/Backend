namespace Backend.Application.Common.Results;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ? Success
    public static ApiResponse<T> Success(T data, int? statusCode = 200) =>
        new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    // ? Error
    public static ApiResponse<T> Error(string errorMessage, int? statusCode = 400, string? errorCode = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode, ErrorCode = errorCode };

    // ? Cancelled
    public static ApiResponse<T> Cancelled(string? errorMessage = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage ?? "Operation was cancelled", StatusCode = 499, ErrorCode = "CANCELLED" };

    // ? Failure from exception
    public static ApiResponse<T> Failure(Exception ex, int? statusCode = 500) =>
        new() { IsSuccess = false, ErrorMessage = ex.Message, StatusCode = statusCode, ErrorCode = ex.GetType().Name };

    // ? Failure with custom message
    public static ApiResponse<T> Failure(string errorMessage, string? errorCode = null, int? statusCode = 400) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode, ErrorCode = errorCode };

    // ? From Result<T>
    public static ApiResponse<T> FromResult(Result<T> result, int? statusCode = null)
    {
        if (result.IsSuccess)
        {
            // Check if Data is null even when IsSuccess is true
            if (result.Data == null)
            {
                return Error("Operation completed but no data received", statusCode ?? 500, "NULL_DATA");
            }
            return Success(result.Data, statusCode ?? 200);
        }
        else
        {
            return Error(result.ErrorMessage ?? "Operation failed", statusCode ?? 400, result.ErrorCode);
        }
    }
}

// ? Non-generic ?? ?????? Data
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(object? data = null, int? statusCode = 200) =>
        new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static new ApiResponse Error(string errorMessage, int? statusCode = 400, string? errorCode = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode, ErrorCode = errorCode };

    public static new ApiResponse Cancelled(string? errorMessage = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage ?? "Operation was cancelled", StatusCode = 499, ErrorCode = "CANCELLED" };

    public static new ApiResponse Failure(Exception ex, int? statusCode = 500) =>
        new() { IsSuccess = false, ErrorMessage = ex.Message, StatusCode = statusCode, ErrorCode = ex.GetType().Name };

    public static new ApiResponse Failure(string errorMessage, string? errorCode = null, int? statusCode = 400) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode, ErrorCode = errorCode };

    public static new ApiResponse FromResult(Result result, int? statusCode = null) =>
        result.IsSuccess
            ? Success(null, statusCode ?? 200) // ????? ??????? Data = null ?? ?? ???? ?????
            : Error(result.ErrorMessage ?? "Operation failed", statusCode ?? 400, result.ErrorCode);
}

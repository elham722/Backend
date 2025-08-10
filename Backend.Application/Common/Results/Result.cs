namespace Backend.Application.Common.Results;

/// <summary>
/// Base result class for operations that don't return data
/// </summary>
public class Result : IResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static Result Failure(string errorMessage, string? errorCode = null) => 
        new(false, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    public static Result Failure(Exception exception) => 
        new(false, exception.Message, exception.GetType().Name);
} 
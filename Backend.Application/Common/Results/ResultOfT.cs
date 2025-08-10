namespace Backend.Application.Common.Results;

/// <summary>
/// Generic result class for operations that return data
/// </summary>
/// <typeparam name="T">Type of the data to return</typeparam>
public class Result<T> : IResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }
    public T? Data { get; }

    protected Result(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static Result<T> Success(T data) => new(true, data);

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static Result<T> Failure(string errorMessage, string? errorCode = null) => 
        new(false, default, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    public static Result<T> Failure(Exception exception) => 
        new(false, default, exception.Message, exception.GetType().Name);

    /// <summary>
    /// Implicit conversion from Result to Result<T>
    /// </summary>
    public static implicit operator Result<T>(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert successful Result to Result<T> without data");
        
        return Failure(result.ErrorMessage!, result.ErrorCode);
    }
} 
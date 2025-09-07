namespace Backend.Application.Common.Results;

public class Result<T> : Result
{
    public T? Data { get; }
    public int StatusCode { get; }
    public DateTime Timestamp { get; }

    private Result(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null, int statusCode = 200)
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
        StatusCode = statusCode;
        Timestamp = DateTime.UtcNow;
    }

    public static Result<T> Success(T data, int statusCode = 200) => new(true, data, null, null, statusCode);
    public static new Result<T> Failure(string errorMessage, string? errorCode = null, int statusCode = 400)
        => new(false, default, errorMessage, errorCode, statusCode);
    public static new Result<T> Failure(Exception ex, int statusCode = 500)
        => new(false, default, ex.Message, ex.GetType().Name, statusCode);
}
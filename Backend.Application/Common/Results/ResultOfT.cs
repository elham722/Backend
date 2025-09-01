namespace Backend.Application.Common.Results;

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null)
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data);
    public static new Result<T> Failure(string errorMessage, string? errorCode = null)
        => new(false, default, errorMessage, errorCode);
    public static new Result<T> Failure(Exception ex)
        => new(false, default, ex.Message, ex.GetType().Name);
}
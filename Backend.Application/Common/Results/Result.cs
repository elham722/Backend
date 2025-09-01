namespace Backend.Application.Common.Results;

public class Result
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

    public static Result Success() => new(true);
    public static Result Failure(string errorMessage, string? errorCode = null)
        => new(false, errorMessage, errorCode);
    public static Result Failure(Exception ex)
        => new(false, ex.Message, ex.GetType().Name);
}
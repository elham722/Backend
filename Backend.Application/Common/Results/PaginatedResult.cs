namespace Backend.Application.Common.Results;

/// <summary>
/// Result class for paginated data
/// </summary>
/// <typeparam name="T">Type of the data items</typeparam>
public class PaginatedResult<T> 
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }
    public IEnumerable<T>? Data { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }

    protected PaginatedResult(
        bool isSuccess, 
        IEnumerable<T>? data = null, 
        int totalCount = 0, 
        int pageNumber = 1, 
        int pageSize = 10,
        string? errorMessage = null, 
        string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful paginated result
    /// </summary>
    public static PaginatedResult<T> Success(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize) => 
        new(true, data, totalCount, pageNumber, pageSize);

    /// <summary>
    /// Creates a failed paginated result
    /// </summary>
    public static PaginatedResult<T> Failure(string errorMessage, string? errorCode = null) => 
        new(false, errorMessage: errorMessage, errorCode: errorCode);

    /// <summary>
    /// Creates a failed paginated result from an exception
    /// </summary>
    public static PaginatedResult<T> Failure(Exception exception) => 
        new(false, errorMessage: exception.Message, errorCode: exception.GetType().Name);
} 
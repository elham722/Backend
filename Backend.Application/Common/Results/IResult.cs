namespace Backend.Application.Common.Results;

/// <summary>
/// Base interface for all operation results
/// </summary>
public interface IResult
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Error code if the operation failed
    /// </summary>
    string? ErrorCode { get; }
} 
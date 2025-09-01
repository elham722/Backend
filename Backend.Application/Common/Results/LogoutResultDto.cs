namespace Backend.Application.Common.Results;

/// <summary>
/// Result of logout operation
/// </summary>
public class LogoutResultDto
{
    /// <summary>
    /// Whether logout was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Error message if logout failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Error code if logout failed
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Time when logout occurred
    /// </summary>
    public DateTime LogoutTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Number of tokens invalidated
    /// </summary>
    public int TokensInvalidated { get; set; }
    
    /// <summary>
    /// Whether logout was from all devices
    /// </summary>
    public bool LogoutFromAllDevices { get; set; }
    
    /// <summary>
    /// User ID who logged out
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Session ID that was invalidated
    /// </summary>
    public string? SessionId { get; set; }
} 
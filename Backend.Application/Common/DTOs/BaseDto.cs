namespace Backend.Application.Common.DTOs;

/// <summary>
/// Base DTO class with common properties
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Last modification date
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// Entity status
    /// </summary>
    public string Status { get; set; } = string.Empty;
} 
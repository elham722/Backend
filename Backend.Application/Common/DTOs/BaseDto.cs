namespace Backend.Application.Common.DTOs;

public abstract class BaseDto
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    public string Status { get; set; } = string.Empty;
} 
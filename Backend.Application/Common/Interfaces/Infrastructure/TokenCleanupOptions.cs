namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Configuration options for token cleanup service
/// </summary>
public class TokenCleanupOptions
{
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(15);
    public bool EnableCleanup { get; set; } = true;
    public int MaxTokensPerCleanup { get; set; } = 1000;
} 
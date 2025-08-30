namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Configuration for token cache settings
/// </summary>
public class TokenCacheConfiguration
{
    public int JwtTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int MaxTokensPerUser { get; set; } = 5;
    public bool EnableTokenRotation { get; set; } = true;
    public int TokenRotationThresholdMinutes { get; set; } = 5;
} 
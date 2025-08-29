using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Commands.RefreshToken;

/// <summary>
/// Command to refresh access token using refresh token
/// </summary>
public class RefreshTokenCommand : ICommand<Result<AuthResultDto>>
{
    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// IP address of the refresh attempt (for security)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string (for security)
    /// </summary>
    public string? UserAgent { get; set; }
} 
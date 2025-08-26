using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Commands.Login;

/// <summary>
/// Command to authenticate a user
/// </summary>
public class LoginCommand : ICommand<Result<AuthResultDto>>
{
    /// <summary>
    /// User's email or username
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to remember the user
    /// </summary>
    public bool RememberMe { get; set; } = false;
    
    /// <summary>
    /// Two-factor authentication code (if enabled)
    /// </summary>
    public string? TwoFactorCode { get; set; }
    
    /// <summary>
    /// IP address of the login attempt (for security)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string (for security)
    /// </summary>
    public string? UserAgent { get; set; }
} 
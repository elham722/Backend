using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Commands.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public class RegisterCommand : ICommand<Result<AuthResultDto>>
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's username
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirm password
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// User's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether to accept terms and conditions
    /// </summary>
    public bool AcceptTerms { get; set; } = false;
    
    /// <summary>
    /// Whether to subscribe to newsletter
    /// </summary>
    public bool SubscribeToNewsletter { get; set; } = false;
    
    /// <summary>
    /// IP address of the registration attempt (for security)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string (for security)
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Device information (browser, OS, device type)
    /// </summary>
    public string? DeviceInfo { get; set; }
} 
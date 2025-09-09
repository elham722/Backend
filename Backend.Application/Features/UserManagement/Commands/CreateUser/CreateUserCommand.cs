using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Commands.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public class CreateUserCommand : ICommand<Result<UserDto>>
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
    /// User's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// User roles to assign
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Associated customer ID (optional)
    /// </summary>
    public string? CustomerId { get; set; }
    
    /// <summary>
    /// Whether to send confirmation email
    /// </summary>
    public bool SendConfirmationEmail { get; set; } = true;
    
    /// <summary>
    /// Whether to require password change on first login
    /// </summary>
    public bool RequirePasswordChange { get; set; } = false;
    
    /// <summary>
    /// ID of the user creating this account (for audit)
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
} 
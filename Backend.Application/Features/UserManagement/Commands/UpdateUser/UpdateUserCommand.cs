using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Command to update user information
/// </summary>
public class UpdateUserCommand : ICommand<Result<UserDto>>
{
    /// <summary>
    /// User ID to update
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// User's username
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether email is confirmed
    /// </summary>
    public bool? EmailConfirmed { get; set; }
    
    /// <summary>
    /// Whether phone number is confirmed
    /// </summary>
    public bool? PhoneNumberConfirmed { get; set; }
    
    /// <summary>
    /// Whether account is active
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// User roles to assign (replaces existing roles)
    /// </summary>
    public List<string>? Roles { get; set; }
    
    /// <summary>
    /// Associated customer ID
    /// </summary>
    public Guid? CustomerId { get; set; }
    
    /// <summary>
    /// ID of the user performing the update (for audit)
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;
} 
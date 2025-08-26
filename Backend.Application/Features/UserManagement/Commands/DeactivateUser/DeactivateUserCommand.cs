using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;

namespace Backend.Application.Features.UserManagement.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user account
/// </summary>
public class DeactivateUserCommand : ICommand<Result>
{
    /// <summary>
    /// User ID to deactivate
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the user performing the deactivation (for audit)
    /// </summary>
    public string DeactivatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason for deactivation
    /// </summary>
    public string? Reason { get; set; }
} 
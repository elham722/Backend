using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;

namespace Backend.Application.Features.UserManagement.Commands.ActivateUser;

/// <summary>
/// Command to activate a user account
/// </summary>
public class ActivateUserCommand : ICommand<Result>
{
    /// <summary>
    /// User ID to activate
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the user performing the activation (for audit)
    /// </summary>
    public string ActivatedBy { get; set; } = string.Empty;
} 
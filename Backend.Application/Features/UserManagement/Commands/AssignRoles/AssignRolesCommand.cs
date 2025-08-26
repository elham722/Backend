using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;

namespace Backend.Application.Features.UserManagement.Commands.AssignRoles;

/// <summary>
/// Command to assign roles to a user
/// </summary>
public class AssignRolesCommand : ICommand<Result>
{
    /// <summary>
    /// User ID to assign roles to
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Roles to assign
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// ID of the user performing the role assignment (for audit)
    /// </summary>
    public string AssignedBy { get; set; } = string.Empty;
} 
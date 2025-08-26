using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;

namespace Backend.Application.Features.UserManagement.Commands.DeleteUser;

/// <summary>
/// Command to delete a user (soft delete)
/// </summary>
public class DeleteUserCommand : ICommand<Result>
{
    /// <summary>
    /// User ID to delete
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the user performing the deletion (for audit)
    /// </summary>
    public string DeletedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to permanently delete the user
    /// </summary>
    public bool PermanentDelete { get; set; } = false;
} 
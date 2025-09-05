using Backend.Application.Common.DTOs;

namespace Backend.Application.Features.UserManagement.DTOs.Identity;

/// <summary>
/// DTO for UserRole
/// </summary>
public class UserRoleDto : BaseDto
{
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
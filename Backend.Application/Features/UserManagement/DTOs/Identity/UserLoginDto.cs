using Backend.Application.Common.DTOs;

namespace Backend.Application.Features.UserManagement.DTOs.Identity;

/// <summary>
/// DTO for UserLogin
/// </summary>
public class UserLoginDto : BaseDto
{
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
using Backend.Application.Common.DTOs;

namespace Backend.Application.Features.UserManagement.DTOs.Identity;

/// <summary>
/// DTO for UserClaim
/// </summary>
public class UserClaimDto : BaseDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}
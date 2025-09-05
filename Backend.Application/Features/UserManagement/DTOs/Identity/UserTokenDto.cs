using Backend.Application.Common.DTOs;
using System;

namespace Backend.Application.Features.UserManagement.DTOs.Identity;

/// <summary>
/// DTO for UserToken
/// </summary>
public class UserTokenDto : BaseDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LoginProvider { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
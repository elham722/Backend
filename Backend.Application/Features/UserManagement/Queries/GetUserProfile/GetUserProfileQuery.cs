using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Queries.GetUserProfile;

/// <summary>
/// Query to get the current user's profile
/// </summary>
public class GetUserProfileQuery : IQuery<Result<UserDto>>
{
    /// <summary>
    /// Current user ID (from JWT token)
    /// </summary>
    public string CurrentUserId { get; set; } = string.Empty;
} 
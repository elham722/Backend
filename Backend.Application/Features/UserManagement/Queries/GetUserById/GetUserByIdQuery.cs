using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Query to get a user by ID
/// </summary>
public class GetUserByIdQuery : IQuery<Result<UserDto>>
{
    /// <summary>
    /// User ID to retrieve
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include roles in the response
    /// </summary>
    public bool IncludeRoles { get; set; } = true;
} 
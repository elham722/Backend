using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Query to get user by ID
/// </summary>
public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    /// <summary>
    /// User ID to retrieve
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.Queries.GetUserById;
using MediatR;

namespace Backend.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery
/// Uses IUserService to maintain clean architecture principles
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUserByIdAsync(request.UserId, includeRoles: true, cancellationToken);

        return ApiResponse<UserDto>.FromResult(result, result.IsSuccess ? 200 : 404);
    }
}
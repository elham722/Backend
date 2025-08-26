using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Queries.GetUserProfile;

/// <summary>
/// Handler for GetUserProfileQuery
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserDto>>
{
    private readonly IUserService _userService;

    public GetUserProfileQueryHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to get user profile (current user)
            var result = await _userService.GetUserByIdAsync(request.CurrentUserId, true, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure(ex.Message, "GetUserProfileError");
        }
    }
} 
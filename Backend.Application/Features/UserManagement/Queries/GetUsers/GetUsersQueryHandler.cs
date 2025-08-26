using Backend.Application.Common.DTOs;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Queries.GetUsers;

/// <summary>
/// Handler for GetUsersQuery
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PaginationResponse<UserDto>>>
{
    private readonly IUserService _userService;

    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<PaginationResponse<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to get paginated users
            var result = await _userService.GetUsersAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.Status,
                request.Role,
                request.EmailConfirmed,
                request.IsActive,
                request.SortBy,
                request.SortDirection,
                request.IncludeDeleted,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<PaginationResponse<UserDto>>.Failure(ex.Message, "GetUsersError");
        }
    }
} 
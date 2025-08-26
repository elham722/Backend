using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.AssignRoles;

/// <summary>
/// Handler for AssignRolesCommand
/// </summary>
public class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, Result>
{
    private readonly IUserService _userService;

    public AssignRolesCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to assign roles
            var result = await _userService.AssignRolesAsync(request.UserId, request.Roles, request.AssignedBy, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "AssignRolesError");
        }
    }
} 
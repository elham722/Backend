using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.DeactivateUser;

/// <summary>
/// Handler for DeactivateUserCommand
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly IUserService _userService;

    public DeactivateUserCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to deactivate user
            var result = await _userService.DeactivateUserAsync(request.UserId, request.DeactivatedBy, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "DeactivateUserError");
        }
    }
} 
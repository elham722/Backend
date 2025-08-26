using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.ActivateUser;

/// <summary>
/// Handler for ActivateUserCommand
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Result>
{
    private readonly IUserService _userService;

    public ActivateUserCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to activate user
            var result = await _userService.ActivateUserAsync(request.UserId, request.ActivatedBy, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "ActivateUserError");
        }
    }
} 
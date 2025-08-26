using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.ChangePassword;

/// <summary>
/// Handler for ChangePasswordCommand
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserService _userService;

    public ChangePasswordCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to change password
            var result = await _userService.ChangePasswordAsync(
                request.UserId, 
                request.CurrentPassword, 
                request.NewPassword, 
                request.ChangedBy, 
                request.RequirePasswordChangeOnNextLogin, 
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "ChangePasswordError");
        }
    }
} 
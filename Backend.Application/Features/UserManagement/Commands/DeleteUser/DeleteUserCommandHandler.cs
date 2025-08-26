using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.DeleteUser;

/// <summary>
/// Handler for DeleteUserCommand
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserService _userService;

    public DeleteUserCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Call service to delete user
            var result = await _userService.DeleteUserAsync(request.UserId, request.DeletedBy, request.PermanentDelete, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "DeleteUserError");
        }
    }
} 
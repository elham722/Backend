using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create DTO from command
            var updateUserDto = new UpdateUserDto
            {
                Email = request.Email,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = request.EmailConfirmed,
                PhoneNumberConfirmed = request.PhoneNumberConfirmed,
                IsActive = request.IsActive,
                Roles = request.Roles,
                CustomerId = request.CustomerId
            };

            // Call service to update user
            var result = await _userService.UpdateUserAsync(request.UserId, updateUserDto, request.UpdatedBy, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure(ex.Message, "UpdateUserError");
        }
    }
} 
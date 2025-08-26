using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.CreateUser;

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserService _userService;

    public CreateUserCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create DTO from command
            var createUserDto = new CreateUserDto
            {
                Email = request.Email,
                UserName = request.UserName,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                Roles = request.Roles,
                CustomerId = request.CustomerId,
                SendConfirmationEmail = request.SendConfirmationEmail,
                RequirePasswordChange = request.RequirePasswordChange
            };

            // Call service to create user
            var result = await _userService.CreateUserAsync(createUserDto, request.CreatedBy, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure(ex.Message, "CreateUserError");
        }
    }
} 
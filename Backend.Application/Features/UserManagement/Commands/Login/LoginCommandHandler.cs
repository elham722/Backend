using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.Login;

/// <summary>
/// Handler for LoginCommand
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly IUserService _userService;

    public LoginCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create DTO from command
            var loginDto = new LoginDto
            {
                EmailOrUsername = request.EmailOrUsername,
                Password = request.Password,
                RememberMe = request.RememberMe,
                TwoFactorCode = request.TwoFactorCode
            };

            // Call service to authenticate user
            var result = await _userService.LoginAsync(loginDto, request.IpAddress, request.UserAgent, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<AuthResultDto>.Failure(ex.Message, "LoginError");
        }
    }
} 
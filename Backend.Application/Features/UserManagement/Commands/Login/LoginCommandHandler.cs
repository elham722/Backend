using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.Login;

/// <summary>
/// Handler for LoginCommand
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserService _userService;

    public LoginCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginDto = new LoginRequest
        {
            EmailOrUsername = request.EmailOrUsername,
            Password = request.Password,
            RememberMe = request.RememberMe,
            TwoFactorCode = request.TwoFactorCode,
            IpAddress = request.IpAddress,
            DeviceInfo = request.DeviceInfo
        };

        return await _userService.LoginAsync(loginDto, request.IpAddress, request.UserAgent, cancellationToken);
    }

}
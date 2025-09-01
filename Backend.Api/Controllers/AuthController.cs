using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.RefreshToken;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.Results;
using Backend.Application.Common.Interfaces.Infrastructure;

namespace Backend.Api.Controllers;

/// <summary>
/// Authentication controller for user registration and login
/// </summary>
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IDeviceInfoService _deviceInfoService;

    public AuthController(IMediator mediator, IDeviceInfoService deviceInfoService, ILogger<AuthController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _deviceInfoService = deviceInfoService ?? throw new ArgumentNullException(nameof(deviceInfoService));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                Password = registerDto.Password,
                ConfirmPassword = registerDto.ConfirmPassword,
                PhoneNumber = registerDto.PhoneNumber,
                AcceptTerms = registerDto.AcceptTerms,
                SubscribeToNewsletter = registerDto.SubscribeToNewsletter,
                CaptchaToken = registerDto.CaptchaToken,
                IpAddress = registerDto.IpAddress ?? _deviceInfoService.GetIpAddress(),
                UserAgent = _deviceInfoService.GetUserAgent(),
                DeviceInfo = registerDto.DeviceInfo ?? _deviceInfoService.GetDeviceInfo()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex, "An unexpected error occurred during registration");
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var command = new LoginCommand
            {
                EmailOrUsername = loginDto.EmailOrUsername,
                Password = loginDto.Password,
                RememberMe = loginDto.RememberMe,
                TwoFactorCode = loginDto.TwoFactorCode,
                IpAddress = loginDto.IpAddress ?? _deviceInfoService.GetIpAddress(),
                UserAgent = _deviceInfoService.GetUserAgent(),
                DeviceInfo = loginDto.DeviceInfo ?? _deviceInfoService.GetDeviceInfo()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex, "An unexpected error occurred during login");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = refreshTokenDto.RefreshToken,
                IpAddress = _deviceInfoService.GetIpAddress(),
                UserAgent = _deviceInfoService.GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex, "An unexpected error occurred during token refresh");
        }
    }

    /// <summary>
    /// Logout user and invalidate refresh tokens
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<LogoutResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        try
        {
            var command = new LogoutCommand
            {
                RefreshToken = logoutDto.RefreshToken,
                LogoutFromAllDevices = logoutDto.LogoutFromAllDevices,
                IpAddress = _deviceInfoService.GetIpAddress(),
                UserAgent = _deviceInfoService.GetUserAgent(),
                LogoutReason = "User initiated logout"
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex, "An unexpected error occurred during logout");
        }
    }

    
}

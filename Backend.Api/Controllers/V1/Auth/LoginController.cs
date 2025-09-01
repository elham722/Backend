 using Asp.Versioning;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.V1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class LoginController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IMediator mediator, IDeviceInfoService deviceInfoService, ILogger<LoginController> logger)
    {
        _mediator = mediator;
        _deviceInfoService = deviceInfoService;
        _logger = logger;
    }

    [HttpPost("login")]
    [MapToApiVersion("1.0")]
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
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during login");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred during login", 500, ex.GetType().Name));
        }
    }

    [HttpPost("logout")]
    [MapToApiVersion("1.0")]
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
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during logout");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred during logout", 500, ex.GetType().Name));
        }
    }
}


using Asp.Versioning;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
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
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginDto)
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
            
            // Log the result for debugging
            _logger.LogInformation("Login result - IsSuccess: {IsSuccess}, Data: {Data}", 
                result.IsSuccess, result.Data != null ? "Not null" : "NULL");
            
            if (result.Data != null)
            {
                _logger.LogInformation("Login result - AccessToken: {AccessToken}, RefreshToken: {RefreshToken}", 
                    result.Data.AccessToken, result.Data.RefreshToken);
            }
            
            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<LoginResponse>.Success(result.Data, 200));
            }
            else
            {
                return BadRequest(ApiResponse<LoginResponse>.Error(result.ErrorMessage ?? "Login failed", 400));
            }
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
            
            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<LogoutResultDto>.Success(result.Data, 200));
            }
            else
            {
                return BadRequest(ApiResponse<LogoutResultDto>.Error(result.ErrorMessage ?? "Logout failed", 400));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during logout");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred during logout", 500, ex.GetType().Name));
        }
    }
}


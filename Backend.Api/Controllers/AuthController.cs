using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.RefreshToken;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.Results;

namespace Backend.Api.Controllers;

/// <summary>
/// Authentication controller for user registration and login
/// </summary>
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator, ILogger<AuthController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
                IpAddress = registerDto.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                DeviceInfo = registerDto.DeviceInfo ??
                             GetDeviceInfoFromUserAgent(HttpContext.Request.Headers["User-Agent"].ToString())
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
                IpAddress = loginDto.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                DeviceInfo = loginDto.DeviceInfo ??
                             GetDeviceInfoFromUserAgent(HttpContext.Request.Headers["User-Agent"].ToString())
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
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
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
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
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

    /// <summary>
    /// Extract device information from User-Agent string
    /// </summary>
    private string GetDeviceInfoFromUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown Device";

        try
        {
            var deviceInfo = new List<string>();

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                deviceInfo.Add("Mobile");
            else if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
                deviceInfo.Add("Tablet");
            else
                deviceInfo.Add("Desktop");

            if (userAgent.Contains("Windows"))
                deviceInfo.Add("Windows");
            else if (userAgent.Contains("Mac OS"))
                deviceInfo.Add("macOS");
            else if (userAgent.Contains("Linux"))
                deviceInfo.Add("Linux");
            else if (userAgent.Contains("Android"))
                deviceInfo.Add("Android");
            else if (userAgent.Contains("iOS"))
                deviceInfo.Add("iOS");

            if (userAgent.Contains("Chrome"))
                deviceInfo.Add("Chrome");
            else if (userAgent.Contains("Firefox"))
                deviceInfo.Add("Firefox");
            else if (userAgent.Contains("Safari"))
                deviceInfo.Add("Safari");
            else if (userAgent.Contains("Edge"))
                deviceInfo.Add("Edge");

            return string.Join(" | ", deviceInfo);
        }
        catch
        {
            return "Unknown Device";
        }
    }
}

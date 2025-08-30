using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.RefreshToken;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers;

/// <summary>
/// Authentication controller for user registration and login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registerDto">Registration request</param>
    /// <returns>Authentication result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            // Create command from DTO
            var command = new RegisterCommand
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                Password = registerDto.Password,
                ConfirmPassword = registerDto.ConfirmPassword,
                PhoneNumber = registerDto.PhoneNumber,
                AcceptTerms = registerDto.AcceptTerms,
                SubscribeToNewsletter = registerDto.SubscribeToNewsletter,
                CaptchaChallengeId = registerDto.CaptchaChallengeId,
                CaptchaAnswer = registerDto.CaptchaAnswer,
                IpAddress = registerDto.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                DeviceInfo = registerDto.DeviceInfo ??
                             GetDeviceInfoFromUserAgent(HttpContext.Request.Headers["User-Agent"].ToString())
            };

            // Send command through mediator
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = result.ErrorMessage,
                Status = 400,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during registration");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during registration",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <param name="loginDto">Login request</param>
    /// <returns>Authentication result</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            // Create command from DTO
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

            // Send command through mediator
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Login Failed",
                Detail = result.ErrorMessage,
                Status = 400,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during login");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during login",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token request</param>
    /// <returns>Authentication result with new tokens</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            // Create command from DTO
            var command = new RefreshTokenCommand
            {
                RefreshToken = refreshTokenDto.RefreshToken,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            };

            // Send command through mediator
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Token Refresh Failed",
                Detail = result.ErrorMessage,
                Status = 400,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during token refresh");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during token refresh",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Logout user and invalidate refresh tokens
    /// </summary>
    /// <param name="logoutDto">Logout request</param>
    /// <returns>Logout result</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        try
        {
            // Create command from DTO
            var command = new LogoutCommand
            {
                RefreshToken = logoutDto.RefreshToken,
                LogoutFromAllDevices = logoutDto.LogoutFromAllDevices,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                LogoutReason = "User initiated logout"
            };

            // Send command through mediator
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Logout Failed",
                Detail = result.ErrorMessage,
                Status = 400,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during logout");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during logout",
                Status = 500
            });
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

                // Check for mobile devices
                if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                    deviceInfo.Add("Mobile");
                else if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
                    deviceInfo.Add("Tablet");
                else
                    deviceInfo.Add("Desktop");

                // Check for operating system
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

                // Check for browser
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



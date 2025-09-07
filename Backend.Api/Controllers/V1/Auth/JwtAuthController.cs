using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.RefreshToken;
using Backend.Application.Features.UserManagement.Commands.Logout;
using MediatR;
using System.Security.Claims;

namespace Backend.Api.Controllers.V1.Auth
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class JwtAuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<JwtAuthController> _logger;

        public JwtAuthController(
            IMediator mediator,
            ILogger<JwtAuthController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Login with username/email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var loginCommand = new LoginCommand
                {
                    EmailOrUsername = request.EmailOrUsername,
                    Password = request.Password,
                    RememberMe = request.RememberMe,
                    TwoFactorCode = request.TwoFactorCode,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    DeviceInfo = Request.Headers.UserAgent.ToString()
                };

                var result = await _mediator.Send(loginCommand, cancellationToken);

                _logger.LogInformation("Controller result - IsSuccess: {IsSuccess}, ErrorMessage: {ErrorMessage}, Data: {Data}, DataType: {DataType}", 
                    result.IsSuccess, result.ErrorMessage, 
                    result.Data != null ? "Not null" : "NULL",
                    result.Data?.GetType().Name ?? "NULL");

                var apiResponse = ApiResponse.FromResult(result, result.IsSuccess ? 200 : 401);
                
                _logger.LogInformation("ApiResponse after FromResult - IsSuccess: {IsSuccess}, StatusCode: {StatusCode}, Data: {Data}, DataType: {DataType}", 
                    apiResponse.IsSuccess, apiResponse.StatusCode, 
                    apiResponse.Data != null ? "Not null" : "NULL",
                    apiResponse.Data?.GetType().Name ?? "NULL");
                
                if (result.IsSuccess)
                {
                    return Ok(apiResponse);
                }
                else
                {
                    return Unauthorized(apiResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<LoginResponse>.Failure(ex, 500);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshTokenCommand = new RefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                };

                var result = await _mediator.Send(refreshTokenCommand, cancellationToken);

                var apiResponse = ApiResponse.FromResult(result, result.IsSuccess ? 200 : 401);
                
                if (result.IsSuccess)
                {
                    return Ok(apiResponse);
                }
                else
                {
                    return Unauthorized(apiResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<LoginResponse>.Failure(ex, 500);
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<LogoutResultDto>>> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var logoutCommand = new LogoutCommand
                {
                    RefreshToken = request.RefreshToken,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                };

                var result = await _mediator.Send(logoutCommand, cancellationToken);

                var apiResponse = ApiResponse.FromResult(result, result.IsSuccess ? 200 : 401);
                
                if (result.IsSuccess)
                {
                    return Ok(apiResponse);
                }
                else
                {
                    return Unauthorized(apiResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<LogoutResultDto>.Failure(ex, 500);
                return StatusCode(500, errorResponse);
            }
        }
    }

    // Request/Response DTOs
    public class LoginRequest
    {
        public string EmailOrUsername { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
        public string? TwoFactorCode { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }

}
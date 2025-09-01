using Asp.Versioning;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.RefreshToken;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.V1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class TokenController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly ILogger<TokenController> _logger;

    public TokenController(IMediator mediator, IDeviceInfoService deviceInfoService, ILogger<TokenController> logger)
    {
        _mediator = mediator;
        _deviceInfoService = deviceInfoService;
        _logger = logger;
    }

    [HttpPost("refresh-token")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
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
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during token refresh");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred during token refresh", 500, ex.GetType().Name));
        }
    }
}


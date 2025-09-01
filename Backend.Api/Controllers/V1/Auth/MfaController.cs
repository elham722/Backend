using Asp.Versioning;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.MFA;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.Queries.MFA;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers.V1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth/mfa")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly ILogger<MfaController> _logger;

    public MfaController(IMediator mediator, IDeviceInfoService deviceInfoService, ILogger<MfaController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _deviceInfoService = deviceInfoService ?? throw new ArgumentNullException(nameof(deviceInfoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MfaSetupDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> GetMfaMethods([FromQuery] bool? isEnabled = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Unauthorized");

            var query = new GetMfaMethodsQuery { UserId = userId, IsEnabled = isEnabled };
            var result = await _mediator.Send(query);

            return Ok(ApiResponse.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA methods");
            return StatusCode(500, ApiResponse.Error("Error getting MFA methods", 500, ex.GetType().Name));
        }
    }

    [HttpPost("setup")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<MfaSetupDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> SetupMfa([FromBody] SetupMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Unauthorized");

            var command = new SetupMfaCommand
            {
                UserId = userId,
                Type = request.Type,
                PhoneNumber = request.PhoneNumber,
                IpAddress = _deviceInfoService.GetIpAddress(),
                DeviceInfo = _deviceInfoService.GetDeviceInfo()
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.Success(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MFA");
            return StatusCode(500, ApiResponse.Error("Error setting up MFA", 500, ex.GetType().Name));
        }
    }

    [HttpPost("verify")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<MfaVerificationResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Unauthorized");

            var command = new VerifyMfaCommand
            {
                UserId = userId,
                Type = request.Type,
                Code = request.Code,
                DeviceInfo = _deviceInfoService.GetDeviceInfo(),
                IpAddress = _deviceInfoService.GetIpAddress(),
                RememberDevice = request.RememberDevice
            };

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(ApiResponse.Success(result))
                : BadRequest(ApiResponse.Error("MFA verification failed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA");
            return StatusCode(500, ApiResponse.Error("Error verifying MFA", 500, ex.GetType().Name));
        }
    }

    [HttpPost("disable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> DisableMfa([FromBody] DisableMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Unauthorized");

            var command = new DisableMfaCommand
            {
                UserId = userId,
                Type = request.Type,
                IpAddress = _deviceInfoService.GetIpAddress(),
                DeviceInfo = _deviceInfoService.GetDeviceInfo()
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA");
            return StatusCode(500, ApiResponse.Error("Error disabling MFA", 500, ex.GetType().Name));
        }
    }

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<MfaStatusDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> GetMfaStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Unauthorized");

            var query = new GetMfaMethodsQuery { UserId = userId, IsEnabled = true };
            var enabledMethods = await _mediator.Send(query);

            var status = new MfaStatusDto
            {
                UserId = userId,
                HasEnabledMfa = enabledMethods.Any(),
                EnabledMethods = enabledMethods.Select(m => m.Type).ToList(),
                TotalMethods = enabledMethods.Count()
            };

            return Ok(ApiResponse.Success(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA status");
            return StatusCode(500, ApiResponse.Error("Error getting MFA status", 500, ex.GetType().Name));
        }
    }

    #region Helpers

    private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    #endregion
}

public class SetupMfaRequest
{
    public MfaType Type { get; set; }
    public string? PhoneNumber { get; set; }
}

public class VerifyMfaRequest
{
    public MfaType Type { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool RememberDevice { get; set; }
}

public class DisableMfaRequest
{
    public MfaType Type { get; set; }
}

public class MfaStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public bool HasEnabledMfa { get; set; }
    public List<MfaType> EnabledMethods { get; set; } = new();
    public int TotalMethods { get; set; }
} 
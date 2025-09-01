using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.MFA;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.Queries.MFA;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MfaController : BaseApiController
{
    private readonly IMediator _mediator;

    public MfaController(IMediator mediator, ILogger<MfaController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
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
            return InternalServerError(ex, "Error getting MFA methods");
        }
    }

    [HttpPost("setup")]
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
                IpAddress = GetClientIpAddress(),
                DeviceInfo = GetUserAgent()
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
            return InternalServerError(ex, "Error setting up MFA");
        }
    }

    [HttpPost("verify")]
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
                DeviceInfo = GetUserAgent(),
                IpAddress = GetClientIpAddress(),
                RememberDevice = request.RememberDevice
            };

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(ApiResponse.Success(result))
                : BadRequest(ApiResponse.Error("MFA verification failed"));
        }
        catch (Exception ex)
        {
            return InternalServerError(ex, "Error verifying MFA");
        }
    }

    [HttpPost("disable")]
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
                IpAddress = GetClientIpAddress(),
                DeviceInfo = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.Success(result));
        }
        catch (Exception ex)
        {
            return InternalServerError(ex, "Error disabling MFA");
        }
    }

    [HttpGet("status")]
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
            return InternalServerError(ex, "Error getting MFA status");
        }
    }

    #region Helpers

    private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string GetClientIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ??
        HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
        "Unknown";
    private string GetUserAgent() =>
        HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

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

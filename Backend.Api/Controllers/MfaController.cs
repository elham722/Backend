using Backend.Application.Features.UserManagement.Commands.MFA;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.Queries.MFA;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers;

/// <summary>
/// MFA (Multi-Factor Authentication) controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MfaController> _logger;

    public MfaController(IMediator mediator, ILogger<MfaController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get MFA methods for the authenticated user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MfaSetupDto>>> GetMfaMethods([FromQuery] bool? isEnabled = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetMfaMethodsQuery
            {
                UserId = userId,
                IsEnabled = isEnabled
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA methods for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Setup MFA for the authenticated user
    /// </summary>
    [HttpPost("setup")]
    public async Task<ActionResult<MfaSetupDto>> SetupMfa([FromBody] SetupMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new SetupMfaCommand
            {
                UserId = userId,
                Type = request.Type,
                PhoneNumber = request.PhoneNumber,
                IpAddress = GetClientIpAddress(),
                DeviceInfo = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid MFA setup request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MFA");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Verify MFA code for the authenticated user
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<MfaVerificationResultDto>> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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
            
            if (result.IsSuccess)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Disable MFA for the authenticated user
    /// </summary>
    [HttpPost("disable")]
    public async Task<ActionResult<bool>> DisableMfa([FromBody] DisableMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new DisableMfaCommand
            {
                UserId = userId,
                Type = request.Type,
                IpAddress = GetClientIpAddress(),
                DeviceInfo = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get MFA setup status for the authenticated user
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<MfaStatusDto>> GetMfaStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetMfaMethodsQuery
            {
                UserId = userId,
                IsEnabled = true
            };

            var enabledMethods = await _mediator.Send(query);
            var status = new MfaStatusDto
            {
                UserId = userId,
                HasEnabledMfa = enabledMethods.Any(),
                EnabledMethods = enabledMethods.Select(m => m.Type).ToList(),
                TotalMethods = enabledMethods.Count()
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA status");
            return StatusCode(500, "Internal server error");
        }
    }

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? 
               HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
               "Unknown";
    }

    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }

    #endregion
}

#region Request/Response DTOs

/// <summary>
/// Request DTO for MFA setup
/// </summary>
public class SetupMfaRequest
{
    public MfaType Type { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Request DTO for MFA verification
/// </summary>
public class VerifyMfaRequest
{
    public MfaType Type { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool RememberDevice { get; set; }
}

/// <summary>
/// Request DTO for MFA disable
/// </summary>
public class DisableMfaRequest
{
    public MfaType Type { get; set; }
}

/// <summary>
/// Response DTO for MFA status
/// </summary>
public class MfaStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public bool HasEnabledMfa { get; set; }
    public List<MfaType> EnabledMethods { get; set; } = new();
    public int TotalMethods { get; set; }
}

#endregion 
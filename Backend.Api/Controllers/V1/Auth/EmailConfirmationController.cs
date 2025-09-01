using Asp.Versioning;
using Backend.Application.Common.Results;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.V1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth/email")]
public class EmailConfirmationController : ControllerBase
{
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<EmailConfirmationController> _logger;

    public EmailConfirmationController(
        IEmailConfirmationService emailConfirmationService,
        ILogger<EmailConfirmationController> logger)
    {
        _emailConfirmationService = emailConfirmationService ?? throw new ArgumentNullException(nameof(emailConfirmationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("confirm")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(ApiResponse.Error("User ID and token are required"));

            var result = await _emailConfirmationService.ConfirmEmailAsync(userId, token);

            if (result)
                return Ok(ApiResponse.Success(new { Message = "Email confirmed successfully", IsConfirmed = true }));

            return BadRequest(ApiResponse.Error("Invalid or expired confirmation token"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while confirming email");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred while confirming email", 500, ex.GetType().Name));
        }
    }

    [HttpPost("resend")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(ApiResponse.Error("Email address is required"));

            var result = await _emailConfirmationService.ResendEmailConfirmationAsync(request.Email);

            if (result)
                return Ok(ApiResponse.Success(new { Message = "Email confirmation resent successfully", IsResent = true }));

            return BadRequest(ApiResponse.Error("Failed to resend email confirmation"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while resending email confirmation");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred while resending email confirmation", 500, ex.GetType().Name));
        }
    }

    [HttpGet("status/{userId}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> GetEmailConfirmationStatus(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse.Error("User ID is required"));

            var isConfirmed = await _emailConfirmationService.IsEmailConfirmedAsync(userId);

            return Ok(ApiResponse.Success(new
            {
                UserId = userId,
                IsEmailConfirmed = isConfirmed
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while checking email confirmation status");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred while checking email confirmation status", 500, ex.GetType().Name));
        }
    }
}

public class ResendEmailConfirmationRequest
{
    public string Email { get; set; } = string.Empty;
} 
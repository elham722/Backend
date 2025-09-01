using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailConfirmationController : BaseApiController
{
    private readonly IEmailConfirmationService _emailConfirmationService;

    public EmailConfirmationController(
        IEmailConfirmationService emailConfirmationService,
        ILogger<EmailConfirmationController> logger) : base(logger)
    {
        _emailConfirmationService = emailConfirmationService ?? throw new ArgumentNullException(nameof(emailConfirmationService));
    }

    [HttpGet("confirm")]
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
            return InternalServerError(ex, "An unexpected error occurred while confirming email");
        }
    }

    [HttpPost("resend")]
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
            return InternalServerError(ex, "An unexpected error occurred while resending email confirmation");
        }
    }

    [HttpGet("status/{userId}")]
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
            return InternalServerError(ex, "An unexpected error occurred while checking email confirmation status");
        }
    }
}

public class ResendEmailConfirmationRequest
{
    public string Email { get; set; } = string.Empty;
}

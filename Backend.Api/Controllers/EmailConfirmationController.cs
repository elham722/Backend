using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// Controller for email confirmation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
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

    /// <summary>
    /// Confirm user's email address
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Confirmation token</param>
    /// <returns>Confirmation result</returns>
    [HttpGet("confirm")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "User ID and token are required",
                    Status = 400
                });
            }

            var result = await _emailConfirmationService.ConfirmEmailAsync(userId, token);
            
            if (result)
            {
                return Ok(new
                {
                    Message = "Email confirmed successfully",
                    IsConfirmed = true
                });
            }
            else
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Email Confirmation Failed",
                    Detail = "Invalid or expired confirmation token",
                    Status = 400
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user {UserId}", userId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while confirming email",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Resend result</returns>
    [HttpPost("resend")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "Email address is required",
                    Status = 400
                });
            }

            var result = await _emailConfirmationService.ResendEmailConfirmationAsync(request.Email);
            
            if (result)
            {
                return Ok(new
                {
                    Message = "Email confirmation resent successfully",
                    IsResent = true
                });
            }
            else
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Resend Failed",
                    Detail = "Failed to resend email confirmation",
                    Status = 400
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation for email {Email}", request.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while resending email confirmation",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Check email confirmation status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Confirmation status</returns>
    [HttpGet("status/{userId}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> GetEmailConfirmationStatus(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "User ID is required",
                    Status = 400
                });
            }

            var isConfirmed = await _emailConfirmationService.IsEmailConfirmedAsync(userId);
            
            return Ok(new
            {
                UserId = userId,
                IsEmailConfirmed = isConfirmed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email confirmation status for user {UserId}", userId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while checking email confirmation status",
                Status = 500
            });
        }
    }
}

/// <summary>
/// Request model for resending email confirmation
/// </summary>
public class ResendEmailConfirmationRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
} 
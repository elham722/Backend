using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// Controller for CAPTCHA-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captchaService;
    private readonly ILogger<CaptchaController> _logger;

    public CaptchaController(
        ICaptchaService captchaService,
        ILogger<CaptchaController> logger)
    {
        _captchaService = captchaService ?? throw new ArgumentNullException(nameof(captchaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get CAPTCHA configuration for client
    /// </summary>
    /// <returns>CAPTCHA configuration</returns>
    [HttpGet("config")]
    [ProducesResponseType(typeof(CaptchaConfiguration), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public IActionResult GetConfiguration()
    {
        try
        {
            var config = _captchaService.GetConfiguration();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CAPTCHA configuration");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while getting CAPTCHA configuration",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Generate a new CAPTCHA challenge
    /// </summary>
    /// <returns>CAPTCHA challenge</returns>
    [HttpGet("generate")]
    [ProducesResponseType(typeof(CaptchaChallenge), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> GenerateChallenge()
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var challenge = await _captchaService.GenerateChallengeAsync(ipAddress);
            
            return Ok(challenge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CAPTCHA challenge");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while generating CAPTCHA challenge",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Check if CAPTCHA is required for the current request
    /// </summary>
    /// <param name="action">Action being performed (e.g., "register", "login")</param>
    /// <returns>Whether CAPTCHA is required</returns>
    [HttpGet("required")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> IsRequired([FromQuery] string action = "default")
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var isRequired = await _captchaService.IsRequiredAsync(ipAddress, action);
            
            return Ok(new { IsRequired = isRequired });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if CAPTCHA is required");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while checking CAPTCHA requirement",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Validate CAPTCHA answer
    /// </summary>
    /// <param name="request">CAPTCHA validation request</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(CaptchaValidationResult), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Validate([FromBody] CaptchaValidationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ChallengeId) || string.IsNullOrEmpty(request.Answer))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "Challenge ID and answer are required",
                    Status = 400
                });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _captchaService.ValidateAsync(request.ChallengeId, request.Answer, ipAddress);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CAPTCHA answer");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while validating CAPTCHA",
                Status = 500
            });
        }
    }
}

/// <summary>
/// Request model for CAPTCHA validation
/// </summary>
public class CaptchaValidationRequest
{
    /// <summary>
    /// CAPTCHA challenge ID
    /// </summary>
    public string ChallengeId { get; set; } = string.Empty;

    /// <summary>
    /// CAPTCHA answer
    /// </summary>
    public string Answer { get; set; } = string.Empty;
} 
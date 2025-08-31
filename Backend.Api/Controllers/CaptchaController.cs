using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// Controller for CAPTCHA-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaServiceFactory _captchaServiceFactory;
    private readonly ILogger<CaptchaController> _logger;

    public CaptchaController(
        ICaptchaServiceFactory captchaServiceFactory,
        ILogger<CaptchaController> logger)
    {
        _captchaServiceFactory = captchaServiceFactory ?? throw new ArgumentNullException(nameof(captchaServiceFactory));
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
            var captchaService = _captchaServiceFactory.CreateCaptchaService();
            var config = captchaService.GetConfiguration();
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
    [HttpPost("generate")]
    [ProducesResponseType(typeof(CaptchaChallenge), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> GenerateChallenge()
    {
        try
        {
            var captchaService = _captchaServiceFactory.CreateCaptchaService();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var challenge = await captchaService.GenerateChallengeAsync(ipAddress);
            
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

            var captchaService = _captchaServiceFactory.CreateCaptchaService();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await captchaService.ValidateAsync(request.ChallengeId, request.Answer, ipAddress);
            
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

    /// <summary>
    /// Validate Google reCAPTCHA token directly
    /// </summary>
    /// <param name="request">reCAPTCHA validation request</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate-google")]
    [ProducesResponseType(typeof(CaptchaValidationResult), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> ValidateGoogleReCaptcha([FromBody] GoogleReCaptchaValidationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "reCAPTCHA token is required",
                    Status = 400
                });
            }

            var captchaService = _captchaServiceFactory.CreateCaptchaService();
            var ipAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // For Google reCAPTCHA, we use the token as both challengeId and answer
            var result = await captchaService.ValidateAsync("google-recaptcha", request.Token, ipAddress);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google reCAPTCHA token");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while validating reCAPTCHA",
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
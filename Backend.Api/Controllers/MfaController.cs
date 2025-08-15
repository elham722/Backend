using Backend.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Identity.Models;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MfaController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITotpService _totpService;
        private readonly ISmsService _smsService;
        private readonly ILogger<MfaController> _logger;

        public MfaController(
            UserManager<ApplicationUser> userManager,
            ITotpService totpService,
            ISmsService smsService,
            ILogger<MfaController> logger)
        {
            _userManager = userManager;
            _totpService = totpService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpPost("setup-totp")]
        public async Task<IActionResult> SetupTotp()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                // Generate new TOTP secret
                var secretKey = _totpService.GenerateSecretKey();
                var qrCodeUrl = _totpService.GenerateQrCodeUrl(secretKey, user.UserName);

                return Ok(new
                {
                    IsSuccess = true,
                    SecretKey = secretKey,
                    QrCodeUrl = qrCodeUrl,
                    Message = "TOTP setup initiated. Scan the QR code with your authenticator app."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up TOTP");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to setup TOTP" });
            }
        }

        [HttpPost("enable-totp")]
        public async Task<IActionResult> EnableTotp([FromBody] EnableTotpRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                // Validate the TOTP code
                if (!_totpService.ValidateCode(request.SecretKey, request.Code))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Invalid TOTP code" });
                }

                // Enable TOTP for the user
                user.EnableTotp(request.SecretKey);
                await _userManager.UpdateAsync(user);

                return Ok(new { IsSuccess = true, Message = "TOTP enabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling TOTP");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to enable TOTP" });
            }
        }

        [HttpPost("disable-totp")]
        public async Task<IActionResult> DisableTotp()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                // Disable TOTP for the user
                user.DisableTotp();
                await _userManager.UpdateAsync(user);

                return Ok(new { IsSuccess = true, Message = "TOTP disabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling TOTP");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to disable TOTP" });
            }
        }

        [HttpPost("send-sms-code")]
        public async Task<IActionResult> SendSmsCode()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Phone number not set" });
                }

                // Generate verification code
                var code = GenerateVerificationCode();
                
                // Store code in cache (you might want to implement this)
                // For now, we'll just send the SMS
                var success = await _smsService.SendVerificationCodeAsync(user.PhoneNumber, code);

                if (success)
                {
                    return Ok(new { IsSuccess = true, Message = "SMS verification code sent" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, Message = "Failed to send SMS code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS code");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to send SMS code" });
            }
        }

        [HttpPost("enable-sms")]
        public async Task<IActionResult> EnableSms([FromBody] EnableSmsRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                // Validate phone number
                if (!_smsService.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Invalid phone number format" });
                }

                // Update phone number and enable SMS
                user.PhoneNumber = _smsService.FormatPhoneNumber(request.PhoneNumber);
                user.EnableSms();
                await _userManager.UpdateAsync(user);

                return Ok(new { IsSuccess = true, Message = "SMS enabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling SMS");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to enable SMS" });
            }
        }

        [HttpPost("disable-sms")]
        public async Task<IActionResult> DisableSms()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                // Disable SMS for the user
                user.DisableSms();
                await _userManager.UpdateAsync(user);

                return Ok(new { IsSuccess = true, Message = "SMS disabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling SMS");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to disable SMS" });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetMfaStatus()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                return Ok(new
                {
                    IsSuccess = true,
                    TotpEnabled = user.TotpEnabled,
                    SmsEnabled = user.SmsEnabled,
                    PhoneNumber = user.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA status");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to get MFA status" });
            }
        }

        private static string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }

    public class EnableTotpRequest
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class EnableSmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }
} 
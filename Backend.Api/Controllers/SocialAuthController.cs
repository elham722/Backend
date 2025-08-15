using Backend.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Identity.Models;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISocialAuthService _socialAuthService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<SocialAuthController> _logger;

        public SocialAuthController(
            UserManager<ApplicationUser> userManager,
            ISocialAuthService socialAuthService,
            IJwtService jwtService,
            ILogger<SocialAuthController> logger)
        {
            _userManager = userManager;
            _socialAuthService = socialAuthService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new { IsSuccess = false, Message = "ID token is required" });
                }

                var result = await _socialAuthService.AuthenticateWithGoogleAsync(request.IdToken);
                if (!result.IsSuccess)
                {
                    return BadRequest(new { IsSuccess = false, Message = result.ErrorMessage });
                }

                // Check if user exists
                var user = await _userManager.FindByEmailAsync(result.Email);
                if (user == null)
                {
                    // Create new user
                    user = ApplicationUser.Create(result.Email!, result.UserName!);
                    user.LinkGoogleAccount(result.ProviderUserId!);
                    
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest(new { IsSuccess = false, Message = "Failed to create user account" });
                    }
                }
                else
                {
                    // Link existing user to Google account
                    if (string.IsNullOrEmpty(user.GoogleId))
                    {
                        user.LinkGoogleAccount(result.ProviderUserId!);
                        await _userManager.UpdateAsync(user);
                    }
                }

                // Generate JWT token
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);

                return Ok(new
                {
                    IsSuccess = true,
                    Message = "Google authentication successful",
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                    ExpiresAt = _jwtService.GetTokenExpiration(token),
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                return StatusCode(500, new { IsSuccess = false, Message = "Authentication failed" });
            }
        }

        [HttpPost("microsoft")]
        public async Task<IActionResult> MicrosoftAuth([FromBody] MicrosoftAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new { IsSuccess = false, Message = "ID token is required" });
                }

                var result = await _socialAuthService.AuthenticateWithMicrosoftAsync(request.IdToken);
                if (!result.IsSuccess)
                {
                    return BadRequest(new { IsSuccess = false, Message = result.ErrorMessage });
                }

                // Check if user exists
                var user = await _userManager.FindByEmailAsync(result.Email);
                if (user == null)
                {
                    // Create new user
                    user = ApplicationUser.Create(result.Email!, result.UserName!);
                    user.LinkMicrosoftAccount(result.ProviderUserId!);
                    
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest(new { IsSuccess = false, Message = "Failed to create user account" });
                    }
                }
                else
                {
                    // Link existing user to Microsoft account
                    if (string.IsNullOrEmpty(user.MicrosoftId))
                    {
                        user.LinkMicrosoftAccount(result.ProviderUserId!);
                        await _userManager.UpdateAsync(user);
                    }
                }

                // Generate JWT token
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);

                return Ok(new
                {
                    IsSuccess = true,
                    Message = "Microsoft authentication successful",
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                    ExpiresAt = _jwtService.GetTokenExpiration(token),
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Microsoft authentication");
                return StatusCode(500, new { IsSuccess = false, Message = "Authentication failed" });
            }
        }

        [HttpPost("unlink-google")]
        [Authorize]
        public async Task<IActionResult> UnlinkGoogle()
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

                user.UnlinkGoogleAccount();
                await _userManager.UpdateAsync(user);

                return Ok(new { IsSuccess = true, Message = "Google account unlinked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking Google account");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to unlink Google account" });
            }
        }

        [HttpPost("unlink-microsoft")]
        [Authorize]
        public async Task<IActionResult> UnlinkMicrosoft()
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

                user.UnlinkMicrosoftAccount();
                await _userManager.UpdateAsync(user);

                return Ok(new { IsSuccess = true, Message = "Microsoft account unlinked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking Microsoft account");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to unlink Microsoft account" });
            }
        }

        [HttpGet("linked-accounts")]
        [Authorize]
        public async Task<IActionResult> GetLinkedAccounts()
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
                    GoogleLinked = !string.IsNullOrEmpty(user.GoogleId),
                    MicrosoftLinked = !string.IsNullOrEmpty(user.MicrosoftId)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked accounts");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to get linked accounts" });
            }
        }
    }

    public class GoogleAuthRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public class MicrosoftAuthRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }
} 
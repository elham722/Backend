using Client.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers
{
    /// <summary>
    /// Test controller for testing authentication and refresh flow
    /// </summary>
    [Authorize]
    public class TestController : Controller
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IBackgroundJobAuthClient _backgroundAuthClient;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IUserSessionService userSessionService,
            IBackgroundJobAuthClient backgroundAuthClient,
            ILogger<TestController> logger)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            _backgroundAuthClient = backgroundAuthClient ?? throw new ArgumentNullException(nameof(backgroundAuthClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Test page to check current authentication status
        /// </summary>
        
        public IActionResult Index()
        {
            var isAuthenticated = _userSessionService.IsAuthenticated();
            var jwtToken = _userSessionService.GetJwtToken();
            var refreshToken = _userSessionService.GetRefreshToken();
            var userId = _userSessionService.GetUserId();
            var userName = _userSessionService.GetUserName();

            var model = new
            {
                IsAuthenticated = isAuthenticated,
                HasJwtToken = !string.IsNullOrEmpty(jwtToken),
                HasRefreshToken = !string.IsNullOrEmpty(refreshToken),
                UserId = userId,
                UserName = userName,
                JwtTokenLength = jwtToken?.Length ?? 0,
                RefreshTokenLength = refreshToken?.Length ?? 0
            };

            return View(model);
        }

        /// <summary>
        /// Test token validation
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var isValid = await _backgroundAuthClient.ValidateCurrentTokenAsync();
                var tokenInfo = await _backgroundAuthClient.GetTokenInfoAsync(_userSessionService.GetJwtToken() ?? "");

                var result = new
                {
                    IsValid = isValid,
                    TokenInfo = tokenInfo
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Json(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test clearing session
        /// </summary>
        [HttpPost]
        public IActionResult ClearSession()
        {
            try
            {
                _userSessionService.ClearUserSession();
                return Json(new { Success = true, Message = "Session cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session");
                return Json(new { Error = ex.Message });
            }
        }
    }
} 
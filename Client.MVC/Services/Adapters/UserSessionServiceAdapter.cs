using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Client.MVC.Services.Abstractions;

namespace Client.MVC.Services.Adapters
{
    /// <summary>
    /// Adapter that implements the old IUserSessionService interface using the new refactored services
    /// Provides backward compatibility while maintaining clean architecture and avoiding circular dependencies
    /// </summary>
    public class UserSessionServiceAdapter : IUserSessionService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ISessionManager _sessionManager;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogoutService _logoutService;
        private readonly ILogger<UserSessionServiceAdapter> _logger;

        public UserSessionServiceAdapter(
            ICurrentUser currentUser,
            ISessionManager sessionManager,
            ITokenProvider tokenProvider,
            ILogoutService logoutService,
            ILogger<UserSessionServiceAdapter> logger)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _logoutService = logoutService ?? throw new ArgumentNullException(nameof(logoutService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Session Management
        
        public void SetUserSession(LoginResponse result)
        {
            _sessionManager.SetUserSession(result);
        }

        public void SetUserSession(ApiResponse<LoginResponse> response)
        {
            _sessionManager.SetUserSession(response);
        }

        public void ClearUserSession()
        {
            _sessionManager.ClearUserSession();
        }

        public Task ClearUserSessionAsync(CancellationToken cancellationToken = default)
        {
            return _sessionManager.ClearUserSessionAsync(cancellationToken);
        }

        public Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default)
        {
            return _logoutService.LogoutAsync(logoutFromAllDevices, cancellationToken);
        }

        public LogoutDto GetLogoutDto()
        {
            return _sessionManager.GetLogoutDto();
        }

        #endregion

        #region User Information

        public string? GetUserId()
        {
            return _currentUser.GetUserId();
        }

        public string? GetUserName()
        {
            return _currentUser.GetUserName();
        }

        public string? GetUserEmail()
        {
            return _currentUser.GetUserEmail();
        }

        public bool IsAuthenticated()
        {
            return _currentUser.IsAuthenticated();
        }

        #endregion

        #region Token Management

        public string? GetJwtToken()
        {
            return _tokenProvider.GetJwtToken();
        }

        public string? GetRefreshToken()
        {
            return _tokenProvider.GetRefreshToken();
        }

        public bool IsTokenAboutToExpire(int minutesBeforeExpiry = 5)
        {
            return _tokenProvider.IsTokenAboutToExpire(minutesBeforeExpiry);
        }

        public DateTime? GetTokenExpiration()
        {
            return _tokenProvider.GetTokenExpiration();
        }

        public DateTimeOffset? GetCachedTokenExpiration(string currentToken)
        {
            return _tokenProvider.GetCachedTokenExpiration(currentToken);
        }

        public bool HasValidRefreshToken()
        {
            return _tokenProvider.HasValidRefreshToken();
        }

        public DateTime? GetRefreshTokenExpiration()
        {
            return _tokenProvider.GetRefreshTokenExpiration();
        }

        public bool IsRefreshTokenAboutToExpire(int daysBeforeExpiry = 7)
        {
            return _tokenProvider.IsRefreshTokenAboutToExpire(daysBeforeExpiry);
        }

        public string? GetRefreshTokenType()
        {
            return _tokenProvider.GetRefreshTokenType();
        }

        public void RefreshJwtToken(string newToken, DateTime? expiresAt = null)
        {
            _tokenProvider.RefreshJwtToken(newToken, expiresAt);
        }

        #endregion
    }
}
using Microsoft.AspNetCore.Http;

namespace Client.MVC.Services
{
    /// <summary>
    /// Implementation of token service for managing JWT tokens in session
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetToken()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            if (session.TryGetValue("JWTToken", out var tokenBytes))
            {
                return System.Text.Encoding.UTF8.GetString(tokenBytes);
            }
            return null;
        }

        public string? GetRefreshToken()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            if (session.TryGetValue("RefreshToken", out var tokenBytes))
            {
                return System.Text.Encoding.UTF8.GetString(tokenBytes);
            }
            return null;
        }

        public string? GetUserName()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            if (session.TryGetValue("UserName", out var userNameBytes))
            {
                return System.Text.Encoding.UTF8.GetString(userNameBytes);
            }
            return null;
        }

        public string? GetUserId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            if (session.TryGetValue("UserId", out var userIdBytes))
            {
                return System.Text.Encoding.UTF8.GetString(userIdBytes);
            }
            return null;
        }

        public DateTime? GetTokenExpiration()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            if (session.TryGetValue("TokenExpiresAt", out var expiresAtBytes))
            {
                var expiresAtString = System.Text.Encoding.UTF8.GetString(expiresAtBytes);
                if (DateTime.TryParse(expiresAtString, out var expiresAt))
                {
                    return expiresAt;
                }
            }
            return null;
        }

        public bool IsTokenExpired()
        {
            var expiration = GetTokenExpiration();
            return expiration.HasValue && expiration.Value <= DateTime.UtcNow;
        }

        public bool IsTokenExpiringSoon(int minutesThreshold = 5)
        {
            var expiration = GetTokenExpiration();
            return expiration.HasValue && expiration.Value <= DateTime.UtcNow.AddMinutes(minutesThreshold);
        }

        public void StoreTokens(string token, string refreshToken, string userName, string userId, DateTime expiresAt)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            var refreshTokenBytes = System.Text.Encoding.UTF8.GetBytes(refreshToken);
            var userNameBytes = System.Text.Encoding.UTF8.GetBytes(userName);
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId);
            var expiresAtBytes = System.Text.Encoding.UTF8.GetBytes(expiresAt.ToString("O"));

            session.Set("JWTToken", tokenBytes);
            session.Set("RefreshToken", refreshTokenBytes);
            session.Set("UserName", userNameBytes);
            session.Set("UserId", userIdBytes);
            session.Set("TokenExpiresAt", expiresAtBytes);
        }

        public void ClearTokens()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            session.Remove("JWTToken");
            session.Remove("RefreshToken");
            session.Remove("UserName");
            session.Remove("UserId");
            session.Remove("TokenExpiresAt");
        }

        public bool HasValidToken()
        {
            var token = GetToken();
            return !string.IsNullOrEmpty(token) && !IsTokenExpired();
        }
    }
} 
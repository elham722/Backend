namespace Client.MVC.Services
{
    /// <summary>
    /// Service for managing JWT tokens in session
    /// </summary>
    public interface ITokenService
    {
        string? GetToken();
        string? GetRefreshToken();
        string? GetUserName();
        string? GetUserId();
        DateTime? GetTokenExpiration();
        bool IsTokenExpired();
        bool IsTokenExpiringSoon(int minutesThreshold = 5);
        void StoreTokens(string token, string refreshToken, string userName, string userId, DateTime expiresAt);
        void ClearTokens();
        bool HasValidToken();
    }
} 
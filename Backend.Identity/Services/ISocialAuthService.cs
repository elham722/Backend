namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for social authentication service
    /// </summary>
    public interface ISocialAuthService
    {
        /// <summary>
        /// Authenticates a user with Google OAuth
        /// </summary>
        Task<SocialAuthResult> AuthenticateWithGoogleAsync(string idToken);

        /// <summary>
        /// Authenticates a user with Microsoft OAuth
        /// </summary>
        Task<SocialAuthResult> AuthenticateWithMicrosoftAsync(string idToken);

        /// <summary>
        /// Validates a Google ID token
        /// </summary>
        Task<bool> ValidateGoogleTokenAsync(string idToken);

        /// <summary>
        /// Validates a Microsoft ID token
        /// </summary>
        Task<bool> ValidateMicrosoftTokenAsync(string idToken);

        /// <summary>
        /// Gets user information from Google
        /// </summary>
        Task<SocialUserInfo?> GetGoogleUserInfoAsync(string accessToken);

        /// <summary>
        /// Gets user information from Microsoft
        /// </summary>
        Task<SocialUserInfo?> GetMicrosoftUserInfoAsync(string accessToken);
    }

    /// <summary>
    /// Result of social authentication
    /// </summary>
    public class SocialAuthResult
    {
        public bool IsSuccess { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PictureUrl { get; set; }
        public string? Provider { get; set; }
        public string? ProviderUserId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Social user information
    /// </summary>
    public class SocialUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
} 
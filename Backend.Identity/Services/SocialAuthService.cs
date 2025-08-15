using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of social authentication service
    /// </summary>
    public class SocialAuthService : ISocialAuthService
    {
        private readonly ILogger<SocialAuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _googleClientId;
        private readonly string _microsoftClientId;

        public SocialAuthService(
            ILogger<SocialAuthService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _googleClientId = configuration["OAuth:Google:ClientId"] ?? string.Empty;
            _microsoftClientId = configuration["OAuth:Microsoft:ClientId"] ?? string.Empty;
        }

        public async Task<SocialAuthResult> AuthenticateWithGoogleAsync(string idToken)
        {
            try
            {
                if (!await ValidateGoogleTokenAsync(idToken))
                {
                    return new SocialAuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid Google ID token"
                    };
                }

                var userInfo = await GetGoogleUserInfoAsync(idToken);
                if (userInfo == null)
                {
                    return new SocialAuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to get user information from Google"
                    };
                }

                return new SocialAuthResult
                {
                    IsSuccess = true,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName,
                    Email = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    PictureUrl = userInfo.PictureUrl,
                    Provider = "Google",
                    ProviderUserId = userInfo.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Google");
                return new SocialAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed"
                };
            }
        }

        public async Task<SocialAuthResult> AuthenticateWithMicrosoftAsync(string idToken)
        {
            try
            {
                if (!await ValidateMicrosoftTokenAsync(idToken))
                {
                    return new SocialAuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid Microsoft ID token"
                    };
                }

                var userInfo = await GetMicrosoftUserInfoAsync(idToken);
                if (userInfo == null)
                {
                    return new SocialAuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to get user information from Microsoft"
                    };
                }

                return new SocialAuthResult
                {
                    IsSuccess = true,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName,
                    Email = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    PictureUrl = userInfo.PictureUrl,
                    Provider = "Microsoft",
                    ProviderUserId = userInfo.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Microsoft");
                return new SocialAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed"
                };
            }
        }

        public async Task<bool> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                // In a real implementation, you would validate the token with Google's API
                // For now, we'll simulate validation
                _logger.LogInformation("Validating Google ID token");
                
                // Simulate API call
                await Task.Delay(100);
                
                return !string.IsNullOrEmpty(idToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return false;
            }
        }

        public async Task<bool> ValidateMicrosoftTokenAsync(string idToken)
        {
            try
            {
                // In a real implementation, you would validate the token with Microsoft's API
                // For now, we'll simulate validation
                _logger.LogInformation("Validating Microsoft ID token");
                
                // Simulate API call
                await Task.Delay(100);
                
                return !string.IsNullOrEmpty(idToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Microsoft token");
                return false;
            }
        }

        public async Task<SocialUserInfo?> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                // In a real implementation, you would call Google's userinfo endpoint
                // For now, we'll simulate the response
                _logger.LogInformation("Getting Google user info");
                
                // Simulate API call
                await Task.Delay(100);
                
                return new SocialUserInfo
                {
                    Id = "google_user_id",
                    UserName = "google_user",
                    Email = "user@gmail.com",
                    FirstName = "John",
                    LastName = "Doe",
                    PictureUrl = "https://example.com/avatar.jpg",
                    Provider = "Google"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google user info");
                return null;
            }
        }

        public async Task<SocialUserInfo?> GetMicrosoftUserInfoAsync(string accessToken)
        {
            try
            {
                // In a real implementation, you would call Microsoft's userinfo endpoint
                // For now, we'll simulate the response
                _logger.LogInformation("Getting Microsoft user info");
                
                // Simulate API call
                await Task.Delay(100);
                
                return new SocialUserInfo
                {
                    Id = "microsoft_user_id",
                    UserName = "microsoft_user",
                    Email = "user@outlook.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    PictureUrl = "https://example.com/avatar.jpg",
                    Provider = "Microsoft"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Microsoft user info");
                return null;
            }
        }
    }
} 
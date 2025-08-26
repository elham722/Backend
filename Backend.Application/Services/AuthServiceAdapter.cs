using Backend.Application.Common.Interfaces;
using Backend.Identity.Services;
using Backend.Identity.DTOs;

namespace Backend.Application.Services
{
    /// <summary>
    /// Adapter that bridges the Application layer with the Identity layer
    /// This follows the Adapter pattern to maintain proper layer separation
    /// </summary>
    public class AuthServiceAdapter : IAuthService
    {
        private readonly Backend.Identity.Services.IAuthService _identityAuthService;

        public AuthServiceAdapter(Backend.Identity.Services.IAuthService identityAuthService)
        {
            _identityAuthService = identityAuthService;
        }

        public async Task<AuthResult> LoginAsync(string userName, string password, bool rememberMe)
        {
            return await _identityAuthService.LoginAsync(userName, password, rememberMe);
        }

        public async Task<AuthResult> RegisterAsync(string userName, string email, string password, string? phoneNumber)
        {
            return await _identityAuthService.RegisterAsync(userName, email, password, phoneNumber);
        }

        public async Task<bool> LogoutAsync()
        {
            return await _identityAuthService.LogoutAsync();
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string userId)
        {
            return await _identityAuthService.RefreshTokenAsync(refreshToken, userId);
        }

        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            return await _identityAuthService.GetUserProfileAsync(userId);
        }

        public async Task<IEnumerable<string>?> GetUserRolesAsync(string userId)
        {
            return await _identityAuthService.GetUserRolesAsync(userId);
        }
    }
} 
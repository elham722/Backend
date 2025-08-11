using Backend.Application.Common.Results;

namespace Backend.Application.Features.Auth.Commands.Login
{
    public class LoginResult : Result
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsLockedOut { get; set; }
        public bool RequiresTwoFactor { get; set; }

        private LoginResult(bool isSuccess, string? errorMessage = null, string? errorCode = null) 
            : base(isSuccess, errorMessage, errorCode)
        {
        }

        public static LoginResult Success(string userId, string userName, string email, string token, DateTime expiresAt)
        {
            return new LoginResult(true)
            {
                UserId = userId,
                UserName = userName,
                Email = email,
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public static LoginResult Failure(string message, string? errorCode = null)
        {
            return new LoginResult(false, message, errorCode);
        }

        public static LoginResult LockedOut()
        {
            return new LoginResult(false, "Account is locked out", "ACCOUNT_LOCKED");
        }

        public static LoginResult RequiresTwoFactorAuth()
        {
            return new LoginResult(false, "Two-factor authentication is required", "REQUIRES_2FA");
        }
    }
} 
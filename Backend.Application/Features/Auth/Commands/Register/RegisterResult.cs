using Backend.Application.Common.Results;

namespace Backend.Application.Features.Auth.Commands.Register
{
    public class RegisterResult : Result
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }

        private RegisterResult(bool isSuccess, string? errorMessage = null, string? errorCode = null) 
            : base(isSuccess, errorMessage, errorCode)
        {
        }

        public static RegisterResult Success(string userId, string userName, string email, string token, DateTime expiresAt)
        {
            return new RegisterResult(true)
            {
                UserId = userId,
                UserName = userName,
                Email = email,
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public static RegisterResult Failure(string message, string? errorCode = null)
        {
            return new RegisterResult(false, message, errorCode);
        }
    }
} 
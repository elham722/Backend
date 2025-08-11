using Backend.Application.Common.Commands;

namespace Backend.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : ICommand<RegisterResult>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
} 
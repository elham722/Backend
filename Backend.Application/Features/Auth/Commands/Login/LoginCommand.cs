using Backend.Application.Common.Commands;

namespace Backend.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : ICommand<LoginResult>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }
} 
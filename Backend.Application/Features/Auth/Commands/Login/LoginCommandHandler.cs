using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResult>
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IAuthService authService,
            ILogger<LoginCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing login command for user: {UserName}", command.UserName);

                var authResult = await _authService.LoginAsync(command.UserName, command.Password, command.RememberMe);

                if (authResult.IsSuccess)
                {
                    return LoginResult.Success(
                        authResult.UserId!,
                        authResult.UserName!,
                        authResult.Email!,
                        authResult.Token!,
                        authResult.ExpiresAt!.Value,
                        authResult.Roles ?? new List<string>()
                    );
                }

                if (authResult.IsLockedOut)
                {
                    return LoginResult.LockedOut();
                }

                if (authResult.RequiresTwoFactor)
                {
                    return LoginResult.RequiresTwoFactorAuth();
                }

                return LoginResult.Failure(
                    authResult.Message ?? "Login failed",
                    authResult.Errors?.FirstOrDefault()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing login command for user: {UserName}", command.UserName);
                return LoginResult.Failure("An error occurred during login", "LOGIN_ERROR");
            }
        }
    }
} 
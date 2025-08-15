using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResult>
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IAuthService authService,
            ILogger<RegisterCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<RegisterResult> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing register command for user: {UserName}, Email: {Email}", 
                    command.UserName, command.Email);

                var authResult = await _authService.RegisterAsync(
                    command.UserName, 
                    command.Email, 
                    command.Password, 
                    command.PhoneNumber);

                if (authResult.IsSuccess)
                {
                    return RegisterResult.Success(
                        authResult.UserId!,
                        authResult.UserName!,
                        authResult.Email!,
                        authResult.Token!,
                        authResult.ExpiresAt!.Value,
                        authResult.Roles ?? new List<string>()
                    );
                }

                return RegisterResult.Failure(
                    authResult.Message ?? "Registration failed",
                    authResult.Errors?.FirstOrDefault()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing register command for user: {UserName}", command.UserName);
                return RegisterResult.Failure("An error occurred during registration", "REGISTRATION_ERROR");
            }
        }
    }
} 
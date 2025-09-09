using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Interfaces.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Backend.Application.Common.Interfaces;

namespace Backend.Application.Features.UserManagement.Commands.Logout
{
    /// <summary>
    /// Handler for LogoutCommand
    /// </summary>
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<LogoutResultDto>>
    {
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly Common.Interfaces.Identity.IUserService _userService;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IRefreshTokenService refreshTokenService,
            Common.Interfaces.Identity.IUserService userService,
            ILogger<LogoutCommandHandler> logger)
        {
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<LogoutResultDto>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing logout request. LogoutFromAllDevices: {LogoutFromAllDevices}, IP: {IpAddress}", 
                    request.LogoutFromAllDevices, request.IpAddress);

                var result = new LogoutResultDto
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    LogoutTime = DateTime.UtcNow,
                    LogoutFromAllDevices = request.LogoutFromAllDevices,
                    TokensInvalidated = 0
                };

                // Extract user ID from refresh token if not provided
                string? userId = request.UserId;
                if (string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(request.RefreshToken))
                {
                    userId = ExtractUserIdFromToken(request.RefreshToken);
                }

                result.UserId = userId;

                if (request.LogoutFromAllDevices)
                {
                    // Logout from all devices
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var invalidatedCount = await _refreshTokenService.InvalidateAllTokensForUserAsync(userId, cancellationToken);
                        result.TokensInvalidated = invalidatedCount;
                        
                        _logger.LogInformation("Logged out user {UserId} from all devices. Tokens invalidated: {TokenCount}", 
                            userId, invalidatedCount);
                    }
                    else
                    {
                        _logger.LogWarning("Cannot logout from all devices: User ID not found");
                        return Result<LogoutResultDto>.Failure("User ID not found", "LOGOUT_006");
                    }
                }
                else
                {
                    // Logout from current device only
                    if (!string.IsNullOrEmpty(request.RefreshToken))
                    {
                        var isInvalidated = await _refreshTokenService.InvalidateTokenAsync(request.RefreshToken, cancellationToken);
                        if (isInvalidated)
                        {
                            result.TokensInvalidated = 1;
                            _logger.LogInformation("Successfully invalidated refresh token for user {UserId}", userId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to invalidate refresh token. Token may already be invalid or expired");
                            // Don't return error - token might already be invalid
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No refresh token provided for logout");
                        return Result<LogoutResultDto>.Failure("Refresh token is required", "LOGOUT_007");
                    }
                }

                // Log the logout event
                await LogLogoutEventAsync(userId, request, result, cancellationToken);

                _logger.LogInformation("Logout completed successfully. User: {UserId}, Tokens invalidated: {TokenCount}", 
                    userId, result.TokensInvalidated);

                return Result<LogoutResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout processing");
                return Result<LogoutResultDto>.Failure("An error occurred during logout", "LOGOUT_008");
            }
        }

        /// <summary>
        /// Extract user ID from JWT token
        /// </summary>
        private string? ExtractUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    return jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid")?.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract user ID from token");
            }
            return null;
        }

        /// <summary>
        /// Log logout event for audit purposes
        /// </summary>
        private async Task LogLogoutEventAsync(string? userId, LogoutCommand request, LogoutResultDto result, CancellationToken cancellationToken)
        {
            try
            {
                var logoutEvent = new
                {
                    UserId = userId,
                    LogoutTime = result.LogoutTime,
                    LogoutFromAllDevices = request.LogoutFromAllDevices,
                    TokensInvalidated = result.TokensInvalidated,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    LogoutReason = request.LogoutReason,
                    Success = result.IsSuccess
                };

                _logger.LogInformation("Logout event: {@LogoutEvent}", logoutEvent);

                // Here you could also save to audit log database if needed
                // await _auditService.LogEventAsync("UserLogout", logoutEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log logout event");
            }
        }
    }
} 
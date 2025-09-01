using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IUserService _userService;

    public RefreshTokenCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create DTO from command
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = request.RefreshToken
            };

            // Call service to refresh token
            var result = await _userService.RefreshTokenAsync(refreshTokenDto, request.IpAddress, request.UserAgent, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure(ex.Message, "RefreshTokenError");
        }
    }
} 
using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Features.UserManagement.Commands.Register;

/// <summary>
/// Handler for RegisterCommand
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    private readonly IUserService _userService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserService userService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<LoginResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // CAPTCHA validation is handled by CaptchaBehavior pipeline
            _logger.LogInformation("Processing registration request for user: {Email}", request.Email);

            // Create DTO from command
            var registerDto = new RegisterDto
            {
                Email = request.Email,
                UserName = request.UserName,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                PhoneNumber = request.PhoneNumber,
                AcceptTerms = request.AcceptTerms,
                SubscribeToNewsletter = request.SubscribeToNewsletter,
                CaptchaToken = request.CaptchaToken,
                IpAddress = request.IpAddress,
                DeviceInfo = request.DeviceInfo
            };

            // Call service to register user
            var result = await _userService.RegisterAsync(registerDto, request.IpAddress, request.UserAgent, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure(ex.Message, "RegisterError");
        }
    }
} 
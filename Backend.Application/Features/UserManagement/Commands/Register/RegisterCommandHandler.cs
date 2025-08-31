using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.Register;

/// <summary>
/// Handler for RegisterCommand
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResultDto>>
{
    private readonly IUserService _userService;
    private readonly ICaptchaService _captchaService;

    public RegisterCommandHandler(
        IUserService userService,
        ICaptchaService captchaService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _captchaService = captchaService ?? throw new ArgumentNullException(nameof(captchaService));
    }

    public async Task<Result<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate CAPTCHA first using Google reCAPTCHA
            var captchaResult = await _captchaService.ValidateAsync("google-recaptcha", request.CaptchaToken, request.IpAddress);
            if (!captchaResult.IsValid)
            {
                return Result<AuthResultDto>.Failure(
                    $"CAPTCHA validation failed: {captchaResult.ErrorMessage ?? "Invalid CAPTCHA"}",
                    "CaptchaValidationFailed");
            }

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
            return Result<AuthResultDto>.Failure(ex.Message, "RegisterError");
        }
    }
} 
using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces;
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

    public RegisterCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
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
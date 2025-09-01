using Asp.Versioning;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.V1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class RegistrationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(IMediator mediator, IDeviceInfoService deviceInfoService, ILogger<RegistrationController> logger)
    {
        _mediator = mediator;
        _deviceInfoService = deviceInfoService;
        _logger = logger;
    }

    [HttpPost("register")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                Password = registerDto.Password,
                ConfirmPassword = registerDto.ConfirmPassword,
                PhoneNumber = registerDto.PhoneNumber,
                AcceptTerms = registerDto.AcceptTerms,
                SubscribeToNewsletter = registerDto.SubscribeToNewsletter,
                CaptchaToken = registerDto.CaptchaToken,
                IpAddress = registerDto.IpAddress ?? _deviceInfoService.GetIpAddress(),
                UserAgent = _deviceInfoService.GetUserAgent(),
                DeviceInfo = registerDto.DeviceInfo ?? _deviceInfoService.GetDeviceInfo()
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during registration");
            return StatusCode(500, ApiResponse.Error("An unexpected error occurred during registration", 500, ex.GetType().Name));
        }
    }
}


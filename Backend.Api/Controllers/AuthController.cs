using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers;

/// <summary>
/// Authentication controller for user registration and login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <returns>Authentication result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand request)
    {
        try
        {
            // Get client information for security
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            request.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Send command through mediator
            var result = await _mediator.Send(request);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = result.ErrorMessage,
                Status = 400,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during registration",
                Status = 500
            });
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Authentication result</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Login([FromBody] LoginCommand request)
    {
        try
        {
            // Get client information for security
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            request.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Send command through mediator
            var result = await _mediator.Send(request);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Login Failed",
                Detail = result.ErrorMessage,
                Status = 400,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during login",
                Status = 500
            });
        }
    }
} 
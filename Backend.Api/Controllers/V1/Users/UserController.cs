using Asp.Versioning;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.Queries.GetUserById;
using Backend.Application.Features.UserManagement.Queries.GetUsers;
using Backend.Application.Features.UserManagement.Commands.UpdateUser;
using Backend.Application.Features.UserManagement.Commands.DeleteUser;
using Backend.Application.Features.UserManagement.Commands.ActivateUser;
using Backend.Application.Features.UserManagement.Commands.DeactivateUser;
using Backend.Application.Features.UserManagement.Commands.ChangePassword;
using Backend.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers.V1.Users;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var query = new GetUserByIdQuery { UserId = id };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            return StatusCode(500, ApiResponse.Error("An error occurred while getting the user", 500, "InternalServerError"));
        }
    }

    /// <summary>
    /// Gets current user profile
    /// </summary>
    [HttpGet("profile")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetUserByIdQuery { UserId = userId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user profile");
            return StatusCode(500, ApiResponse.Error("An error occurred while getting the user profile", 500, "InternalServerError"));
        }
    }

    /// <summary>
    /// Gets paginated list of users
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<PaginationResponse<UserDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? emailConfirmed = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            var query = new GetUsersQuery
            {
                PageNumber = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Status = status,
                Role = role,
                EmailConfirmed = emailConfirmed,
                IsActive = isActive,
                SortBy = sortBy,
                SortDirection = sortDirection,
                IncludeDeleted = includeDeleted
            };

            var result = await _mediator.Send(query);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, ApiResponse.Error("An error occurred while getting users", 500, ex.GetType().Name));
        }
    }

    /// <summary>
    /// Updates a user
    /// </summary>
    [HttpPut("{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var command = new UpdateUserCommand
            {
                UserId = id,
                Email = updateUserDto.Email,
                UserName = updateUserDto.UserName,
                PhoneNumber = updateUserDto.PhoneNumber,
                EmailConfirmed = updateUserDto.EmailConfirmed,
                PhoneNumberConfirmed = updateUserDto.PhoneNumberConfirmed,
                IsActive = updateUserDto.IsActive,
                Roles = updateUserDto.Roles,
                CustomerId = updateUserDto.CustomerId,
                UpdatedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return StatusCode(500, ApiResponse.Error("An error occurred while updating the user", 500, ex.GetType().Name));
        }
    }

    /// <summary>
    /// Changes user password
    /// </summary>
    [HttpPost("change-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var command = new ChangePasswordCommand
            {
                UserId = changePasswordDto.UserId ?? currentUserId,
                CurrentPassword = changePasswordDto.CurrentPassword,
                NewPassword = changePasswordDto.NewPassword,
                ConfirmNewPassword = changePasswordDto.ConfirmNewPassword,
                ChangedBy = currentUserId,
                RequirePasswordChangeOnNextLogin = changePasswordDto.RequirePasswordChangeOnNextLogin
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, ApiResponse.Error("An error occurred while changing password", 500, ex.GetType().Name));
        }
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    [HttpDelete("{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var command = new DeleteUserCommand
            {
                UserId = id,
                DeletedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return StatusCode(500, ApiResponse.Error("An error occurred while deleting the user", 500, ex.GetType().Name));
        }
    }

    /// <summary>
    /// Activates a user
    /// </summary>
    [HttpPost("{id}/activate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> ActivateUser(string id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var command = new ActivateUserCommand
            {
                UserId = id,
                ActivatedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user: {UserId}", id);
            return StatusCode(500, ApiResponse.Error("An error occurred while activating the user", 500, ex.GetType().Name));
        }
    }

    /// <summary>
    /// Deactivates a user
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var command = new DeactivateUserCommand
            {
                UserId = id,
                DeactivatedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse.FromResult(result, result.IsSuccess ? 200 : 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", id);
            return StatusCode(500, ApiResponse.Error("An error occurred while deactivating the user", 500, ex.GetType().Name));
        }
    }
}
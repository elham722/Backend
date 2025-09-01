using Backend.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger Logger;

    protected BaseApiController(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle Result<T> from Application Layer and return ApiResponse<T>
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode = 200, int errorStatusCode = 400)
    {
        var response = ApiResponse<T>.FromResult(result, result.IsSuccess ? successStatusCode : errorStatusCode);

        if (response.IsSuccess)
            return StatusCode(successStatusCode, response);

        return StatusCode(errorStatusCode, response);
    }

    /// <summary>
    /// Handle Result (no data) from Application Layer and return ApiResponse
    /// </summary>
    protected IActionResult HandleResult(Result result, int successStatusCode = 200, int errorStatusCode = 400)
    {
        var response = ApiResponse.FromResult(result, result.IsSuccess ? successStatusCode : errorStatusCode);

        if (response.IsSuccess)
            return StatusCode(successStatusCode, response);

        return StatusCode(errorStatusCode, response);
    }

    /// <summary>
    /// Success response with data
    /// </summary>
    protected IActionResult Success<T>(T data, int statusCode = 200)
        => StatusCode(statusCode, ApiResponse<T>.Success(data, statusCode));

    /// <summary>
    /// Success response without data
    /// </summary>
    protected IActionResult Success(int statusCode = 200)
        => StatusCode(statusCode, ApiResponse.Success(statusCode));

    /// <summary>
    /// Error response
    /// </summary>
    protected IActionResult Error(string errorMessage, int statusCode = 400, string? errorCode = null)
        => StatusCode(statusCode, ApiResponse.Error(errorMessage, statusCode, errorCode));

    /// <summary>
    /// Internal server error response
    /// </summary>
    protected IActionResult InternalServerError(Exception ex, string? userMessage = null)
    {
        Logger.LogError(ex, "Unhandled Exception");
        return StatusCode(500, ApiResponse.Error(userMessage ?? ex.Message, 500, ex.GetType().Name));
    }

    /// <summary>
    /// Not found response
    /// </summary>
    protected IActionResult NotFound(string? message = null)
        => StatusCode(404, ApiResponse.Error(message ?? "Resource not found", 404, "NOT_FOUND"));

    /// <summary>
    /// Unauthorized response
    /// </summary>
    protected IActionResult Unauthorized(string? message = null)
        => StatusCode(401, ApiResponse.Error(message ?? "Authentication required", 401, "UNAUTHORIZED"));

    /// <summary>
    /// Forbidden response
    /// </summary>
    protected IActionResult Forbidden(string? message = null)
        => StatusCode(403, ApiResponse.Error(message ?? "Access forbidden", 403, "FORBIDDEN"));

    /// <summary>
    /// Validation errors
    /// </summary>
    protected IActionResult ValidationError(Dictionary<string, string[]> errors)
    {
        var flatMessage = string.Join("; ", errors.SelectMany(x => x.Value));
        return BadRequest(ApiResponse.Error(flatMessage, 400, "VALIDATION_ERROR"));
    }
}

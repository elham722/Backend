using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Backend.Application.Common.Security;
using Backend.Application.Common.Results;

namespace Backend.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for CAPTCHA verification
/// </summary>
public class CaptchaBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IHumanVerificationService _captcha;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<CaptchaBehavior<TRequest, TResponse>> _logger;

    public CaptchaBehavior(
        IHumanVerificationService captcha, 
        IHttpContextAccessor http,
        ILogger<CaptchaBehavior<TRequest, TResponse>> logger)
    {
        _captcha = captcha;
        _http = http;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Check if request requires CAPTCHA
        if (request is not IRequireCaptcha rc) 
        {
            _logger.LogDebug("Request {RequestType} does not require CAPTCHA", typeof(TRequest).Name);
            return await next();
        }

        _logger.LogInformation("Verifying CAPTCHA for request {RequestType}", typeof(TRequest).Name);

        try
        {
            // Get user IP address
            var ip = _http.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            
            // Verify CAPTCHA
            var result = await _captcha.VerifyAsync(
                rc.CaptchaToken, 
                rc.CaptchaAction, 
                ip, 
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("CAPTCHA verification failed for request {RequestType}. Errors: {Errors}", 
                    typeof(TRequest).Name, result.ErrorCodes);
                
                var message = "احراز هویت انسانی (reCAPTCHA) ناموفق بود.";
                
                // Create appropriate failure result
                if (typeof(TResponse) == typeof(Result))
                    return (TResponse)(object)Result.Failure(message, "CAPTCHA_FAILED");

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var dataType = typeof(TResponse).GetGenericArguments()[0];
                    var failure = typeof(Result<>).MakeGenericType(dataType)
                        .GetMethod("Failure", new[] { typeof(string), typeof(string) })!;
                    return (TResponse)failure.Invoke(null, new object[] { message, "CAPTCHA_FAILED" })!;
                }
            }

            _logger.LogInformation("CAPTCHA verification successful for request {RequestType}", typeof(TRequest).Name);
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CAPTCHA verification for request {RequestType}", typeof(TRequest).Name);
            var message = "خطا در تأیید امنیتی. لطفاً دوباره تلاش کنید.";
            
            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(message, "CAPTCHA_ERROR");

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = typeof(TResponse).GetGenericArguments()[0];
                var failure = typeof(Result<>).MakeGenericType(dataType)
                    .GetMethod("Failure", new[] { typeof(string), typeof(string) })!;
                return (TResponse)failure.Invoke(null, new object[] { message, "CAPTCHA_ERROR" })!;
            }

            throw;
        }
    }
} 
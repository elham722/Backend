using Client.MVC.Services;
using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;

namespace Client.MVC.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IErrorHandlingService _errorHandlingService;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IErrorHandlingService errorHandlingService)
        {
            _next = next;
            _logger = logger;
            _errorHandlingService = errorHandlingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var requestInfo = new
            {
                Url = context.Request.GetDisplayUrl(),
                Method = context.Request.Method,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserId = context.User?.Identity?.Name,
                Timestamp = DateTime.UtcNow
            };

            await _errorHandlingService.LogErrorAsync(exception, "GlobalExceptionMiddleware", requestInfo);

            context.Response.StatusCode = GetStatusCode(exception);
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = new
                {
                    message = GetErrorMessage(exception),
                    type = exception.GetType().Name,
                    requestId = context.TraceIdentifier
                }
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                HttpRequestException => StatusCodes.Status502BadGateway,
                TaskCanceledException => StatusCodes.Status408RequestTimeout,
                OperationCanceledException => StatusCodes.Status408RequestTimeout,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => "دسترسی غیرمجاز",
                ArgumentException => "پارامترهای ورودی نامعتبر",
                InvalidOperationException => "عملیات نامعتبر",
                HttpRequestException => "خطا در ارتباط با سرور",
                TaskCanceledException => "درخواست لغو شد",
                OperationCanceledException => "عملیات لغو شد",
                _ => "خطای داخلی سرور"
            };
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
} 
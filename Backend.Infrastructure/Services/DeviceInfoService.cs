using Backend.Application.Common.Helpers;
using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Device info service that uses HttpContext to extract request metadata.
/// </summary>
public class DeviceInfoService : IDeviceInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeviceInfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return null;
        }

        return context.Request?.Headers["User-Agent"].ToString();
    }

    public string? GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return null;
        }

        return context.Connection?.RemoteIpAddress?.ToString();
    }

    public string GetDeviceInfo(string? userAgent = null)
    {
        var ua = userAgent ?? GetUserAgent();
        return DeviceInfoParser.ParseToLabel(ua);
    }
}


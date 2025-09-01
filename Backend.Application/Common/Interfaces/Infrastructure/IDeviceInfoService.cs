using System;

namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Provides information about the current device/request context.
/// Intended to abstract away HTTP-specific details from upper layers.
/// </summary>
public interface IDeviceInfoService
{
    /// <summary>
    /// Gets the raw User-Agent string for the current request if available.
    /// </summary>
    string? GetUserAgent();

    /// <summary>
    /// Gets the remote IP address for the current request if available.
    /// </summary>
    string? GetIpAddress();

    /// <summary>
    /// Parses the User-Agent (and other hints) to a concise device info label.
    /// Example: "Desktop | Windows | Chrome".
    /// </summary>
    /// <param name="userAgent">Optional explicit user agent; if null, uses current request.</param>
    string GetDeviceInfo(string? userAgent = null);
}


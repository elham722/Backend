using System;
using System.Collections.Generic;

namespace Backend.Application.Common.Helpers;

/// <summary>
/// Pure helper for parsing User-Agent strings to a concise device info label.
/// Keep this dependency-free for easy testing.
/// </summary>
public static class DeviceInfoParser
{
    public static string ParseToLabel(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "Unknown Device";
        }

        try
        {
            var deviceInfo = new List<string>();

            if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
            {
                deviceInfo.Add("Mobile");
            }
            else if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase) ||
                     userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            {
                deviceInfo.Add("Tablet");
            }
            else
            {
                deviceInfo.Add("Desktop");
            }

            if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Windows");
            else if (userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("macOS");
            else if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Linux");
            else if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Android");
            else if (userAgent.Contains("iOS", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("iOS");

            if (userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("Edge", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Edge");
            else if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Chrome");
            else if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Firefox");
            else if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase))
                deviceInfo.Add("Safari");

            return string.Join(" | ", deviceInfo);
        }
        catch
        {
            return "Unknown Device";
        }
    }
}


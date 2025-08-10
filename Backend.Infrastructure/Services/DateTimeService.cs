using Backend.Application.Common.Interfaces;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Service for getting current date and time
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.Now;

    public DateTime UtcToday => DateTime.UtcNow.Date;

    public DateTime LocalToday => DateTime.Today;

    public DateTime ToLocalTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
    }

    public DateTime ToUtcTime(DateTime localDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, TimeZoneInfo.Local);
    }
} 
namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service for getting current date and time
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }
    
    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime LocalNow { get; }
    
    /// <summary>
    /// Gets the current date (UTC)
    /// </summary>
    DateTime UtcToday { get; }
    
    /// <summary>
    /// Gets the current date (local)
    /// </summary>
    DateTime LocalToday { get; }
    
    /// <summary>
    /// Converts UTC time to local time
    /// </summary>
    /// <param name="utcDateTime">UTC date time</param>
    /// <returns>Local date time</returns>
    DateTime ToLocalTime(DateTime utcDateTime);
    
    /// <summary>
    /// Converts local time to UTC time
    /// </summary>
    /// <param name="localDateTime">Local date time</param>
    /// <returns>UTC date time</returns>
    DateTime ToUtcTime(DateTime localDateTime);
} 
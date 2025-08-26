
using Backend.Application.Common.Interfaces;

namespace Backend.Identity.Services
{
    public class DefaultDateTimeService : IDateTimeService
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
} 
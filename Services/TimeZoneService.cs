using System.Security.Claims;

namespace WagerWatch.Services
{
    public interface ITimeZoneService
    {
        DateTime ConvertToUserTimeZone(DateTime utcDateTime, string userTimeZone);
        DateTime ConvertFromUserTimeZone(DateTime userDateTime, string userTimeZone);
        string GetUserTimeZone(ClaimsPrincipal user);
        List<TimeZoneOption> GetAvailableTimeZones();
    }

    public class TimeZoneService : ITimeZoneService
    {
        private readonly ILogger<TimeZoneService> _logger;

        public TimeZoneService(ILogger<TimeZoneService> logger)
        {
            _logger = logger;
        }

        public DateTime ConvertToUserTimeZone(DateTime utcDateTime, string userTimeZone)
        {
            try
            {
                if (string.IsNullOrEmpty(userTimeZone))
                {
                    userTimeZone = "America/New_York"; // Default to Eastern
                }

                var timeZoneInfo = GetTimeZoneInfo(userTimeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error converting time to user timezone: {userTimeZone}");
                // Fallback to Eastern Time
                var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, easternZone);
            }
        }

        public DateTime ConvertFromUserTimeZone(DateTime userDateTime, string userTimeZone)
        {
            try
            {
                if (string.IsNullOrEmpty(userTimeZone))
                {
                    userTimeZone = "America/New_York";
                }

                var timeZoneInfo = GetTimeZoneInfo(userTimeZone);
                return TimeZoneInfo.ConvertTimeToUtc(userDateTime, timeZoneInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error converting time from user timezone: {userTimeZone}");
                // Fallback to Eastern Time
                var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return TimeZoneInfo.ConvertTimeToUtc(userDateTime, easternZone);
            }
        }

        public string GetUserTimeZone(ClaimsPrincipal user)
        {
            // Try to get timezone from user claims (added during login)
            var timeZoneClaim = user.FindFirst("TimeZone")?.Value;
            
            if (!string.IsNullOrEmpty(timeZoneClaim))
            {
                return timeZoneClaim;
            }

            // Default to Eastern Time
            return "America/New_York";
        }

        public List<TimeZoneOption> GetAvailableTimeZones()
        {
            return new List<TimeZoneOption>
            {
                new TimeZoneOption { Id = "America/New_York", Name = "Eastern Time (ET)", Offset = "UTC-5/-4" },
                new TimeZoneOption { Id = "America/Chicago", Name = "Central Time (CT)", Offset = "UTC-6/-5" },
                new TimeZoneOption { Id = "America/Denver", Name = "Mountain Time (MT)", Offset = "UTC-7/-6" },
                new TimeZoneOption { Id = "America/Los_Angeles", Name = "Pacific Time (PT)", Offset = "UTC-8/-7" },
                new TimeZoneOption { Id = "America/Phoenix", Name = "Arizona Time (MST)", Offset = "UTC-7" },
                new TimeZoneOption { Id = "America/Anchorage", Name = "Alaska Time (AKT)", Offset = "UTC-9/-8" },
                new TimeZoneOption { Id = "Pacific/Honolulu", Name = "Hawaii Time (HST)", Offset = "UTC-10" },
                new TimeZoneOption { Id = "UTC", Name = "UTC", Offset = "UTC+0" }
            };
        }

        private TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
        {
            // Map IANA timezone IDs to Windows timezone IDs
            return timeZoneId switch
            {
                "America/New_York" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
                "America/Chicago" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
                "America/Denver" => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
                "America/Los_Angeles" => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
                "America/Phoenix" => TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time"),
                "America/Anchorage" => TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"),
                "Pacific/Honolulu" => TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"),
                "UTC" => TimeZoneInfo.Utc,
                _ => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") // Default fallback
            };
        }
    }

    public class TimeZoneOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Offset { get; set; } = string.Empty;
    }
}
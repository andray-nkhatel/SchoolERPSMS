using System;
using System.Globalization;

namespace SchoolErpSMS.Utilities
{
    /// <summary>
    /// Utility class for handling DateTime operations with Zambia timezone (Africa/Lusaka, UTC+2)
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Zambia timezone identifier (Africa/Lusaka)
        /// </summary>
        public const string ZambiaTimeZoneId = "Africa/Lusaka";

        /// <summary>
        /// Gets the current time in Zambia timezone
        /// </summary>
        /// <returns>Current DateTime in Zambia timezone</returns>
        public static DateTime GetZambiaTime()
        {
            try
            {
                var zambiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ZambiaTimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zambiaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: Manual offset (UTC+2) - Zambia is always UTC+2 (no daylight saving)
                return DateTime.UtcNow.AddHours(2);
            }
        }

        /// <summary>
        /// Converts UTC DateTime to Zambia local time
        /// </summary>
        /// <param name="utcDateTime">UTC DateTime to convert</param>
        /// <returns>DateTime in Zambia timezone</returns>
        public static DateTime ToZambiaTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                // If not UTC, assume it's already UTC but not marked as such
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            try
            {
                var zambiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ZambiaTimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, zambiaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: Manual offset (UTC+2) - Zambia is always UTC+2 (no daylight saving)
                return utcDateTime.AddHours(2);
            }
        }

        /// <summary>
        /// Converts Zambia local time to UTC
        /// </summary>
        /// <param name="zambiaDateTime">DateTime in Zambia timezone</param>
        /// <returns>UTC DateTime</returns>
        public static DateTime ToUtc(DateTime zambiaDateTime)
        {
            try
            {
                var zambiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ZambiaTimeZoneId);
                return TimeZoneInfo.ConvertTimeToUtc(zambiaDateTime, zambiaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: Manual offset (UTC+2) - Zambia is always UTC+2 (no daylight saving)
                return zambiaDateTime.AddHours(-2);
            }
        }

        /// <summary>
        /// Formats a DateTime to a string in Zambia timezone
        /// </summary>
        /// <param name="dateTime">DateTime to format (assumed to be UTC)</param>
        /// <param name="format">Format string (default: "yyyy-MM-dd HH:mm:ss")</param>
        /// <returns>Formatted string in Zambia timezone</returns>
        public static string FormatZambiaTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
        {
            var zambiaTime = ToZambiaTime(dateTime);
            return zambiaTime.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets current UTC time (for database storage)
        /// This is the recommended method for storing timestamps in the database
        /// </summary>
        /// <returns>Current UTC DateTime</returns>
        public static DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Formats a DateTime for display in Zambia timezone with a standard format
        /// </summary>
        /// <param name="dateTime">DateTime to format (assumed to be UTC)</param>
        /// <returns>Formatted string like "Jan 15, 2025 14:30"</returns>
        public static string FormatForDisplay(DateTime dateTime)
        {
            var zambiaTime = ToZambiaTime(dateTime);
            return zambiaTime.ToString("MMM dd, yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime for display with date only
        /// </summary>
        /// <param name="dateTime">DateTime to format (assumed to be UTC)</param>
        /// <returns>Formatted string like "Jan 15, 2025"</returns>
        public static string FormatDateOnly(DateTime dateTime)
        {
            var zambiaTime = ToZambiaTime(dateTime);
            return zambiaTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime for display with time only
        /// </summary>
        /// <param name="dateTime">DateTime to format (assumed to be UTC)</param>
        /// <returns>Formatted string like "14:30"</returns>
        public static string FormatTimeOnly(DateTime dateTime)
        {
            var zambiaTime = ToZambiaTime(dateTime);
            return zambiaTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
    }
}


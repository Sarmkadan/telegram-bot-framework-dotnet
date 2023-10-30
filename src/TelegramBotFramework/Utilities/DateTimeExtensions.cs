// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Extension methods for DateTime operations.
/// Provides conversions, formatting, and time calculations.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Determines if a DateTime is in the past relative to now.
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Determines if a DateTime is in the future relative to now.
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the start of the day (00:00:00).
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday).
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dateTime.DayOfWeek - startDayOfWeek)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Gets the end of the week (Sunday).
    /// </summary>
    public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek endDayOfWeek = DayOfWeek.Sunday)
    {
        return dateTime.StartOfWeek().AddDays(7).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the month (1st day at 00:00:00).
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month (last day at 23:59:59).
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Returns a human-readable relative time string (e.g., "2 hours ago").
    /// </summary>
    public static string ToRelativeTimeString(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan.TotalSeconds < 60 ? "just now" :
               timeSpan.TotalMinutes < 60 ? $"{(int)timeSpan.TotalMinutes}m ago" :
               timeSpan.TotalHours < 24 ? $"{(int)timeSpan.TotalHours}h ago" :
               timeSpan.TotalDays < 30 ? $"{(int)timeSpan.TotalDays}d ago" :
               timeSpan.TotalDays < 365 ? $"{(int)(timeSpan.TotalDays / 30)}mo ago" :
               $"{(int)(timeSpan.TotalDays / 365)}y ago";
    }

    /// <summary>
    /// Determines if a DateTime is between two dates (inclusive).
    /// </summary>
    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }

    /// <summary>
    /// Adds business days (excluding weekends) to a DateTime.
    /// </summary>
    public static DateTime AddBusinessDays(this DateTime dateTime, int days)
    {
        int direction = days < 0 ? -1 : 1;
        int daysRemaining = Math.Abs(days);

        while (daysRemaining > 0)
        {
            dateTime = dateTime.AddDays(direction);
            if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
                daysRemaining--;
        }

        return dateTime;
    }

    /// <summary>
    /// Gets the age in years from a DateTime to now.
    /// </summary>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age;
    }
}

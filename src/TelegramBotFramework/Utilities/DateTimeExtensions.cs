#nullable enable

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
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>The Unix timestamp in seconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the resulting timestamp would overflow long.</exception>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        var epoch = DateTimeExtensionsConstants.UnixEpoch;
        var result = dateTime - epoch;

        if (result.TotalSeconds > long.MaxValue || result.TotalSeconds < long.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), DateTimeExtensionsConstants.UnixTimestampOutOfRangeErrorMessage);
        }

        return (long)result.TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    /// <param name="timestamp">The Unix timestamp in seconds.</param>
    /// <returns>The corresponding DateTime.</returns>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTimeExtensionsConstants.UnixEpoch.AddSeconds(timestamp);
    }

    /// <summary>
    /// Determines if a DateTime is in the past relative to now.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the DateTime is in the past; otherwise, false.</returns>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Determines if a DateTime is in the future relative to now.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the DateTime is in the future; otherwise, false.</returns>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the start of the day (00:00:00).
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize.</param>
    /// <returns>The start of the day.</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize.</param>
    /// <returns>The end of the day.</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday).
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize.</param>
    /// <param name="startDayOfWeek">The day of week to consider as start of week. Defaults to Monday.</param>
    /// <returns>The start of the week.</returns>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dateTime.DayOfWeek - startDayOfWeek)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Gets the end of the week (Sunday).
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize.</param>
    /// <param name="endDayOfWeek">The day of week to consider as end of week. Defaults to Sunday.</param>
    /// <returns>The end of the week.</returns>
    public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek endDayOfWeek = DayOfWeek.Sunday)
    {
        return dateTime.StartOfWeek().AddDays(7).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the month (1st day at 00:00:00).
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize.</param>
    /// <returns>The start of the month.</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month (last day at 23:59:59).
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize.</param>
    /// <returns>The end of the month.</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Returns a human-readable relative time string (e.g., "2 hours ago").
    /// </summary>
    /// <param name="dateTime">The DateTime to convert to relative string.</param>
    /// <returns>A human-readable relative time string.</returns>
    public static string ToRelativeTimeString(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan.TotalSeconds < DateTimeExtensionsConstants.SecondsInMinute ? DateTimeExtensionsConstants.JustNow :
               timeSpan.TotalMinutes < DateTimeExtensionsConstants.MinutesInHour ? $"{(int)timeSpan.TotalMinutes}{DateTimeExtensionsConstants.MinutesAgoFormat}" :
               timeSpan.TotalHours < DateTimeExtensionsConstants.HoursInDay ? $"{(int)timeSpan.TotalHours}{DateTimeExtensionsConstants.HoursAgoFormat}" :
               timeSpan.TotalDays < DateTimeExtensionsConstants.DaysInMonthApprox ? $"{(int)timeSpan.TotalDays}{DateTimeExtensionsConstants.DaysAgoFormat}" :
               timeSpan.TotalDays < DateTimeExtensionsConstants.DaysInYear ? $"{(int)(timeSpan.TotalDays / DateTimeExtensionsConstants.DaysInMonthApprox)}{DateTimeExtensionsConstants.MonthsAgoFormat}" :
               $"{(int)(timeSpan.TotalDays / DateTimeExtensionsConstants.DaysInYear)}{DateTimeExtensionsConstants.YearsAgoFormat}";
    }

    /// <summary>
    /// Determines if a DateTime is between two dates (inclusive).
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <param name="start">The start date.</param>
    /// <param name="end">The end date.</param>
    /// <returns>True if the DateTime is between start and end (inclusive); otherwise, false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when start is after end.</exception>
    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "Start date cannot be after end date.");
        }

        return dateTime >= start && dateTime <= end;
    }

    /// <summary>
    /// Adds business days (excluding weekends) to a DateTime.
    /// </summary>
    /// <param name="dateTime">The DateTime to add business days to.</param>
    /// <param name="days">The number of business days to add. Can be negative.</param>
    /// <returns>The resulting DateTime after adding business days.</returns>
    public static DateTime AddBusinessDays(this DateTime dateTime, int days)
    {
        int direction = days < 0 ? -1 : 1;
        int daysRemaining = Math.Abs(days);

        while (daysRemaining > 0)
        {
            dateTime = dateTime.AddDays(direction);
            if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
            {
                daysRemaining--;
            }
        }

        return dateTime;
    }

    /// <summary>
    /// Gets the age in years from a DateTime to now.
    /// </summary>
    /// <param name="birthDate">The birth date.</param>
    /// <returns>The age in years.</returns>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
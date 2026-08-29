#nullable enable

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Constants for DateTimeExtensions.
/// </summary>
internal static class DateTimeExtensionsConstants
{
    /// <summary>
    /// Unix epoch (January 1, 1970, 00:00:00 UTC).
    /// </summary>
    public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Error message for DateTime values outside Unix timestamp range.
    /// </summary>
    public const string UnixTimestampOutOfRangeErrorMessage = "The DateTime value is outside the valid range for Unix timestamp conversion.";

    /// <summary>
    /// Number of seconds in a minute.
    /// </summary>
    public const int SecondsInMinute = 60;

    /// <summary>
    /// Number of minutes in an hour.
    /// </summary>
    public const int MinutesInHour = 60;

    /// <summary>
    /// Number of hours in a day.
    /// </summary>
    public const int HoursInDay = 24;

    /// <summary>
    /// Approximate number of days in a month.
    /// </summary>
    public const int DaysInMonthApprox = 30;

    /// <summary>
    /// Number of days in a year.
    /// </summary>
    public const int DaysInYear = 365;

    /// <summary>
    /// String for "just now" in relative time.
    /// </summary>
    public const string JustNow = "just now";

    /// <summary>
    /// Format string for minutes ago in relative time.
    /// </summary>
    public const string MinutesAgoFormat = "{0}m ago";

    /// <summary>
    /// Format string for hours ago in relative time.
    /// </summary>
    public const string HoursAgoFormat = "{0}h ago";

    /// <summary>
    /// Format string for days ago in relative time.
    /// </summary>
    public const string DaysAgoFormat = "{0}d ago";

    /// <summary>
    /// Format string for months ago in relative time.
    /// </summary>
    public const string MonthsAgoFormat = "{0}mo ago";

    /// <summary>
    /// Format string for years ago in relative time.
    /// </summary>
    public const string YearsAgoFormat = "{0}y ago";
}
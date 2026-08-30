#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Caching;

/// <summary>
/// Constants for <see cref="DistributedCacheProviderValidation"/>.
/// </summary>
internal static class DistributedCacheProviderValidationConstants
{
    /// <summary>
    /// Prefix for CacheStatistics property names in validation error messages.
    /// </summary>
    public const string CacheStatisticsPrefix = "CacheStatistics.";

    /// <summary>
    /// Suffix for non-negative validation error messages.
    /// </summary>
    public const string NonNegativeErrorMessageSuffix = " must be non-negative, but was ";

    /// <summary>
    /// Suffix for hit rate validation error messages (between 0 and 100).
    /// </summary>
    public const string BetweenZeroAndOneHundredErrorMessageSuffix = " must be between 0 and 100, but was ";

    /// <summary>
    /// Zero value used for validation comparisons.
    /// </summary>
    public const int Zero = 0;

    /// <summary>
    /// One hundred value used for validation comparisons.
    /// </summary>
    public const int OneHundred = 100;

    /// <summary>
    /// New line character used for formatting validation error messages.
    /// </summary>
    public const string NewLine = "\n";
}
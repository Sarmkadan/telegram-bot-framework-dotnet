using System;
using System.Collections.Generic;
using System.Globalization;

namespace TelegramBotFramework.Services
{
    /// <summary>
    /// Extension methods for <see cref="RateLimitStats"/>.
    /// </summary>
    public static class RateLimitStatsExtensions
    {
        /// <summary>
        /// Calculates the success rate of sent messages.
        /// </summary>
        /// <param name="stats">The rate‑limit statistics instance.</param>
        /// <returns>
        /// The ratio of successfully sent messages to the total number of attempted messages,
        /// expressed as a value between 0.0 and 1.0. Returns 0.0 when no messages have been attempted.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="stats"/> is <c>null</c>.</exception>
        public static double SuccessRate(this RateLimitStats stats)
        {
            ArgumentNullException.ThrowIfNull(stats);
            var total = stats.TotalMessagesSent + stats.TotalMessagesFailed;
            return total == 0 ? 0.0 : (double)stats.TotalMessagesSent / total;
        }

        /// <summary>
        /// Determines whether the current concurrency exceeds the configured maximum.
        /// </summary>
        /// <param name="stats">The rate‑limit statistics instance.</param>
        /// <returns><c>true</c> if <see cref="RateLimitStats.CurrentConcurrency"/> is greater than <see cref="RateLimitStats.MaxConcurrency"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stats"/> is <c>null</c>.</exception>
        public static bool IsOverConcurrencyLimit(this RateLimitStats stats)
        {
            ArgumentNullException.ThrowIfNull(stats);
            return stats.CurrentConcurrency > stats.MaxConcurrency;
        }

        /// <summary>
        /// Returns a culture‑invariant, human‑readable representation of the statistics.
        /// </summary>
        /// <param name="stats">The rate‑limit statistics instance.</param>
        /// <returns>A string containing all public members formatted with <see cref="CultureInfo.InvariantCulture"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stats"/> is <c>null</c>.</exception>
        public static string ToInvariantString(this RateLimitStats stats)
        {
            ArgumentNullException.ThrowIfNull(stats);
            return string.Format(
                CultureInfo.InvariantCulture,
                "Timestamp={0:O}, MessagesPerSecond={1}, MaxConcurrency={2}, TotalMessagesSent={3}, TotalMessagesFailed={4}, AverageMessagesPerSecond={5:F2}, CurrentConcurrency={6}",
                stats.Timestamp,
                stats.MessagesPerSecond,
                stats.MaxConcurrency,
                stats.TotalMessagesSent,
                stats.TotalMessagesFailed,
                stats.AverageMessagesPerSecond,
                stats.CurrentConcurrency);
        }

        /// <summary>
        /// Returns a read‑only dictionary that maps metric names to their values.
        /// </summary>
        /// <param name="stats">The rate‑limit statistics instance.</param>
        /// <returns>An <see cref="IReadOnlyDictionary{String,Object}"/> containing the metrics.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stats"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, object> ToDictionary(this RateLimitStats stats)
        {
            ArgumentNullException.ThrowIfNull(stats);
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Timestamp"] = stats.Timestamp,
                ["MessagesPerSecond"] = stats.MessagesPerSecond,
                ["MaxConcurrency"] = stats.MaxConcurrency,
                ["TotalMessagesSent"] = stats.TotalMessagesSent,
                ["TotalMessagesFailed"] = stats.TotalMessagesFailed,
                ["AverageMessagesPerSecond"] = stats.AverageMessagesPerSecond,
                ["CurrentConcurrency"] = stats.CurrentConcurrency
            };
            return dict;
        }
    }
}

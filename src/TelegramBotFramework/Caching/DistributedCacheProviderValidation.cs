#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Caching;

/// <summary>
/// Provides validation helpers for <see cref="DistributedCacheProvider"/> instances.
/// </summary>
public static class DistributedCacheProviderValidation
{
    /// <summary>
    /// Validates the specified <see cref="DistributedCacheProvider"/> instance.
    /// </summary>
    /// <param name="value">The cache provider to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="DistributedCacheProvider.GetStatisticsAsync"/> fails.</exception>
    public static IReadOnlyList<string> Validate(this DistributedCacheProvider? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate GetStatisticsAsync result
        CacheStatistics stats = value.GetStatisticsAsync().GetAwaiter().GetResult();
        ValidateCacheStatistics(stats, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DistributedCacheProvider"/> is valid.
    /// </summary>
    /// <param name="value">The cache provider to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DistributedCacheProvider? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DistributedCacheProvider"/> is valid.
    /// </summary>
    /// <param name="value">The cache provider to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="DistributedCacheProvider.GetStatisticsAsync"/> fails.</exception>
    public static void EnsureValid(this DistributedCacheProvider? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DistributedCacheProvider is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }

    private static void ValidateCacheStatistics(CacheStatistics stats, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(stats);

        if (stats.HitCount is < 0)
        {
            problems.Add($"CacheStatistics.HitCount must be non-negative, but was {stats.HitCount}.");
        }

        if (stats.MissCount is < 0)
        {
            problems.Add($"CacheStatistics.MissCount must be non-negative, but was {stats.MissCount}.");
        }

        if (stats.SetCount is < 0)
        {
            problems.Add($"CacheStatistics.SetCount must be non-negative, but was {stats.SetCount}.");
        }

        if (stats.RemoveCount is < 0)
        {
            problems.Add($"CacheStatistics.RemoveCount must be non-negative, but was {stats.RemoveCount}.");
        }

        if (stats.ItemCount is < 0)
        {
            problems.Add($"CacheStatistics.ItemCount must be non-negative, but was {stats.ItemCount}.");
        }

        if (stats.MemoryBytes is < 0)
        {
            problems.Add($"CacheStatistics.MemoryBytes must be non-negative, but was {stats.MemoryBytes}.");
        }

        if (stats.HitRate is < 0 or > 100)
        {
            problems.Add($"CacheStatistics.HitRate must be between 0 and 100, but was {stats.HitRate:F2}.");
        }
    }
}

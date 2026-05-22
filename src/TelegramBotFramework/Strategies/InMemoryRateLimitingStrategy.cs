#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.Strategies;

/// <summary>
/// Defines the interface for a rate limiting strategy.
/// </summary>
public interface IRateLimitingStrategy
{
    /// <summary>
    /// Checks if an action is allowed for a given key within a specified limit and interval.
    /// </summary>
    /// <param name="key">The key to identify the action (e.g., user ID, IP address).</param>
    /// <param name="limit">The maximum number of actions allowed within the interval.</param>
    /// <param name="interval">The time interval for the rate limit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the action is allowed, false otherwise.</returns>
    Task<bool> IsActionAllowedAsync(string key, int limit, TimeSpan interval, CancellationToken cancellationToken = default);
}

/// <summary>
/// An in-memory rate limiting strategy that uses a sliding window approach.
/// This implementation is suitable for single-instance applications or for testing purposes.
/// For distributed applications, a distributed cache (e.g., Redis) should be used.
/// </summary>
public sealed class InMemoryRateLimitingStrategy : IRateLimitingStrategy
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _requests = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Checks if an action is allowed for a given key within a specified limit and interval.
    /// </summary>
    /// <param name="key">The key to identify the action (e.g., user ID, IP address).</param>
    /// <param name="limit">The maximum number of actions allowed within the interval.</param>
    /// <param name="interval">The time interval for the rate limit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the action is allowed, false otherwise.</returns>
    public Task<bool> IsActionAllowedAsync(string key, int limit, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            // Get or create the queue for the current key
            var queue = _requests.GetOrAdd(key, _ => new ConcurrentQueue<DateTime>());

            // Remove requests older than the interval
            while (queue.TryPeek(out var oldestRequest) && oldestRequest < now - interval)
            {
                queue.TryDequeue(out _);
            }

            // Check if the current number of requests exceeds the limit
            if (queue.Count < limit)
            {
                queue.Enqueue(now);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
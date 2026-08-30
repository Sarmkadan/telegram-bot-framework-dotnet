#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.Strategies;

/// <summary>
/// An in-memory rate limiting strategy that uses a sliding window approach.
/// This implementation is suitable for single-instance applications or for testing purposes.
/// For distributed applications, a distributed cache (e.g., Redis) should be used.
/// </summary>
public sealed class InMemoryRateLimitingStrategy : IRateLimitingStrategy, IInMemoryRateLimitingStrategy
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _requests = new();
    private static readonly object _lock = new();

    /// <inheritdoc/>
    public bool IsRequestAllowed(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var queue = _requests.GetOrAdd(identifier, _ => new ConcurrentQueue<DateTime>());

            // Default window: 1 minute, default limit: 30
            while (queue.TryPeek(out var oldest) && oldest <= now - TimeSpan.FromMinutes(1))
                queue.TryDequeue(out _);

            if (queue.Count < 30)
            {
                queue.Enqueue(now);
                return true;
            }

            return false;
        }
    }

    /// <inheritdoc/>
    public int GetRemainingRequests(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            if (!_requests.TryGetValue(identifier, out var queue))
                return 30;

            while (queue.TryPeek(out var oldest) && oldest <= now - TimeSpan.FromMinutes(1))
                queue.TryDequeue(out _);

            return Math.Max(0, 30 - queue.Count);
        }
    }

    /// <summary>
    /// Checks if an action is allowed for a given key within a specified limit and interval.
    /// </summary>
    public Task<bool> IsActionAllowedAsync(string key, int limit, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var queue = _requests.GetOrAdd(key, _ => new ConcurrentQueue<DateTime>());

            while (queue.TryPeek(out var oldestRequest) && oldestRequest <= now - interval)
                queue.TryDequeue(out _);

            if (queue.Count < limit)
            {
                queue.Enqueue(now);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}

#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Strategies;

/// <summary>
/// Strategies for rate limiting implementation.
/// Provides different algorithms for controlling request rates.
/// </summary>
public interface IRateLimitingStrategy
{
    /// <summary>Checks if a request from an identifier is allowed.</summary>
    bool IsRequestAllowed(string identifier);

    /// <summary>Gets the remaining requests for an identifier.</summary>
    int GetRemainingRequests(string identifier);
}

/// <summary>
/// Token bucket algorithm for rate limiting.
/// Replenishes tokens at a fixed rate, allowing burst traffic.
/// </summary>
public sealed class TokenBucketStrategy : IRateLimitingStrategy
{
    private readonly int _bucketCapacity;
    private readonly int _tokensPerSecond;
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly object _lockObj = new();

    public TokenBucketStrategy(int bucketCapacity, int tokensPerSecond)
    {
        _bucketCapacity = bucketCapacity;
        _tokensPerSecond = tokensPerSecond;
    }

    public bool IsRequestAllowed(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lockObj)
        {
            if (!_buckets.TryGetValue(identifier, out var bucket))
            {
                bucket = new TokenBucket(_bucketCapacity);
                _buckets[identifier] = bucket;
            }

            bucket.Replenish(_tokensPerSecond);

            if (bucket.AvailableTokens >= 1)
            {
                bucket.AvailableTokens--;
                return true;
            }

            return false;
        }
    }

    public int GetRemainingRequests(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lockObj)
        {
            if (_buckets.TryGetValue(identifier, out var bucket))
            {
                bucket.Replenish(_tokensPerSecond);
                return Math.Max(0, (int)bucket.AvailableTokens);
            }

            return _bucketCapacity;
        }
    }

    private class TokenBucket
    {
        private readonly int _capacity;
        public double AvailableTokens { get; set; }
        private DateTime _lastRefillTime;

        public TokenBucket(int capacity)
        {
            _capacity = capacity;
            AvailableTokens = capacity;
            _lastRefillTime = DateTime.UtcNow;
        }

        public void Replenish(int tokensPerSecond)
        {
            var now = DateTime.UtcNow;
            var timePassed = (now - _lastRefillTime).TotalSeconds;
            var tokensToAdd = timePassed * tokensPerSecond;

            AvailableTokens = Math.Min(_capacity, AvailableTokens + tokensToAdd);
            _lastRefillTime = now;
        }
    }
}

/// <summary>
/// Sliding window rate limiting strategy.
/// Tracks requests within a rolling time window.
/// </summary>
public sealed class SlidingWindowStrategy : IRateLimitingStrategy
{
    private readonly int _requestsPerWindow;
    private readonly TimeSpan _windowDuration;
    private readonly Dictionary<string, Queue<DateTime>> _requestTimes = new();
    private readonly object _lockObj = new();

    public SlidingWindowStrategy(int requestsPerWindow, TimeSpan windowDuration)
    {
        _requestsPerWindow = requestsPerWindow;
        _windowDuration = windowDuration;
    }

    public bool IsRequestAllowed(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lockObj)
        {
            if (!_requestTimes.TryGetValue(identifier, out var times))
            {
                times = new Queue<DateTime>();
                _requestTimes[identifier] = times;
            }

            var cutoffTime = DateTime.UtcNow - _windowDuration;

            // Remove old requests outside the window
            while (times.Count > 0 && times.Peek() < cutoffTime)
            {
                times.Dequeue();
            }

            if (times.Count < _requestsPerWindow)
            {
                times.Enqueue(DateTime.UtcNow);
                return true;
            }

            return false;
        }
    }

    public int GetRemainingRequests(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lockObj)
        {
            if (!_requestTimes.TryGetValue(identifier, out var times))
            {
                return _requestsPerWindow;
            }

            var cutoffTime = DateTime.UtcNow - _windowDuration;

            // Count valid requests
            int validRequests = times.Count(t => t >= cutoffTime);
            return Math.Max(0, _requestsPerWindow - validRequests);
        }
    }
}

/// <summary>
/// Fixed window rate limiting strategy.
/// Simple approach that resets counter at fixed time intervals.
/// </summary>
public sealed class FixedWindowStrategy : IRateLimitingStrategy
{
    private readonly int _requestsPerWindow;
    private readonly TimeSpan _windowDuration;
    private readonly Dictionary<string, WindowData> _windows = new();
    private readonly object _lockObj = new();

    public FixedWindowStrategy(int requestsPerWindow, TimeSpan windowDuration)
    {
        _requestsPerWindow = requestsPerWindow;
        _windowDuration = windowDuration;
    }

    public bool IsRequestAllowed(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lockObj)
        {
            if (!_windows.TryGetValue(identifier, out var window))
            {
                window = new WindowData();
                _windows[identifier] = window;
            }

            // Check if window has expired
            if (DateTime.UtcNow >= window.WindowEndTime)
            {
                window.WindowStartTime = DateTime.UtcNow;
                window.WindowEndTime = DateTime.UtcNow.Add(_windowDuration);
                window.RequestCount = 0;
            }

            if (window.RequestCount < _requestsPerWindow)
            {
                window.RequestCount++;
                return true;
            }

            return false;
        }
    }

    public int GetRemainingRequests(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lockObj)
        {
            if (!_windows.TryGetValue(identifier, out var window))
            {
                return _requestsPerWindow;
            }

            return Math.Max(0, _requestsPerWindow - window.RequestCount);
        }
    }

    private class WindowData
    {
        public DateTime WindowStartTime { get; set; } = DateTime.UtcNow;
        public DateTime WindowEndTime { get; set; } = DateTime.UtcNow.AddMinutes(1);
        public int RequestCount { get; set; }
    }
}
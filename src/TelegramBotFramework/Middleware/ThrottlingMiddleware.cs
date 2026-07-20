#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Middleware for throttling user messages based on a sliding window algorithm.
/// Limits the number of messages a user can send within a time window.
/// </summary>
public sealed class ThrottlingMiddleware : IBotMiddleware
{
    private readonly ILogger<ThrottlingMiddleware> _logger;
    private readonly int _maxMessages;
    private readonly TimeSpan _windowSize;
    private readonly ConcurrentDictionary<long, List<DateTime>> _messageTimestamps;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingMiddleware"/> class.
    /// </summary>
    /// <param name="maxMessages">Maximum number of messages allowed in the time window (default: 20)</param>
    /// <param name="windowSizeSeconds">Time window size in seconds (default: 60)</param>
    public ThrottlingMiddleware(ILogger<ThrottlingMiddleware> logger, int maxMessages = 20, int windowSizeSeconds = 60)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxMessages = maxMessages > 0 ? maxMessages : 20;
        _windowSize = TimeSpan.FromSeconds(windowSizeSeconds > 0 ? windowSizeSeconds : 60);
        _messageTimestamps = new ConcurrentDictionary<long, List<DateTime>>();
    }

    public int Priority => 30; // Throttling comes after rate limiting

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        if (!context.IsValid || context.IsStopped)
        {
            return context;
        }

        if (context.UserId <= 0)
        {
            context.AddError("UserId must be positive for throttling.");
            _logger.LogWarning("ThrottlingMiddleware: UserId is not valid for throttling check.");
            return await next(context).ConfigureAwait(false);
        }

        // Check throttling limit
        var isAllowed = CheckThrottlingLimit(context.UserId);

        if (!isAllowed)
        {
            var errorMessage = $"Throttling limit exceeded for user {context.UserId}. Max {_maxMessages} messages per {_windowSize.TotalSeconds} seconds.";
            context.AddError(errorMessage);
            context.RespondAndStop(errorMessage);
            _logger.LogWarning("ThrottlingMiddleware: User {UserId} exceeded throttling limit of {MaxMessages} messages per {WindowSeconds} seconds",
                context.UserId, _maxMessages, _windowSize.TotalSeconds);
            return context;
        }

        return await next(context).ConfigureAwait(false);
    }

    private bool CheckThrottlingLimit(long userId)
    {
        try
        {
            _lock.EnterUpgradeableReadLock();

            if (!_messageTimestamps.TryGetValue(userId, out var timestamps))
            {
                // First message from this user
                timestamps = new List<DateTime>();
                _messageTimestamps[userId] = timestamps;
                return true;
            }

            var now = DateTime.UtcNow;
            var windowStart = now - _windowSize;

            // Remove timestamps outside the current window (sliding window algorithm)
            timestamps.RemoveAll(t => t < windowStart);

            if (timestamps.Count >= _maxMessages)
            {
                return false; // Throttled
            }

            // Add current message timestamp
            timestamps.Add(now);
            return true;
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }
}
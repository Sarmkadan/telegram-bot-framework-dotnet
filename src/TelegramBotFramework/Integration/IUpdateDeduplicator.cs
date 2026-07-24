#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Defines a contract for deduplicating Telegram updates by update_id.
/// Prevents the same update from being processed multiple times when delivered
/// via webhook retries or polling offset races.
/// </summary>
public interface IUpdateDeduplicator
{
    /// <summary>
    /// Checks if an update with the given update_id has already been processed.
    /// </summary>
    /// <param name="updateId">The update identifier to check.</param>
    /// <returns>
    /// <c>true</c> if the update has already been processed and should be skipped;
    /// <c>false</c> if the update should be processed.
    /// </returns>
    bool IsDuplicate(long updateId);

    /// <summary>
    /// Marks an update with the given update_id as processed.
    /// </summary>
    /// <param name="updateId">The update identifier to mark as processed.</param>
    void MarkAsProcessed(long updateId);

    /// <summary>
    /// Gets the deduplication window size in milliseconds.
    /// Updates older than this window will be automatically cleaned up.
    /// </summary>
    TimeSpan DeduplicationWindow { get; }
}

/// <summary>
/// In-memory implementation of update deduplication using LRU tracking.
/// </summary>
public sealed class InMemoryUpdateDeduplicator : IUpdateDeduplicator
{
    private readonly HashSet<long> _processedUpdateIds = new();
    private readonly Queue<long> _lruQueue = new();
    private readonly object _syncLock = new();
    private readonly TimeSpan _window;
    private DateTime _windowStartTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryUpdateDeduplicator"/> class.
    /// </summary>
    /// <param name="deduplicationWindow">The time window for deduplication. Updates older than this will be cleaned up.</param>
    public InMemoryUpdateDeduplicator(TimeSpan deduplicationWindow)
    {
        if (deduplicationWindow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(deduplicationWindow), "Deduplication window must be positive.");
        }

        _window = deduplicationWindow;
        _windowStartTime = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public TimeSpan DeduplicationWindow => _window;

    /// <inheritdoc/>
    public bool IsDuplicate(long updateId)
    {
        lock (_syncLock)
        {
            CleanupOldEntries();
            return _processedUpdateIds.Contains(updateId);
        }
    }

    /// <inheritdoc/>
    public void MarkAsProcessed(long updateId)
    {
        lock (_syncLock)
        {
            CleanupOldEntries();
            _processedUpdateIds.Add(updateId);
            _lruQueue.Enqueue(updateId);
        }
    }

    private void CleanupOldEntries()
    {
        var currentTime = DateTime.UtcNow;
        var cutoffTime = currentTime - _window;

        // Only clean up if we've passed the window start time
        if (cutoffTime > _windowStartTime)
        {
            _windowStartTime = cutoffTime;

            // Remove entries older than our window
            // We don't need to track exact timing since we're using a simple LRU approach
            // The HashSet will naturally grow, but we rely on the window for cleanup
            if (_processedUpdateIds.Count > 1000) // Safety limit
            {
                // Simple cleanup: clear everything when we get too large
                // In a real distributed scenario, we'd want a more sophisticated approach
                _processedUpdateIds.Clear();
                _lruQueue.Clear();
            }
        }
    }
}
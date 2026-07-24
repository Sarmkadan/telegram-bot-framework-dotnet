#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// An in-memory implementation of <see cref="IUpdateOffsetStore"/> that stores the last processed update offset.
/// This implementation is suitable for testing or scenarios where persistence is not required.
/// </summary>
public sealed class InMemoryUpdateOffsetStore : IUpdateOffsetStore
{
    private long _lastOffset = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryUpdateOffsetStore"/> class.
    /// </summary>
    public InMemoryUpdateOffsetStore()
    {
    }

    /// <summary>
    /// Gets the last processed update offset.
    /// </summary>
    /// <returns>The last processed update offset, or 0 if no updates have been processed.</returns>
    public long GetLastOffset()
    {
        return _lastOffset;
    }

    /// <summary>
    /// Sets the last processed update offset.
    /// </summary>
    /// <param name="offset">The update offset to store.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset is negative.</exception>
    public Task SetLastOffset(long offset)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");
        }

        _lastOffset = offset;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists any pending changes to the offset store.
    /// For in-memory implementation, this is a no-op since changes are immediate.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task PersistAsync()
    {
        return Task.CompletedTask;
    }
}
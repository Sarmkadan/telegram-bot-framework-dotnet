#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Defines a contract for storing and retrieving the last processed update offset.
/// Implementations can persist the offset to disk, database, or keep it in memory.
/// </summary>
public interface IUpdateOffsetStore
{
    /// <summary>
    /// Gets the last processed update offset.
    /// </summary>
    /// <returns>The last processed update offset, or 0 if no updates have been processed.</returns>
    long GetLastOffset();

    /// <summary>
    /// Sets the last processed update offset.
    /// </summary>
    /// <param name="offset">The update offset to store.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetLastOffset(long offset);

    /// <summary>
    /// Persists any pending changes to the offset store.
    /// </summary>
    Task PersistAsync();
}
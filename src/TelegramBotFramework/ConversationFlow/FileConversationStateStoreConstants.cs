#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Centralises magic values used by <see cref="FileConversationStateStore"/>.
/// </summary>
internal static class FileConversationStateStoreConstants
{
    /// <summary>
    /// Extension appended to every persisted state file name.
    /// </summary>
    public const string StateFileExtension = ".json";

    /// <summary>
    /// Search pattern used when enumerating persisted state files on disk.
    /// </summary>
    public const string StateFileSearchPattern = "*" + StateFileExtension;

    /// <summary>
    /// Error message raised when the configured state directory path is empty.
    /// </summary>
    public const string DirectoryPathEmptyError = "Directory path cannot be empty.";

    /// <summary>
    /// Initial count of the semaphore that serialises access to state files.
    /// Exactly one read/write operation may run at a time.
    /// </summary>
    public const int SingleAccessSemaphoreInitialCount = 1;

    /// <summary>
    /// Maximum count of the semaphore that serialises access to state files.
    /// Exactly one read/write operation may run at a time.
    /// </summary>
    public const int SingleAccessSemaphoreMaxCount = 1;
}

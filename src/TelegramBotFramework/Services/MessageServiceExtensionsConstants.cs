#nullable enable

namespace TelegramBotFramework.Services;

/// <summary>
/// Constants for MessageServiceExtensions.
/// </summary>
internal static class MessageServiceExtensionsConstants
{
    /// <summary>
    /// Default maximum number of messages to return.
    /// </summary>
    public const int DefaultMessageLimit = 50;

    /// <summary>
    /// Minimum allowed limit for message queries.
    /// </summary>
    public const int MinimumMessageLimit = 1;

    /// <summary>
    /// Maximum limit value used to retrieve all messages.
    /// </summary>
    public const int MaximumMessageLimit = int.MaxValue;
}
#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Constants for the IUserService.
/// </summary>
internal static class IUserServiceConstants
{
    /// <summary>
    /// Default limit for user messages retrieval.
    /// </summary>
    public const int DefaultUserMessagesLimit = 50;

    /// <summary>
    /// Default limit for failed messages retrieval.
    /// </summary>
    public const int DefaultFailedMessagesLimit = 100;

    /// <summary>
    /// Default number of days for archiving old messages.
    /// </summary>
    public const int DefaultArchiveDaysOld = 30;
}
#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Constants used throughout the FileConversationStateStoreExtensions and related types.
/// </summary>
internal static class FileConversationStateStoreExtensionsConstants
{
    /// <summary>
    /// Error message for when inactivity threshold is negative.
    /// </summary>
    public const string InactivityThresholdCannotBeNegative = "Inactivity threshold cannot be negative.";

    /// <summary>
    /// Error message for when maximum age is negative.
    /// </summary>
    public const string MaximumAgeCannotBeNegative = "Maximum age cannot be negative.";
}
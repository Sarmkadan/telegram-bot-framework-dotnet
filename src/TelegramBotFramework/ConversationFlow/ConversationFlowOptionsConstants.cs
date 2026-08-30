#nullable enable
namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Constants for ConversationFlowOptions.
/// </summary>
internal static class ConversationFlowOptionsConstants
{
    /// <summary>
    /// Default value for FlowAbandonedMessage.
    /// </summary>
    public const string FlowAbandonedMessageDefault = "Your conversation was interrupted. You can start over at any time.";

    /// <summary>
    /// Default value for FlowTimeoutMessage.
    /// </summary>
    public const string FlowTimeoutMessageDefault = "Your session has timed out due to inactivity. Please start the conversation again.";

    /// <summary>
    /// Default value for AbortKeyword.
    /// </summary>
    public const string AbortKeywordDefault = "/cancel";

    /// <summary>
    /// Default value for AbortAcknowledgementMessage.
    /// </summary>
    public const string AbortAcknowledgementMessageDefault = "Conversation cancelled. Use the menu to start again.";

    /// <summary>
    /// Default value for DefaultFlowTimeout in minutes.
    /// </summary>
    public const int DefaultFlowTimeoutInMinutes = 30;

    /// <summary>
    /// Default value for MaxActiveFlowsPerUser.
    /// </summary>
    public const int MaxActiveFlowsPerUserDefault = 1;

    /// <summary>
    /// Default value for MaxHistoryPerUser.
    /// </summary>
    public const int MaxHistoryPerUserDefault = 50;

    /// <summary>
    /// Default value for CleanupIntervalMinutes.
    /// </summary>
    public const int CleanupIntervalMinutesDefault = 60;
}
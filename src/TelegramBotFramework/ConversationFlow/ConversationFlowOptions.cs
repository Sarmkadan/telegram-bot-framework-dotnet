#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Configuration options for the <see cref="ConversationFlowEngine"/>.
/// Bind this class from <c>appsettings.json</c> under the <c>ConversationFlow</c> section
/// or configure it inline via <see cref="ConversationFlowExtensions.AddConversationFlows"/>.
/// </summary>
public sealed class ConversationFlowOptions
{
    /// <summary>
    /// Gets or sets the inactivity timeout applied to flows that do not define their own
    /// via <see cref="FlowDefinition.Timeout"/>. Defaults to 30 minutes.
    /// </summary>
    public TimeSpan DefaultFlowTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the maximum number of concurrent active flows per user.
    /// Because <see cref="ConversationFlowEngine"/> currently supports one flow per user at a time,
    /// starting a new flow while one is active aborts the existing one regardless of this setting.
    /// Defaults to 1.
    /// </summary>
    public int MaxActiveFlowsPerUser { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the engine should attempt to restore an
    /// in-progress flow from the user's session context when the user reconnects.
    /// Only flows with <see cref="FlowDefinition.AllowResume"/> set to <c>true</c> are eligible.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool AutoResumeOnSessionRestore { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of historical <see cref="UserFlowState"/> records
    /// retained in memory per user. Oldest records are evicted when the limit is exceeded.
    /// Defaults to 50.
    /// </summary>
    public int MaxHistoryPerUser { get; set; } = 50;

    /// <summary>
    /// Gets or sets the message sent to users when the engine aborts their flow due to a
    /// system-initiated interruption (e.g., a new flow starting). Defaults to a generic notice.
    /// </summary>
    public string FlowAbandonedMessage { get; set; } =
        "Your conversation was interrupted. You can start over at any time.";

    /// <summary>
    /// Gets or sets the message sent to users whose active flow was automatically terminated
    /// because the inactivity timeout elapsed. Defaults to a generic timeout notice.
    /// </summary>
    public string FlowTimeoutMessage { get; set; } =
        "Your session has timed out due to inactivity. Please start the conversation again.";

    /// <summary>
    /// Gets or sets a value indicating whether the engine publishes lifecycle events
    /// (<see cref="FlowStartedEvent"/>, <see cref="FlowStepCompletedEvent"/>,
    /// <see cref="FlowCompletedEvent"/>, <see cref="FlowAbortedEvent"/>) to the
    /// <see cref="Events.IEventBus"/>. Disable to reduce overhead when no event handlers are wired.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableFlowEvents { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval in minutes between periodic cleanup sweeps that remove
    /// timed-out flow states from memory. Defaults to 60 minutes.
    /// </summary>
    public int CleanupIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the keyword a user can type at any step to immediately abort the active flow.
    /// Comparison is case-insensitive. Set to <c>null</c> or an empty string to disable
    /// the abort shortcut. Defaults to <c>/cancel</c>.
    /// </summary>
    public string? AbortKeyword { get; set; } = "/cancel";

    /// <summary>
    /// Gets or sets the message sent to a user when they trigger the <see cref="AbortKeyword"/>.
    /// Defaults to a short confirmation message.
    /// </summary>
    public string AbortAcknowledgementMessage { get; set; } =
        "Conversation cancelled. Use the menu to start again.";
}
#nullable enable
namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Defines configuration options for the conversation flow engine.
/// </summary>
public interface IConversationFlowOptions
{
    /// <summary>
    /// Gets or sets the inactivity timeout applied to flows that do not define their own
    /// via <see cref="FlowDefinition.Timeout"/>. Defaults to 30 minutes.
    /// </summary>
    TimeSpan DefaultFlowTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent active flows per user.
    /// Because <see cref="ConversationFlowEngine"/> currently supports one flow per user at a time,
    /// starting a new flow while one is active aborts the existing one regardless of this setting.
    /// Defaults to 1.
    /// </summary>
    int MaxActiveFlowsPerUser { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the engine should attempt to restore an
    /// in-progress flow from the user's session context when the user reconnects.
    /// Only flows with <see cref="FlowDefinition.AllowResume"/> set to <c>true</c> are eligible.
    /// Defaults to <c>true</c>.
    /// </summary>
    bool AutoResumeOnSessionRestore { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of historical <see cref="UserFlowState"/> records
    /// retained in memory per user. Oldest records are evicted when the limit is exceeded.
    /// Defaults to 50.
    /// </summary>
    int MaxHistoryPerUser { get; set; }

    /// <summary>
    /// Gets or sets the message sent to users when the engine aborts their flow due to a
    /// system-initiated interruption (e.g., a new flow starting). Defaults to a generic notice.
    /// </summary>
    string FlowAbandonedMessage { get; set; }

    /// <summary>
    /// Gets or sets the message sent to users whose active flow was automatically terminated
    /// because the inactivity timeout elapsed. Defaults to a generic timeout notice.
    /// </summary>
    string FlowTimeoutMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the engine publishes lifecycle events
    /// (<see cref="FlowStartedEvent"/>, <see cref="FlowStepCompletedEvent"/>,
    /// <see cref="FlowCompletedEvent"/>, <see cref="FlowAbortedEvent"/>) to the
    /// <see cref="Events.IEventBus"/>. Disable to reduce overhead when no event handlers are wired.
    /// Defaults to <c>true</c>.
    /// </summary>
    bool EnableFlowEvents { get; set; }

    /// <summary>
    /// Gets or sets the interval in minutes between periodic cleanup sweeps that remove
    /// timed-out flow states from memory. Defaults to 60 minutes.
    /// </summary>
    int CleanupIntervalMinutes { get; set; }

    /// <summary>
    /// Gets or sets the keyword a user can type at any step to immediately abort the active flow.
    /// Comparison is case-insensitive. Set to <c>null</c> or an empty string to disable
    /// the abort shortcut. Defaults to <c>/cancel</c>.
    /// </summary>
    string? AbortKeyword { get; set; }

    /// <summary>
    /// Gets or sets the message sent to a user when they trigger the <see cref="AbortKeyword"/>.
    /// Defaults to a short confirmation message.
    /// </summary>
    string AbortAcknowledgementMessage { get; set; }

    /// <summary>
    /// Gets or sets the action taken when a cleanup sweep finds a timed-out flow state.
    /// Defaults to <see cref="FlowEvictionPolicy.SilentDiscard"/>.
    /// </summary>
    FlowEvictionPolicy TimeoutEvictionPolicy { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked for each evicted <see cref="UserFlowState"/>
    /// during a cleanup sweep. Use this to send a timeout notification to the user or persist
    /// collected variables before the state is discarded.
    /// Invoked when <see cref="TimeoutEvictionPolicy"/> is <see cref="FlowEvictionPolicy.NotifyUser"/>
    /// or as a general eviction hook for the other policies.
    /// </summary>
    Func<UserFlowState, CancellationToken, Task>? OnEviction { get; set; }
}
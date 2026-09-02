#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Defines the action taken when an idle conversation flow state is evicted
/// due to inactivity timeout during a cleanup sweep.
/// </summary>
public enum FlowEvictionPolicy
{
    /// <summary>
    /// Silently removes the timed-out state without notifying the user.
    /// The user's next interaction will begin a fresh flow.
    /// </summary>
    SilentDiscard,

    /// <summary>
    /// Invokes the <see cref="ConversationFlowOptions.OnEviction"/> callback before discarding
    /// the state, allowing the host application to send a notification message to the user.
    /// </summary>
    NotifyUser,

    /// <summary>
    /// Resets the flow state back to the initial step instead of removing it, so the user
    /// can continue from the beginning without explicitly restarting the flow.
    /// </summary>
    ResetToInitialStep
}

/// <summary>
/// Configuration options for the <see cref="ConversationFlowEngine"/>.
/// Bind this class from <c>appsettings.json</c> under the <c>ConversationFlow</c> section
/// or configure it inline via <see cref="ConversationFlowExtensions.AddConversationFlows"/>.
/// </summary>
public sealed class ConversationFlowOptions : IConversationFlowOptions, IEquatable<ConversationFlowOptions>
{
    /// <summary>
    /// Gets or sets the inactivity timeout applied to flows that do not define their own
    /// via <see cref="FlowDefinition.Timeout"/>. Defaults to 30 minutes.
    /// </summary>
    public TimeSpan DefaultFlowTimeout { get; set; } = TimeSpan.FromMinutes(ConversationFlowOptionsConstants.DefaultFlowTimeoutInMinutes);

    /// <summary>
    /// Gets or sets the maximum number of concurrent active flows per user.
    /// Because <see cref="ConversationFlowEngine"/> currently supports one flow per user at a time,
    /// starting a new flow while one is active aborts the existing one regardless of this setting.
    /// Defaults to 1.
    /// </summary>
    public int MaxActiveFlowsPerUser { get; set; } = ConversationFlowOptionsConstants.MaxActiveFlowsPerUserDefault;

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
    public int MaxHistoryPerUser { get; set; } = ConversationFlowOptionsConstants.MaxHistoryPerUserDefault;

    /// <summary>
    /// Gets or sets the message sent to users when the engine aborts their flow due to a
    /// system-initiated interruption (e.g., a new flow starting). Defaults to a generic notice.
    /// </summary>
    public string FlowAbandonedMessage { get; set; } =
        ConversationFlowOptionsConstants.FlowAbandonedMessageDefault;

    /// <summary>
    /// Gets or sets the message sent to users whose active flow was automatically terminated
    /// because the inactivity timeout elapsed. Defaults to a generic timeout notice.
    /// </summary>
    public string FlowTimeoutMessage { get; set; } =
        ConversationFlowOptionsConstants.FlowTimeoutMessageDefault;

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
    public int CleanupIntervalMinutes { get; set; } = ConversationFlowOptionsConstants.CleanupIntervalMinutesDefault;

    /// <summary>
    /// Gets or sets the keyword a user can type at any step to immediately abort the active flow.
    /// Comparison is case-insensitive. Set to <c>null</c> or an empty string to disable
    /// the abort shortcut. Defaults to <c>/cancel</c>.
    /// </summary>
    public string? AbortKeyword { get; set; } = ConversationFlowOptionsConstants.AbortKeywordDefault;

    /// <summary>
    /// Gets or sets the message sent to a user when they trigger the <see cref="AbortKeyword"/>.
    /// Defaults to a short confirmation message.
    /// </summary>
    public string AbortAcknowledgementMessage { get; set; } =
        ConversationFlowOptionsConstants.AbortAcknowledgementMessageDefault;

    /// <summary>
    /// Gets or sets the action taken when a cleanup sweep finds a timed-out flow state.
    /// Defaults to <see cref="FlowEvictionPolicy.SilentDiscard"/>.
    /// </summary>
    public FlowEvictionPolicy TimeoutEvictionPolicy { get; set; } = FlowEvictionPolicy.SilentDiscard;

    /// <summary>
    /// Gets or sets an optional callback invoked for each evicted <see cref="UserFlowState"/>
    /// during a cleanup sweep. Use this to send a timeout notification to the user or persist
    /// collected variables before the state is discarded.
    /// Invoked when <see cref="TimeoutEvictionPolicy"/> is <see cref="FlowEvictionPolicy.NotifyUser"/>
    /// or as a general eviction hook for the other policies.
    /// </summary>
    public Func<UserFlowState, CancellationToken, Task>? OnEviction { get; set; }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(ConversationFlowOptions? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DefaultFlowTimeout == other.DefaultFlowTimeout &&
               MaxActiveFlowsPerUser == other.MaxActiveFlowsPerUser &&
               AutoResumeOnSessionRestore == other.AutoResumeOnSessionRestore &&
               MaxHistoryPerUser == other.MaxHistoryPerUser &&
               string.Equals(FlowAbandonedMessage, other.FlowAbandonedMessage, StringComparison.Ordinal) &&
               string.Equals(FlowTimeoutMessage, other.FlowTimeoutMessage, StringComparison.Ordinal) &&
               EnableFlowEvents == other.EnableFlowEvents &&
               CleanupIntervalMinutes == other.CleanupIntervalMinutes;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ConversationFlowOptions)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            DefaultFlowTimeout,
            MaxActiveFlowsPerUser,
            AutoResumeOnSessionRestore,
            MaxHistoryPerUser,
            FlowAbandonedMessage,
            FlowTimeoutMessage,
            EnableFlowEvents,
            CleanupIntervalMinutes);
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="ConversationFlowOptions"/>.
    /// </summary>
    public override string ToString() =>
        $"ConversationFlowOptions {{ DefaultFlowTimeout = {DefaultFlowTimeout}, MaxActiveFlowsPerUser = {MaxActiveFlowsPerUser}, AutoResumeOnSessionRestore = {AutoResumeOnSessionRestore}, MaxHistoryPerUser = {MaxHistoryPerUser}, FlowAbandonedMessage = {FlowAbandonedMessage}, FlowTimeoutMessage = {FlowTimeoutMessage} }}";

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="ConversationFlowOptions"/> objects are equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(ConversationFlowOptions? left, ConversationFlowOptions? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="ConversationFlowOptions"/> objects are not equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
    public static bool operator !=(ConversationFlowOptions? left, ConversationFlowOptions? right)
    {
        return !(left == right);
    }
}
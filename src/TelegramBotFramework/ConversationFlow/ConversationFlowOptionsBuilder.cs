#nullable enable
namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Builder for <see cref="ConversationFlowOptions"/> objects.
/// </summary>
public sealed class ConversationFlowOptionsBuilder
{
    private TimeSpan _defaultFlowTimeout = TimeSpan.FromMinutes(30);
    private int _maxActiveFlowsPerUser = 1;
    private bool _autoResumeOnSessionRestore = true;
    private int _maxHistoryPerUser = 50;
    private string _flowAbandonedMessage = "Your conversation was interrupted. You can start over at any time.";
    private string _flowTimeoutMessage = "Your session has timed out due to inactivity. Please start the conversation again.";
    private bool _enableFlowEvents = true;
    private int _cleanupIntervalMinutes = 60;
    private string? _abortKeyword = "/cancel";
    private string _abortAcknowledgementMessage = "Conversation cancelled. Use the menu to start again.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationFlowOptionsBuilder"/> class with default values.
    /// </summary>
    public ConversationFlowOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the default flow timeout.
    /// </summary>
    /// <param name="value">The timeout value.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative or zero.</exception>
    public ConversationFlowOptionsBuilder WithDefaultFlowTimeout(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Timeout must be positive.");
        }

        _defaultFlowTimeout = value;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of concurrent active flows per user.
    /// </summary>
    /// <param name="value">The maximum number of flows.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1.</exception>
    public ConversationFlowOptionsBuilder WithMaxActiveFlowsPerUser(int value)
    {
        if (value < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Maximum active flows per user must be at least 1.");
        }

        _maxActiveFlowsPerUser = value;
        return this;
    }

    /// <summary>
    /// Sets whether the engine should attempt to restore an in-progress flow from the user's session context when the user reconnects.
    /// </summary>
    /// <param name="value">Whether to auto-resume flows.</param>
    /// <returns>This builder instance.</returns>
    public ConversationFlowOptionsBuilder WithAutoResumeOnSessionRestore(bool value)
    {
        _autoResumeOnSessionRestore = value;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of historical flow records retained in memory per user.
    /// </summary>
    /// <param name="value">The maximum history count.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 0.</exception>
    public ConversationFlowOptionsBuilder WithMaxHistoryPerUser(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Maximum history per user cannot be negative.");
        }

        _maxHistoryPerUser = value;
        return this;
    }

    /// <summary>
    /// Sets the message sent to users when the engine aborts their flow due to a system-initiated interruption.
    /// </summary>
    /// <param name="value">The abandoned message.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or consists only of white-space.</exception>
    public ConversationFlowOptionsBuilder WithFlowAbandonedMessage(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _flowAbandonedMessage = value;
        return this;
    }

    /// <summary>
    /// Sets the message sent to users whose active flow was automatically terminated because the inactivity timeout elapsed.
    /// </summary>
    /// <param name="value">The timeout message.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or consists only of white-space.</exception>
    public ConversationFlowOptionsBuilder WithFlowTimeoutMessage(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _flowTimeoutMessage = value;
        return this;
    }

    /// <summary>
    /// Sets whether the engine publishes lifecycle events to the event bus.
    /// </summary>
    /// <param name="value">Whether to enable flow events.</param>
    /// <returns>This builder instance.</returns>
    public ConversationFlowOptionsBuilder WithEnableFlowEvents(bool value)
    {
        _enableFlowEvents = value;
        return this;
    }

    /// <summary>
    /// Sets the interval in minutes between periodic cleanup sweeps.
    /// </summary>
    /// <param name="value">The cleanup interval in minutes.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1.</exception>
    public ConversationFlowOptionsBuilder WithCleanupIntervalMinutes(int value)
    {
        if (value < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Cleanup interval must be at least 1 minute.");
        }

        _cleanupIntervalMinutes = value;
        return this;
    }

    /// <summary>
    /// Sets the keyword a user can type at any step to immediately abort the active flow.
    /// </summary>
    /// <param name="value">The abort keyword (case-insensitive), or null or empty to disable.</param>
    /// <returns>This builder instance.</returns>
    public ConversationFlowOptionsBuilder WithAbortKeyword(string? value)
    {
        _abortKeyword = value;
        return this;
    }

    /// <summary>
    /// Sets the message sent to a user when they trigger the abort keyword.
    /// </summary>
    /// <param name="value">The abort acknowledgement message.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or consists only of white-space.</exception>
    public ConversationFlowOptionsBuilder WithAbortAcknowledgementMessage(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _abortAcknowledgementMessage = value;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="ConversationFlowOptions"/> instance with the values set on this builder.
    /// </summary>
    /// <returns>A configured <see cref="ConversationFlowOptions"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public ConversationFlowOptions Build()
    {
        // Validate that required properties are set (all properties have defaults, so no validation needed)
        // However, we validate that the values are within acceptable ranges (already validated in With methods)

        return new ConversationFlowOptions
        {
            DefaultFlowTimeout = _defaultFlowTimeout,
            MaxActiveFlowsPerUser = _maxActiveFlowsPerUser,
            AutoResumeOnSessionRestore = _autoResumeOnSessionRestore,
            MaxHistoryPerUser = _maxHistoryPerUser,
            FlowAbandonedMessage = _flowAbandonedMessage,
            FlowTimeoutMessage = _flowTimeoutMessage,
            EnableFlowEvents = _enableFlowEvents,
            CleanupIntervalMinutes = _cleanupIntervalMinutes,
            AbortKeyword = _abortKeyword,
            AbortAcknowledgementMessage = _abortAcknowledgementMessage,
            // Properties not in the builder use their default values
            TimeoutEvictionPolicy = FlowEvictionPolicy.SilentDiscard,
            OnEviction = null
        };
    }

    /// <summary>
    /// Creates a new builder initialized with the values from the specified <see cref="ConversationFlowOptions"/> instance.
    /// </summary>
    /// <param name="template">The options instance to copy values from.</param>
    /// <returns>A new builder pre-filled with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ConversationFlowOptionsBuilder From(ConversationFlowOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new ConversationFlowOptionsBuilder
        {
            _defaultFlowTimeout = template.DefaultFlowTimeout,
            _maxActiveFlowsPerUser = template.MaxActiveFlowsPerUser,
            _autoResumeOnSessionRestore = template.AutoResumeOnSessionRestore,
            _maxHistoryPerUser = template.MaxHistoryPerUser,
            _flowAbandonedMessage = template.FlowAbandonedMessage,
            _flowTimeoutMessage = template.FlowTimeoutMessage,
            _enableFlowEvents = template.EnableFlowEvents,
            _cleanupIntervalMinutes = template.CleanupIntervalMinutes,
            _abortKeyword = template.AbortKeyword,
            _abortAcknowledgementMessage = template.AbortAcknowledgementMessage
        };
    }
}
#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Events;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Thread-safe, in-process implementation of <see cref="IConversationFlowEngine"/>.
/// Maintains flow definitions and per-user runtime states in concurrent dictionaries,
/// integrates with the session layer for persistence across reconnects, and publishes
/// lifecycle events to the <see cref="IEventBus"/>.
/// When an <see cref="IConversationStateStore"/> is provided, states are persisted on
/// every mutation and restored into memory on the first call to any execution method.
/// </summary>
public sealed class ConversationFlowEngine : IConversationFlowEngine
{
    private readonly ConcurrentDictionary<string, FlowDefinition> _flows = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<long, UserFlowState> _activeStates = new();
    private readonly ConcurrentDictionary<long, List<UserFlowState>> _history = new();

    private readonly ConversationFlowOptions _options;
    private readonly ISessionService _sessionService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ConversationFlowEngine> _logger;
    private readonly IConversationStateStore? _stateStore;

    private readonly object _historyLock = new();

    /// <summary>
    /// Initialises a new instance of <see cref="ConversationFlowEngine"/> without state persistence.
    /// </summary>
    /// <param name="options">The options that control conversation flow execution.</param>
    /// <param name="sessionService">The service used to access and update user sessions.</param>
    /// <param name="eventBus">The event bus used to publish conversation flow lifecycle events.</param>
    /// <param name="logger">The logger used to record conversation flow activity.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/>, <paramref name="sessionService"/>,
    /// <paramref name="eventBus"/>, or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public ConversationFlowEngine(
        ConversationFlowOptions options,
        ISessionService sessionService,
        IEventBus eventBus,
        ILogger<ConversationFlowEngine> logger)
        : this(options, sessionService, eventBus, logger, null)
    {
    }

    /// <summary>
    /// Initialises a new instance of <see cref="ConversationFlowEngine"/> with optional state persistence.
    /// </summary>
    /// <param name="options">The options that control conversation flow execution.</param>
    /// <param name="sessionService">The service used to access and update user sessions.</param>
    /// <param name="eventBus">The event bus used to publish conversation flow lifecycle events.</param>
    /// <param name="logger">The logger used to record conversation flow activity.</param>
    /// <param name="stateStore">
    /// The optional store used to persist active states on mutation and remove terminal states.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/>, <paramref name="sessionService"/>,
    /// <paramref name="eventBus"/>, or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public ConversationFlowEngine(
        ConversationFlowOptions options,
        ISessionService sessionService,
        IEventBus eventBus,
        ILogger<ConversationFlowEngine> logger,
        IConversationStateStore? stateStore)
    {
        _options        = options        ?? throw new ArgumentNullException(nameof(options));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _eventBus       = eventBus       ?? throw new ArgumentNullException(nameof(eventBus));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
        _stateStore     = stateStore;
    }

    // -------------------------------------------------------------------------
    // Registration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a flow definition, replacing any existing definition with the same identifier.
    /// </summary>
    /// <param name="flow">The flow definition to register.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task that represents the registration operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flow"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the flow identifier is empty or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the initial step is not present in the flow's steps.</exception>
    public Task RegisterFlowAsync(FlowDefinition flow, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(flow);

        if (string.IsNullOrWhiteSpace(flow.FlowId))
            throw new ArgumentException("FlowId must not be empty.", nameof(flow));

        if (!flow.Steps.Any(s => s.StepId == flow.InitialStepId))
            throw new InvalidOperationException(
                $"Flow '{flow.FlowId}' references InitialStepId '{flow.InitialStepId}' that does not exist in Steps.");

        _flows[flow.FlowId] = flow;

        _logger.LogInformation(ConversationFlowEngineConstants.FlowRegisteredLog,
            flow.FlowId, flow.Name, flow.Steps.Count);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the flow definition with the specified identifier from the engine.
    /// </summary>
    /// <param name="flowId">The identifier of the flow to remove.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task that represents the removal operation.</returns>
    public Task UnregisterFlowAsync(string flowId, CancellationToken cancellationToken = default)
    {
        _flows.TryRemove(flowId, out _);
        _logger.LogInformation(ConversationFlowEngineConstants.FlowUnregisteredLog, flowId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the registered flow definition with the specified identifier.
    /// </summary>
    /// <param name="flowId">The identifier of the flow to retrieve.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>
    /// A task whose result is the matching flow definition, or <see langword="null"/> when no matching flow is registered.
    /// </returns>
    public Task<FlowDefinition?> GetFlowAsync(string flowId, CancellationToken cancellationToken = default)
        => Task.FromResult(_flows.TryGetValue(flowId, out var flow) ? flow : (FlowDefinition?)null);

    /// <summary>
    /// Gets a snapshot of all registered flow definitions.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task whose result contains the registered flow definitions.</returns>
    public Task<IReadOnlyList<FlowDefinition>> GetAllFlowsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FlowDefinition>>(_flows.Values.ToList());

    // -------------------------------------------------------------------------
    // Execution — Start
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a flow for a user, aborting any flow already active for that user.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="chatId">The Telegram chat identifier.</param>
    /// <param name="flowId">The identifier of the registered flow to start.</param>
    /// <param name="initialVariables">Optional variables with which to initialize the new flow state.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task whose result is the newly created flow state positioned at its initial step.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="flowId"/> is not registered.</exception>
    public async Task<UserFlowState> StartFlowAsync(
        long userId,
        long chatId,
        string flowId,
        Dictionary<string, string>? initialVariables = null,
        CancellationToken cancellationToken = default)
    {
        var flow = EnsureFlowExists(flowId);
        await HandleExistingActiveFlowAsync(userId, flowId, cancellationToken).ConfigureAwait(false);

        var state = CreateInitialState(userId, chatId, flow, initialVariables);

        ActivateState(state);
        await PersistAndPublishStateAsync(state, cancellationToken).ConfigureAwait(false);

        return state;
    }

    // -------------------------------------------------------------------------
    // Execution — Input Processing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates and processes user input against the current step of the user's active flow.
    /// </summary>
    /// <param name="userId">The Telegram user identifier whose active flow receives the input.</param>
    /// <param name="input">The raw input to process.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>
    /// A task whose result describes validation, the next prompt, and whether the flow has completed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the user has no active flow, its definition is no longer registered, or its current step does not exist.
    /// </exception>
    public async Task<FlowStepResult> ProcessInputAsync(
        long userId,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_activeStates.TryGetValue(userId, out var state))
            throw new InvalidOperationException(
                $"User {userId} has no active conversation flow. Call StartFlowAsync first.");

        try
        {
            return await ProcessInputCoreAsync(userId, state, input, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Terminate the flow so the user is not permanently stuck in a broken state.
            await TerminateAsync(state, FlowStateStatus.Failed, ex.Message).ConfigureAwait(false);

            _logger.LogError(ex,
                "Unhandled exception during flow step processing — UserId: {UserId}, FlowId: {FlowId}, " +
                "StepId: {StepId}. Flow has been terminated to prevent inconsistent state.",
                userId, state.FlowId, state.CurrentStepId);

            // Re-throw so the global middleware error handler receives the exception.
            throw;
        }
    }

    private async Task<FlowStepResult> ProcessInputCoreAsync(
        long userId,
        UserFlowState state,
        string input,
        CancellationToken cancellationToken)
    {
        var (flow, step) = GetCurrentFlowAndStep(state);

        // --- Abort keyword shortcut ---
        if (!string.IsNullOrEmpty(_options.AbortKeyword) &&
            string.Equals(input.Trim(), _options.AbortKeyword, StringComparison.OrdinalIgnoreCase))
        {
            await AbortFlowAsync(userId, "User triggered abort keyword", cancellationToken).ConfigureAwait(false);
            return BuildTerminalResult(state, _options.AbortAcknowledgementMessage);
        }

        // --- Inactivity timeout check ---
        var effectiveTimeout = flow.Timeout ?? _options.DefaultFlowTimeout;
        if (DateTime.UtcNow - state.LastActivityAt > effectiveTimeout)
        {
            await TerminateAsync(state, FlowStateStatus.TimedOut, "Inactivity timeout").ConfigureAwait(false);
            return BuildTerminalResult(state, _options.FlowTimeoutMessage);
        }

        // --- Validate input ---
        var (isValid, validationError) = ValidateInput(step, input);
        if (!isValid)
        {
            _logger.LogDebug(
                ConversationFlowEngineConstants.ValidationFailedLog,
                userId, step.StepId, validationError);

            return BuildValidationFailureResult(state, step, validationError);
        }

        // --- Store variable ---
        if (!string.IsNullOrWhiteSpace(step.VariableName))
            state.Variables[step.VariableName] = input;

        var stepEnteredAt   = state.LastActivityAt;
        state.LastActivityAt = DateTime.UtcNow;

        // --- Resolve next step ---
        var nextStepId = ResolveNextStep(step, state.Variables);

        RecordCompletedStep(state, step, input, nextStepId, stepEnteredAt);

        if (_options.EnableFlowEvents)
            await _eventBus.PublishAsync(
                new FlowStepCompletedEvent(userId, state.FlowId, step.StepId, nextStepId)).ConfigureAwait(false);

        // --- Terminal step or no outgoing path ---
        if (step.IsTerminal || nextStepId  is null)
        {
            await TerminateAsync(state, FlowStateStatus.Completed, null).ConfigureAwait(false);

            if (_options.EnableFlowEvents)
                await _eventBus.PublishAsync(
                    new FlowCompletedEvent(userId, state.ChatId, state.FlowId, state.StateId)).ConfigureAwait(false);

            _logger.LogInformation(
                ConversationFlowEngineConstants.FlowCompletedLog,
                userId, state.FlowId, state.History.Count);

            return BuildCompletedResult(state, flow.CompletionMenuId);
        }

        // --- Advance to next step ---
        state.CurrentStepId = nextStepId;
        state.Status        = FlowStateStatus.WaitingForInput;

        await SaveStateAsync(state, cancellationToken).ConfigureAwait(false);

        var nextStep = flow.Steps.FirstOrDefault(s => s.StepId == nextStepId);

        _logger.LogDebug(
            "Flow advanced — UserId: {UserId}, FlowId: {FlowId}, Step: {StepId} → {NextStepId}",
            userId, state.FlowId, step.StepId, nextStepId);

        return new FlowStepResult
        {
            IsValid      = true,
            Prompt       = nextStep?.Prompt ?? string.Empty,
            QuickReplies = nextStep?.QuickReplies,
            IsCompleted  = false,
            FlowState    = state
        };
    }

    // -------------------------------------------------------------------------
    // Execution — State Management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the active flow state for a user.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>
    /// A task whose result is the active flow state, or <see langword="null"/> when the user has no active flow.
    /// </returns>
    public Task<UserFlowState?> GetActiveFlowStateAsync(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_activeStates.TryGetValue(userId, out var s) ? s : (UserFlowState?)null);

    /// <summary>
    /// Aborts the active flow for a user and records the supplied reason.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="reason">A human-readable reason for aborting the flow.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task that represents the abort operation.</returns>
    public async Task AbortFlowAsync(long userId, string reason, CancellationToken cancellationToken = default)
    {
        if (!_activeStates.TryGetValue(userId, out var state))
            return;

        await TerminateAsync(state, FlowStateStatus.Aborted, reason).ConfigureAwait(false);

        if (_options.EnableFlowEvents)
            await _eventBus.PublishAsync(new FlowAbortedEvent(userId, state.FlowId, reason)).ConfigureAwait(false);

        _logger.LogInformation(
            "Flow aborted — UserId: {UserId}, FlowId: {FlowId}, Reason: {Reason}",
            userId, state.FlowId, reason);
    }

    /// <summary>
    /// Attempts to resume a suspended in-memory flow for a user.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>
    /// A task whose result is the resumed state, or <see langword="null"/> when no resumable state exists.
    /// </returns>
    public async Task<UserFlowState?> ResumeFlowAsync(long userId, CancellationToken cancellationToken = default)
    {
        if (_activeStates.TryGetValue(userId, out var state) &&
            state.Status == FlowStateStatus.Suspended)
        {
            state.Status         = FlowStateStatus.WaitingForInput;
            state.LastActivityAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Flow resumed — UserId: {UserId}, FlowId: {FlowId}, Step: {StepId}",
                userId, state.FlowId, state.CurrentStepId);

            return state;
        }

        if (!_options.AutoResumeOnSessionRestore)
            return null;

        // Attempt to detect a flow that was in progress before the engine restarted.
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        if (session  is null) return null;

        var restoredFlowId = await _sessionService.GetSessionContextAsync(
            session.SessionId, SessionKeys.FlowId, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(restoredFlowId))
        {
            _logger.LogWarning(
                "Flow '{FlowId}' was in-progress for UserId {UserId} but state is not in memory — " +
                "restart the flow to continue.",
                restoredFlowId, userId);
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Querying
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the most recent flow states for a user in descending start-time order.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="limit">The maximum number of states to return; values less than one are treated as one.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task whose result contains a snapshot of the user's flow history.</returns>
    public Task<IReadOnlyList<UserFlowState>> GetFlowHistoryAsync(
        long userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (!_history.TryGetValue(userId, out var list))
            return Task.FromResult<IReadOnlyList<UserFlowState>>([]);

        List<UserFlowState> snapshot;
        lock (_historyLock)
            snapshot = list.ToList();

        var result = snapshot
            .OrderByDescending(s => s.StartedAt)
            .Take(Math.Max(1, limit))
            .ToList();

        return Task.FromResult<IReadOnlyList<UserFlowState>>(result);
    }

    /// <summary>
    /// Determines whether a user has an active flow or one waiting for input.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> when the user is in a flow; otherwise, <see langword="false"/>.
    /// </returns>
    public Task<bool> IsUserInFlowAsync(long userId, CancellationToken cancellationToken = default)
    {
        var active = _activeStates.TryGetValue(userId, out var state) &&
                     state.Status is FlowStateStatus.Active or FlowStateStatus.WaitingForInput;
        return Task.FromResult(active);
    }

    /// <summary>
    /// Applies the configured eviction policy to all active flow states whose inactivity timeout has elapsed.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A task whose result is the number of expired states processed.</returns>
    public async Task<int> CleanupExpiredFlowStatesAsync(CancellationToken cancellationToken = default)
    {
        var cleaned = 0;

        foreach (var (userId, state) in _activeStates)
        {
            if (!_flows.TryGetValue(state.FlowId, out var flow)) continue;

            var timeout = flow.Timeout ?? _options.DefaultFlowTimeout;
            if (DateTime.UtcNow - state.LastActivityAt <= timeout) continue;

            switch (_options.TimeoutEvictionPolicy)
            {
                case FlowEvictionPolicy.ResetToInitialStep:
                    state.CurrentStepId  = flow.InitialStepId;
                    state.Status         = FlowStateStatus.WaitingForInput;
                    state.LastActivityAt = DateTime.UtcNow;
                    state.AbortReason    = null;
                    _logger.LogInformation(
                        "Flow reset to initial step after timeout — UserId: {UserId}, FlowId: {FlowId}",
                        userId, state.FlowId);
                    break;

                default:
                    state.Status      = FlowStateStatus.TimedOut;
                    state.CompletedAt = DateTime.UtcNow;
                    state.AbortReason = "Inactivity timeout (cleanup sweep)";
                    _activeStates.TryRemove(userId, out _);

                    if (_options.OnEviction is not null)
                    {
                        try
                        {
                            await _options.OnEviction(state, cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "OnEviction callback threw for UserId: {UserId}, FlowId: {FlowId}",
                                userId, state.FlowId);
                            // Eviction callbacks are notifications; a failure must not stop the cleanup sweep.
                        }
                    }
                    break;
            }

            cleaned++;
        }

        if (cleaned > 0)
            _logger.LogInformation("Cleanup processed {Count} expired flow states", cleaned);

        return cleaned;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task TerminateAsync(UserFlowState state, FlowStateStatus status, string? reason)
    {
        state.Status      = status;
        state.CompletedAt = DateTime.UtcNow;
        state.AbortReason = reason;
        _activeStates.TryRemove(state.UserId, out _);

        // Persist final state for audit trail then clean up active entry.
        await SaveStateAsync(state, CancellationToken.None).ConfigureAwait(false);
        await ExecuteStateStoreOperationAsync(
            store => store.DeleteStateAsync(state.UserId),
            "delete",
            state).ConfigureAwait(false);
    }

    private static UserFlowState CreateInitialState(
        long userId,
        long chatId,
        FlowDefinition flow,
        Dictionary<string, string>? initialVariables)
    {
        var state = new UserFlowState
        {
            StateId        = Guid.NewGuid().ToString(ConversationFlowEngineConstants.GuidFormat),
            FlowId         = flow.FlowId,
            UserId         = userId,
            ChatId         = chatId,
            CurrentStepId  = flow.InitialStepId,
            Status         = FlowStateStatus.WaitingForInput,
            StartedAt      = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        if (initialVariables is not null)
        {
            foreach (var (key, value) in initialVariables)
                state.Variables[key] = value;
        }

        return state;
    }

    private async Task MirrorFlowContextToSessionAsync(
        UserFlowState state,
        CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetActiveSessionAsync(
            state.UserId, cancellationToken).ConfigureAwait(false);
        if (session is null)
            return;

        await _sessionService.UpdateSessionContextAsync(
            session.SessionId, SessionKeys.FlowId, state.FlowId, cancellationToken).ConfigureAwait(false);
        await _sessionService.UpdateSessionContextAsync(
            session.SessionId, SessionKeys.FlowStateId, state.StateId, cancellationToken).ConfigureAwait(false);
    }

    private async Task PublishFlowStartedAsync(UserFlowState state)
    {
        if (!_options.EnableFlowEvents)
            return;

        await _eventBus.PublishAsync(
            new FlowStartedEvent(state.UserId, state.ChatId, state.FlowId, state.StateId)).ConfigureAwait(false);
    }

    private (FlowDefinition flow, FlowStep step) GetCurrentFlowAndStep(UserFlowState state)
    {
        if (!_flows.TryGetValue(state.FlowId, out var flow))
            throw new InvalidOperationException(
                $"Flow definition '{state.FlowId}' is no longer registered.");

        var step = flow.Steps.FirstOrDefault(candidate => candidate.StepId == state.CurrentStepId)
            ?? throw new InvalidOperationException(
                $"Step '{state.CurrentStepId}' not found in flow '{state.FlowId}'.");

        return (flow, step);
    }

    private static FlowStepResult BuildValidationFailureResult(
        UserFlowState state,
        FlowStep step,
        string? validationError)
    {
        var errorPrompt = string.IsNullOrEmpty(step.HelpText)
            ? $"{validationError}\n\n{step.Prompt}"
            : $"{validationError}\n\n{step.Prompt}\n{step.HelpText}";

        return new FlowStepResult
        {
            IsValid         = false,
            ValidationError = validationError,
            Prompt          = errorPrompt,
            QuickReplies    = step.QuickReplies,
            IsCompleted     = false,
            FlowState       = state
        };
    }

    private static void RecordCompletedStep(
        UserFlowState state,
        FlowStep step,
        string input,
        string? nextStepId,
        DateTime enteredAt)
    {
        lock (state.HistorySyncRoot)
        {
            state.History.Add(new FlowStepHistory
            {
                StepId      = step.StepId,
                EnteredAt   = enteredAt,
                CompletedAt = state.LastActivityAt,
                UserInput   = input,
                NextStepId  = nextStepId
            });
        }
    }

    private static FlowStepResult BuildCompletedResult(UserFlowState state, string? completionMenuId)
        => new()
        {
            IsValid          = true,
            Prompt           = ConversationFlowEngineConstants.FlowCompletedPrompt,
            IsCompleted      = true,
            FlowState        = state,
            CompletionMenuId = completionMenuId
        };

    private Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken)
        => ExecuteStateStoreOperationAsync(
            store => store.SaveStateAsync(state, cancellationToken),
            "save",
            state);

    private async Task ExecuteStateStoreOperationAsync(
        Func<IConversationStateStore, Task> operation,
        string operationName,
        UserFlowState state)
    {
        if (_stateStore is null)
            return;

        try
        {
            await operation(_stateStore).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to {Operation} conversation state — UserId: {UserId}, FlowId: {FlowId}",
                operationName,
                state.UserId,
                state.FlowId);
            throw;
        }
    }

    private static FlowStepResult BuildTerminalResult(UserFlowState state, string message)
        => new()
        {
            IsValid     = false,
            IsCompleted = true,
            Prompt      = message,
            FlowState   = state
        };

    private static (bool isValid, string? error) ValidateInput(FlowStep step, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (false, step.Validation?.ErrorMessage ?? "Input cannot be empty.");

        var v = step.Validation;

        // Type-level checks first
        switch (step.InputType)
        {
            case FlowInputType.Number:
                if (!double.TryParse(input, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var numVal))
                    return (false, v?.ErrorMessage ?? "Please enter a valid number.");

                if (v?.MinValue.HasValue == true && numVal < v.MinValue.Value)
                    return (false, v.ErrorMessage ?? $"Value must be at least {v.MinValue}.");

                if (v?.MaxValue.HasValue == true && numVal > v.MaxValue.Value)
                    return (false, v.ErrorMessage ?? $"Value must be at most {v.MaxValue}.");
                break;

            case FlowInputType.Boolean:
                var lower = input.ToLowerInvariant();
                if (!new[] { "yes", "no", "true", "false", "1", "0" }.Contains(lower))
                    return (false, v?.ErrorMessage ?? "Please reply with yes or no.");
                break;

            case FlowInputType.Email:
                if (!Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                    return (false, v?.ErrorMessage ?? "Please enter a valid email address.");
                break;

            case FlowInputType.PhoneNumber:
                if (!Regex.IsMatch(input, @"^\+?[\d\s\-\(\)]{7,20}$"))
                    return (false, v?.ErrorMessage ?? "Please enter a valid phone number.");
                break;

            case FlowInputType.DateTime:
                if (!DateTime.TryParse(input, out _))
                    return (false, v?.ErrorMessage ?? "Please enter a valid date and time.");
                break;
        }

        if (v  is null) return (true, null);

        // Text length constraints
        if (v.MinLength.HasValue && input.Length < v.MinLength.Value)
            return (false, v.ErrorMessage ?? $"Minimum length is {v.MinLength} characters.");

        if (v.MaxLength.HasValue && input.Length > v.MaxLength.Value)
            return (false, v.ErrorMessage ?? $"Maximum length is {v.MaxLength} characters.");

        // Allowed values (case-insensitive)
        if (v.AllowedValues is { Count: > 0 } &&
            !v.AllowedValues.Any(av => string.Equals(av, input, StringComparison.OrdinalIgnoreCase)))
            return (false, v.ErrorMessage ?? $"Please choose one of: {string.Join(", ", v.AllowedValues)}.");

        // Regex pattern
        if (!string.IsNullOrEmpty(v.Pattern) && !Regex.IsMatch(input, v.Pattern))
            return (false, v.ErrorMessage ?? "Input does not match the expected format.");

        return (true, null);
    }

    private static string? ResolveNextStep(FlowStep step, Dictionary<string, string> variables)
    {
        foreach (var transition in step.Transitions)
        {
            if (transition.Condition  is null || EvaluateCondition(transition.Condition, variables))
                return transition.TargetStepId;
        }

        return step.DefaultNextStepId;
    }

    private static bool EvaluateCondition(FlowCondition condition, Dictionary<string, string> variables)
    {
        var exists  = variables.TryGetValue(condition.VariableName, out var raw);
        var current = raw ?? string.Empty;

        return condition.Operator switch
        {
            FlowConditionOperator.Equals     => string.Equals(current, condition.Value, StringComparison.OrdinalIgnoreCase),
            FlowConditionOperator.NotEquals  => !string.Equals(current, condition.Value, StringComparison.OrdinalIgnoreCase),
            FlowConditionOperator.Contains   => current.Contains(condition.Value, StringComparison.OrdinalIgnoreCase),
            FlowConditionOperator.StartsWith => current.StartsWith(condition.Value, StringComparison.OrdinalIgnoreCase),
            FlowConditionOperator.EndsWith   => current.EndsWith(condition.Value, StringComparison.OrdinalIgnoreCase),

            FlowConditionOperator.GreaterThan =>
                double.TryParse(current, out var cv)  &&
                double.TryParse(condition.Value, out var tv) && cv > tv,

            FlowConditionOperator.LessThan =>
                double.TryParse(current, out var cv2) &&
                double.TryParse(condition.Value, out var tv2) && cv2 < tv2,

            FlowConditionOperator.IsEmpty    => !exists || string.IsNullOrWhiteSpace(current),
            FlowConditionOperator.IsNotEmpty => exists  && !string.IsNullOrWhiteSpace(current),

            _ => false
        };
    }

    private void AppendHistory(long userId, UserFlowState state)
    {
        lock (_historyLock)
        {
            if (!_history.TryGetValue(userId, out var list))
            {
                list = [];
                _history[userId] = list;
            }

            list.Add(state);

            while (list.Count > _options.MaxHistoryPerUser)
                list.RemoveAt(0);
        }
    }

    // -------------------------------------------------------------------------
    // Session context key constants
    // -------------------------------------------------------------------------

    private FlowDefinition EnsureFlowExists(string flowId)
    {
        if (!_flows.TryGetValue(flowId, out var flow))
            throw new InvalidOperationException($"Flow '{flowId}' is not registered.");

        return flow;
    }

    private async Task HandleExistingActiveFlowAsync(long userId, string flowId, CancellationToken cancellationToken)
    {
        if (_activeStates.ContainsKey(userId))
        {
            _logger.LogDebug(
                ConversationFlowEngineConstants.AbortingExistingFlowLog, userId, flowId);
            await AbortFlowAsync(userId, ConversationFlowEngineConstants.SupersededByNewFlowReason, cancellationToken).ConfigureAwait(false);
        }
    }

    private void ActivateState(UserFlowState state)
    {
        _activeStates[state.UserId] = state;
        AppendHistory(state.UserId, state);
    }

    private async Task PersistAndPublishStateAsync(UserFlowState state, CancellationToken cancellationToken)
    {
        await SaveStateAsync(state, cancellationToken).ConfigureAwait(false);
        await MirrorFlowContextToSessionAsync(state, cancellationToken).ConfigureAwait(false);
        await PublishFlowStartedAsync(state).ConfigureAwait(false);

        _logger.LogInformation(
            ConversationFlowEngineConstants.FlowStartedLog,
            state.UserId, state.FlowId, state.StateId);
    }

    private static class SessionKeys
    {
        internal const string FlowId      = "flow_id";
        internal const string FlowStateId = "flow_state_id";
    }
}

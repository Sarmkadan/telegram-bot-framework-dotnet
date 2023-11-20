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

    private readonly object _historyLock = new();

    /// <summary>
    /// Initialises a new instance of <see cref="ConversationFlowEngine"/>.
    /// </summary>
    public ConversationFlowEngine(
        ConversationFlowOptions options,
        ISessionService sessionService,
        IEventBus eventBus,
        ILogger<ConversationFlowEngine> logger)
    {
        _options        = options        ?? throw new ArgumentNullException(nameof(options));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _eventBus       = eventBus       ?? throw new ArgumentNullException(nameof(eventBus));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    // -------------------------------------------------------------------------
    // Registration
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Task RegisterFlowAsync(FlowDefinition flow, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(flow);

        if (string.IsNullOrWhiteSpace(flow.FlowId))
            throw new ArgumentException("FlowId must not be empty.", nameof(flow));

        if (!flow.Steps.Any(s => s.StepId == flow.InitialStepId))
            throw new InvalidOperationException(
                $"Flow '{flow.FlowId}' references InitialStepId '{flow.InitialStepId}' that does not exist in Steps.");

        _flows[flow.FlowId] = flow;

        _logger.LogInformation("Flow registered — Id: {FlowId}, Name: {FlowName}, Steps: {StepCount}",
            flow.FlowId, flow.Name, flow.Steps.Count);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UnregisterFlowAsync(string flowId, CancellationToken cancellationToken = default)
    {
        _flows.TryRemove(flowId, out _);
        _logger.LogInformation("Flow unregistered — Id: {FlowId}", flowId);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<FlowDefinition?> GetFlowAsync(string flowId, CancellationToken cancellationToken = default)
        => Task.FromResult(_flows.TryGetValue(flowId, out var flow) ? flow : (FlowDefinition?)null);

    /// <inheritdoc/>
    public Task<IReadOnlyList<FlowDefinition>> GetAllFlowsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FlowDefinition>>(_flows.Values.ToList());

    // -------------------------------------------------------------------------
    // Execution — Start
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<UserFlowState> StartFlowAsync(
        long userId,
        long chatId,
        string flowId,
        Dictionary<string, string>? initialVariables = null,
        CancellationToken cancellationToken = default)
    {
        if (!_flows.TryGetValue(flowId, out var flow))
            throw new InvalidOperationException($"Flow '{flowId}' is not registered.");

        if (_activeStates.ContainsKey(userId))
        {
            _logger.LogDebug(
                "Aborting existing flow for UserId {UserId} before starting '{FlowId}'", userId, flowId);
            await AbortFlowAsync(userId, "Superseded by a new flow", cancellationToken);
        }

        var state = new UserFlowState
        {
            StateId        = Guid.NewGuid().ToString("N"),
            FlowId         = flowId,
            UserId         = userId,
            ChatId         = chatId,
            CurrentStepId  = flow.InitialStepId,
            Status         = FlowStateStatus.WaitingForInput,
            StartedAt      = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        if (initialVariables is { Count: > 0 })
        {
            foreach (var (key, value) in initialVariables)
                state.Variables[key] = value;
        }

        _activeStates[userId] = state;
        AppendHistory(userId, state);

        // Mirror flow context into the session layer so the middleware can detect active flows.
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        if (session != null)
        {
            await _sessionService.UpdateSessionContextAsync(
                session.SessionId, SessionKeys.FlowId, flowId, cancellationToken);
            await _sessionService.UpdateSessionContextAsync(
                session.SessionId, SessionKeys.FlowStateId, state.StateId, cancellationToken);
        }

        if (_options.EnableFlowEvents)
            await _eventBus.PublishAsync(new FlowStartedEvent(userId, chatId, flowId, state.StateId));

        _logger.LogInformation(
            "Flow started — UserId: {UserId}, FlowId: {FlowId}, StateId: {StateId}",
            userId, flowId, state.StateId);

        return state;
    }

    // -------------------------------------------------------------------------
    // Execution — Input Processing
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<FlowStepResult> ProcessInputAsync(
        long userId,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_activeStates.TryGetValue(userId, out var state))
            throw new InvalidOperationException(
                $"User {userId} has no active conversation flow. Call StartFlowAsync first.");

        if (!_flows.TryGetValue(state.FlowId, out var flow))
            throw new InvalidOperationException(
                $"Flow definition '{state.FlowId}' is no longer registered.");

        var step = flow.Steps.FirstOrDefault(s => s.StepId == state.CurrentStepId)
            ?? throw new InvalidOperationException(
                $"Step '{state.CurrentStepId}' not found in flow '{state.FlowId}'.");

        // --- Abort keyword shortcut ---
        if (!string.IsNullOrEmpty(_options.AbortKeyword) &&
            string.Equals(input.Trim(), _options.AbortKeyword, StringComparison.OrdinalIgnoreCase))
        {
            await AbortFlowAsync(userId, "User triggered abort keyword", cancellationToken);
            return BuildTerminalResult(state, _options.AbortAcknowledgementMessage);
        }

        // --- Inactivity timeout check ---
        var effectiveTimeout = flow.Timeout ?? _options.DefaultFlowTimeout;
        if (DateTime.UtcNow - state.LastActivityAt > effectiveTimeout)
        {
            await TerminateAsync(state, FlowStateStatus.TimedOut, "Inactivity timeout");
            return BuildTerminalResult(state, _options.FlowTimeoutMessage);
        }

        // --- Validate input ---
        var (isValid, validationError) = ValidateInput(step, input);
        if (!isValid)
        {
            _logger.LogDebug(
                "Validation failed — UserId: {UserId}, Step: {StepId}, Error: {Error}",
                userId, step.StepId, validationError);

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

        // --- Store variable ---
        if (!string.IsNullOrWhiteSpace(step.VariableName))
            state.Variables[step.VariableName] = input;

        var stepEnteredAt   = state.LastActivityAt;
        state.LastActivityAt = DateTime.UtcNow;

        // --- Resolve next step ---
        var nextStepId = ResolveNextStep(step, state.Variables);

        // --- Record history ---
        state.History.Add(new FlowStepHistory
        {
            StepId      = step.StepId,
            EnteredAt   = stepEnteredAt,
            CompletedAt = state.LastActivityAt,
            UserInput   = input,
            NextStepId  = nextStepId
        });

        if (_options.EnableFlowEvents)
            await _eventBus.PublishAsync(
                new FlowStepCompletedEvent(userId, state.FlowId, step.StepId, nextStepId));

        // --- Terminal step or no outgoing path ---
        if (step.IsTerminal || nextStepId == null)
        {
            await TerminateAsync(state, FlowStateStatus.Completed, null);

            if (_options.EnableFlowEvents)
                await _eventBus.PublishAsync(
                    new FlowCompletedEvent(userId, state.ChatId, state.FlowId, state.StateId));

            _logger.LogInformation(
                "Flow completed — UserId: {UserId}, FlowId: {FlowId}, Steps: {StepCount}",
                userId, state.FlowId, state.History.Count);

            return new FlowStepResult
            {
                IsValid          = true,
                Prompt           = "Completed.",
                IsCompleted      = true,
                FlowState        = state,
                CompletionMenuId = flow.CompletionMenuId
            };
        }

        // --- Advance to next step ---
        state.CurrentStepId = nextStepId;
        state.Status        = FlowStateStatus.WaitingForInput;

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

    /// <inheritdoc/>
    public Task<UserFlowState?> GetActiveFlowStateAsync(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_activeStates.TryGetValue(userId, out var s) ? s : (UserFlowState?)null);

    /// <inheritdoc/>
    public async Task AbortFlowAsync(long userId, string reason, CancellationToken cancellationToken = default)
    {
        if (!_activeStates.TryGetValue(userId, out var state))
            return;

        await TerminateAsync(state, FlowStateStatus.Aborted, reason);

        if (_options.EnableFlowEvents)
            await _eventBus.PublishAsync(new FlowAbortedEvent(userId, state.FlowId, reason));

        _logger.LogInformation(
            "Flow aborted — UserId: {UserId}, FlowId: {FlowId}, Reason: {Reason}",
            userId, state.FlowId, reason);
    }

    /// <inheritdoc/>
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
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        if (session == null) return null;

        var restoredFlowId = await _sessionService.GetSessionContextAsync(
            session.SessionId, SessionKeys.FlowId, cancellationToken);

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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Task<bool> IsUserInFlowAsync(long userId, CancellationToken cancellationToken = default)
    {
        var active = _activeStates.TryGetValue(userId, out var state) &&
                     state.Status is FlowStateStatus.Active or FlowStateStatus.WaitingForInput;
        return Task.FromResult(active);
    }

    /// <inheritdoc/>
    public Task<int> CleanupExpiredFlowStatesAsync(CancellationToken cancellationToken = default)
    {
        var cleaned = 0;

        foreach (var (userId, state) in _activeStates)
        {
            if (!_flows.TryGetValue(state.FlowId, out var flow)) continue;

            var timeout = flow.Timeout ?? _options.DefaultFlowTimeout;
            if (DateTime.UtcNow - state.LastActivityAt <= timeout) continue;

            state.Status       = FlowStateStatus.TimedOut;
            state.CompletedAt  = DateTime.UtcNow;
            state.AbortReason  = "Inactivity timeout (cleanup sweep)";
            _activeStates.TryRemove(userId, out _);
            cleaned++;
        }

        if (cleaned > 0)
            _logger.LogInformation("Cleanup removed {Count} expired flow states", cleaned);

        return Task.FromResult(cleaned);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private Task TerminateAsync(UserFlowState state, FlowStateStatus status, string? reason)
    {
        state.Status      = status;
        state.CompletedAt = DateTime.UtcNow;
        state.AbortReason = reason;
        _activeStates.TryRemove(state.UserId, out _);
        return Task.CompletedTask;
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

        if (v == null) return (true, null);

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
            if (transition.Condition == null || EvaluateCondition(transition.Condition, variables))
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

    private static class SessionKeys
    {
        internal const string FlowId      = "flow_id";
        internal const string FlowStateId = "flow_state_id";
    }
}

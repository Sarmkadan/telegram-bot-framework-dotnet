// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

// ---------------------------------------------------------------------------
// Flow Definition
// ---------------------------------------------------------------------------

/// <summary>
/// Immutable blueprint of a conversation flow containing all steps, transitions, and
/// branching rules. Register instances with <see cref="IConversationFlowEngine"/> before use.
/// </summary>
public sealed record FlowDefinition
{
    /// <summary>Gets the unique identifier used to look up and start this flow.</summary>
    public required string FlowId { get; init; }

    /// <summary>Gets the human-readable display name of the flow.</summary>
    public required string Name { get; init; }

    /// <summary>Gets an optional description of the flow's purpose.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the <see cref="FlowStep.StepId"/> where execution begins.</summary>
    public required string InitialStepId { get; init; }

    /// <summary>Gets the ordered collection of steps that compose the flow.</summary>
    public required IReadOnlyList<FlowStep> Steps { get; init; }

    /// <summary>
    /// Gets the inactivity period after which the engine automatically times out the flow.
    /// When <c>null</c>, <see cref="ConversationFlowOptions.DefaultFlowTimeout"/> applies.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets a value indicating whether a user can resume this flow after an interruption
    /// or session restart.
    /// </summary>
    public bool AllowResume { get; init; } = true;

    /// <summary>Gets the menu identifier the orchestrator navigates to after successful completion.</summary>
    public string? CompletionMenuId { get; init; }

    /// <summary>Gets arbitrary key-value metadata attached to the flow definition.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

// ---------------------------------------------------------------------------
// Flow Step
// ---------------------------------------------------------------------------

/// <summary>
/// Represents a single step within a <see cref="FlowDefinition"/>. Each step presents a
/// prompt, optionally validates input, stores the result in a named variable, and
/// evaluates outgoing transitions to determine the next step.
/// </summary>
public sealed record FlowStep
{
    /// <summary>Gets the unique identifier for this step within the parent flow.</summary>
    public required string StepId { get; init; }

    /// <summary>Gets the message text sent to the user when this step is entered.</summary>
    public required string Prompt { get; init; }

    /// <summary>Gets optional contextual help appended to the prompt when validation fails.</summary>
    public string? HelpText { get; init; }

    /// <summary>
    /// Gets a value indicating whether reaching this step completes the flow successfully.
    /// No further input or transitions are processed.
    /// </summary>
    public bool IsTerminal { get; init; }

    /// <summary>Gets the semantic category of input expected from the user at this step.</summary>
    public required FlowInputType InputType { get; init; }

    /// <summary>Gets optional validation constraints applied before a transition is evaluated.</summary>
    public FlowValidation? Validation { get; init; }

    /// <summary>
    /// Gets the key under which validated input is stored in <see cref="UserFlowState.Variables"/>.
    /// Transitions and later steps can reference this key via <see cref="FlowCondition.VariableName"/>.
    /// </summary>
    public string? VariableName { get; init; }

    /// <summary>
    /// Gets the ordered list of conditional transitions evaluated left-to-right against
    /// collected variables. The first matching condition wins.
    /// </summary>
    public IReadOnlyList<FlowTransition> Transitions { get; init; } = [];

    /// <summary>Gets suggested quick-reply options surfaced to the user as buttons or hints.</summary>
    public IReadOnlyList<string>? QuickReplies { get; init; }

    /// <summary>Gets the fallback step identifier used when no conditional transition matches.</summary>
    public string? DefaultNextStepId { get; init; }

    /// <summary>Gets arbitrary key-value metadata attached to this step.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

// ---------------------------------------------------------------------------
// Input Type
// ---------------------------------------------------------------------------

/// <summary>
/// Specifies the semantic category of user input expected at a <see cref="FlowStep"/>.
/// The engine uses this to apply type-specific validation before running custom rules.
/// </summary>
public enum FlowInputType
{
    /// <summary>Free-form text. No type-level validation beyond non-empty.</summary>
    Text,

    /// <summary>Numeric value (integer or decimal). Validated against min/max constraints.</summary>
    Number,

    /// <summary>Yes/No or true/false choice. Accepts: yes, no, true, false, 1, 0.</summary>
    Boolean,

    /// <summary>Selection from a pre-defined list of <see cref="FlowValidation.AllowedValues"/>.</summary>
    Choice,

    /// <summary>Date and/or time value in any parseable format.</summary>
    DateTime,

    /// <summary>Formatted international phone number.</summary>
    PhoneNumber,

    /// <summary>RFC 5322 compliant e-mail address.</summary>
    Email,

    /// <summary>User presses a button or sends any non-empty message to confirm.</summary>
    Confirmation,

    /// <summary>Accepts any non-empty input without semantic type validation.</summary>
    Any
}

// ---------------------------------------------------------------------------
// Validation
// ---------------------------------------------------------------------------

/// <summary>
/// Defines validation constraints applied to user input at a <see cref="FlowStep"/> before
/// transitions are evaluated. All constraints are optional and evaluated in combination.
/// </summary>
public sealed record FlowValidation
{
    /// <summary>Gets a regular-expression pattern the entire input must match.</summary>
    public string? Pattern { get; init; }

    /// <summary>Gets the minimum character length required for text or choice input.</summary>
    public int? MinLength { get; init; }

    /// <summary>Gets the maximum character length allowed for text or choice input.</summary>
    public int? MaxLength { get; init; }

    /// <summary>Gets the minimum numeric value (inclusive) for <see cref="FlowInputType.Number"/> steps.</summary>
    public double? MinValue { get; init; }

    /// <summary>Gets the maximum numeric value (inclusive) for <see cref="FlowInputType.Number"/> steps.</summary>
    public double? MaxValue { get; init; }

    /// <summary>
    /// Gets the user-facing error message displayed when any validation rule fails.
    /// When <c>null</c>, the engine supplies a default message for each rule.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the exhaustive list of acceptable values for <see cref="FlowInputType.Choice"/> steps.
    /// Comparison is case-insensitive.
    /// </summary>
    public IReadOnlyList<string>? AllowedValues { get; init; }
}

// ---------------------------------------------------------------------------
// Transitions &amp; Conditions
// ---------------------------------------------------------------------------

/// <summary>
/// Describes a conditional path from the current step to a target step, evaluated after
/// input has passed validation. A <c>null</c> <see cref="Condition"/> acts as a default
/// (unconditional) path; evaluated only after all conditional transitions have been skipped.
/// </summary>
public sealed record FlowTransition
{
    /// <summary>Gets the <see cref="FlowStep.StepId"/> to transition to when the condition is met.</summary>
    public required string TargetStepId { get; init; }

    /// <summary>
    /// Gets the predicate evaluated against <see cref="UserFlowState.Variables"/>.
    /// <c>null</c> means this transition always fires (use as the last/default entry).
    /// </summary>
    public FlowCondition? Condition { get; init; }

    /// <summary>Gets an optional human-readable label describing this path, useful for debugging.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// A boolean predicate evaluated against a named variable in <see cref="UserFlowState.Variables"/>.
/// </summary>
public sealed record FlowCondition
{
    /// <summary>Gets the key in <see cref="UserFlowState.Variables"/> to read the left-hand value from.</summary>
    public required string VariableName { get; init; }

    /// <summary>Gets the comparison operator applied between the stored variable and <see cref="Value"/>.</summary>
    public required FlowConditionOperator Operator { get; init; }

    /// <summary>Gets the right-hand value used in the comparison.</summary>
    public required string Value { get; init; }
}

/// <summary>
/// Comparison operators supported by <see cref="FlowCondition"/>.
/// </summary>
public enum FlowConditionOperator
{
    /// <summary>Stored value equals <c>Value</c> (case-insensitive string comparison).</summary>
    Equals,

    /// <summary>Stored value does not equal <c>Value</c>.</summary>
    NotEquals,

    /// <summary>Stored value contains <c>Value</c> as a substring (case-insensitive).</summary>
    Contains,

    /// <summary>Stored value starts with <c>Value</c> (case-insensitive).</summary>
    StartsWith,

    /// <summary>Stored value ends with <c>Value</c> (case-insensitive).</summary>
    EndsWith,

    /// <summary>Stored numeric value is strictly greater than the numeric <c>Value</c>.</summary>
    GreaterThan,

    /// <summary>Stored numeric value is strictly less than the numeric <c>Value</c>.</summary>
    LessThan,

    /// <summary>Variable is absent, null, or consists only of whitespace.</summary>
    IsEmpty,

    /// <summary>Variable is present and contains at least one non-whitespace character.</summary>
    IsNotEmpty
}

// ---------------------------------------------------------------------------
// Runtime State
// ---------------------------------------------------------------------------

/// <summary>
/// Mutable runtime record tracking a single user's progress through a <see cref="FlowDefinition"/>.
/// One active state per user is maintained by <see cref="IConversationFlowEngine"/>.
/// </summary>
public sealed class UserFlowState
{
    /// <summary>Gets the globally-unique identifier for this execution record.</summary>
    public required string StateId { get; init; }

    /// <summary>Gets or sets the identifier of the flow being executed.</summary>
    public required string FlowId { get; set; }

    /// <summary>Gets the Telegram user identifier for whom this state belongs.</summary>
    public required long UserId { get; init; }

    /// <summary>Gets the Telegram chat identifier where the flow is running.</summary>
    public required long ChatId { get; init; }

    /// <summary>Gets or sets the <see cref="FlowStep.StepId"/> currently awaiting user input.</summary>
    public required string CurrentStepId { get; set; }

    /// <summary>Gets or sets the current lifecycle status of this flow execution.</summary>
    public FlowStateStatus Status { get; set; } = FlowStateStatus.Active;

    /// <summary>Gets the UTC timestamp when this flow was initiated.</summary>
    public required DateTime StartedAt { get; init; }

    /// <summary>Gets or sets the UTC timestamp of the most recent user interaction.</summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the flow reached a terminal state.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets the dictionary of variables accumulated during the flow. Keys are
    /// <see cref="FlowStep.VariableName"/> values; entries are written at step completion.
    /// </summary>
    public Dictionary<string, string> Variables { get; init; } = new();

    /// <summary>Gets the ordered chronological record of steps visited during this execution.</summary>
    public List<FlowStepHistory> History { get; init; } = [];

    /// <summary>
    /// Gets or sets a human-readable reason when <see cref="Status"/> is
    /// <see cref="FlowStateStatus.Aborted"/> or <see cref="FlowStateStatus.TimedOut"/>.
    /// </summary>
    public string? AbortReason { get; set; }
}

/// <summary>
/// Lifecycle status values for a <see cref="UserFlowState"/>.
/// </summary>
public enum FlowStateStatus
{
    /// <summary>Flow is running and awaiting user input at the current step.</summary>
    Active,

    /// <summary>Engine has presented the current step prompt and is holding for input.</summary>
    WaitingForInput,

    /// <summary>Flow has been paused (e.g., preempted by admin action) and can be resumed.</summary>
    Suspended,

    /// <summary>Flow reached a terminal step and all steps completed successfully.</summary>
    Completed,

    /// <summary>Flow was explicitly cancelled by the user or the system.</summary>
    Aborted,

    /// <summary>Flow was automatically terminated because the inactivity timeout elapsed.</summary>
    TimedOut
}

// ---------------------------------------------------------------------------
// Step History
// ---------------------------------------------------------------------------

/// <summary>
/// Immutable record of a user's visit to a specific step within a flow execution.
/// </summary>
public sealed record FlowStepHistory
{
    /// <summary>Gets the <see cref="FlowStep.StepId"/> that was visited.</summary>
    public required string StepId { get; init; }

    /// <summary>Gets the UTC timestamp when the step was entered.</summary>
    public required DateTime EnteredAt { get; init; }

    /// <summary>Gets the UTC timestamp when the step was completed (input accepted), or <c>null</c> if still active.</summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>Gets the raw user input that was accepted at this step, or <c>null</c> for terminal steps.</summary>
    public string? UserInput { get; init; }

    /// <summary>Gets the <see cref="FlowStep.StepId"/> transitioned to after completing this step.</summary>
    public string? NextStepId { get; init; }
}

// ---------------------------------------------------------------------------
// Step Processing Result
// ---------------------------------------------------------------------------

/// <summary>
/// Carries the outcome of a single <see cref="IConversationFlowEngine.ProcessInputAsync"/> call.
/// Inspect <see cref="IsValid"/> to distinguish validation rejection from a successful advance,
/// and <see cref="IsCompleted"/> to detect flow termination.
/// </summary>
public sealed record FlowStepResult
{
    /// <summary>
    /// Gets a value indicating whether the submitted input passed all validation rules.
    /// When <c>false</c>, <see cref="ValidationError"/> contains the failure reason and
    /// the current step is repeated with the original prompt.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>Gets the validation failure message when <see cref="IsValid"/> is <c>false</c>.</summary>
    public string? ValidationError { get; init; }

    /// <summary>
    /// Gets the prompt text to display to the user. Represents either the next step's prompt
    /// (on success) or the repeated current-step prompt with appended error (on validation failure).
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>Gets the quick-reply suggestions for the next (or repeated) step.</summary>
    public IReadOnlyList<string>? QuickReplies { get; init; }

    /// <summary>
    /// Gets a value indicating whether the flow has reached a terminal state (completed,
    /// aborted, or timed out). No further calls to <see cref="IConversationFlowEngine.ProcessInputAsync"/>
    /// should be made for this user until a new flow is started.
    /// </summary>
    public required bool IsCompleted { get; init; }

    /// <summary>Gets the updated <see cref="UserFlowState"/> after processing the input.</summary>
    public required UserFlowState FlowState { get; init; }

    /// <summary>
    /// Gets the menu identifier to navigate to after flow completion, sourced from
    /// <see cref="FlowDefinition.CompletionMenuId"/>. Only populated when <see cref="IsCompleted"/> is <c>true</c>.
    /// </summary>
    public string? CompletionMenuId { get; init; }
}

// ---------------------------------------------------------------------------
// Flow Lifecycle Events
// ---------------------------------------------------------------------------

/// <summary>
/// Published to <see cref="Events.IEventBus"/> immediately after a new flow execution is started.
/// </summary>
public sealed class FlowStartedEvent : Events.EventBase
{
    /// <summary>Gets the Telegram user identifier who started the flow.</summary>
    public long UserId { get; }

    /// <summary>Gets the Telegram chat identifier where the flow is running.</summary>
    public long ChatId { get; }

    /// <summary>Gets the identifier of the flow definition that was started.</summary>
    public string FlowId { get; }

    /// <summary>Gets the unique identifier of the new flow state record.</summary>
    public string StateId { get; }

    /// <summary>Initializes a new <see cref="FlowStartedEvent"/>.</summary>
    public FlowStartedEvent(long userId, long chatId, string flowId, string stateId)
    {
        UserId  = userId;
        ChatId  = chatId;
        FlowId  = flowId;
        StateId = stateId;
    }
}

/// <summary>
/// Published after a user successfully completes a flow step and the engine transitions to the next step.
/// </summary>
public sealed class FlowStepCompletedEvent : Events.EventBase
{
    /// <summary>Gets the Telegram user identifier.</summary>
    public long UserId { get; }

    /// <summary>Gets the identifier of the flow being executed.</summary>
    public string FlowId { get; }

    /// <summary>Gets the identifier of the step that was just completed.</summary>
    public string CompletedStepId { get; }

    /// <summary>Gets the identifier of the next step, or <c>null</c> if the flow is now complete.</summary>
    public string? NextStepId { get; }

    /// <summary>Initializes a new <see cref="FlowStepCompletedEvent"/>.</summary>
    public FlowStepCompletedEvent(long userId, string flowId, string completedStepId, string? nextStepId)
    {
        UserId           = userId;
        FlowId           = flowId;
        CompletedStepId  = completedStepId;
        NextStepId       = nextStepId;
    }
}

/// <summary>
/// Published when a flow reaches a terminal step and completes successfully.
/// </summary>
public sealed class FlowCompletedEvent : Events.EventBase
{
    /// <summary>Gets the Telegram user identifier.</summary>
    public long UserId { get; }

    /// <summary>Gets the Telegram chat identifier.</summary>
    public long ChatId { get; }

    /// <summary>Gets the identifier of the completed flow.</summary>
    public string FlowId { get; }

    /// <summary>Gets the unique state record identifier of the completed execution.</summary>
    public string StateId { get; }

    /// <summary>Initializes a new <see cref="FlowCompletedEvent"/>.</summary>
    public FlowCompletedEvent(long userId, long chatId, string flowId, string stateId)
    {
        UserId  = userId;
        ChatId  = chatId;
        FlowId  = flowId;
        StateId = stateId;
    }
}

/// <summary>
/// Published when a flow is forcefully aborted — either by the user invoking the abort keyword
/// or by the system starting a new conflicting flow.
/// </summary>
public sealed class FlowAbortedEvent : Events.EventBase
{
    /// <summary>Gets the Telegram user identifier.</summary>
    public long UserId { get; }

    /// <summary>Gets the identifier of the aborted flow.</summary>
    public string FlowId { get; }

    /// <summary>Gets the human-readable reason the flow was aborted.</summary>
    public string Reason { get; }

    /// <summary>Initializes a new <see cref="FlowAbortedEvent"/>.</summary>
    public FlowAbortedEvent(long userId, string flowId, string reason)
    {
        UserId = userId;
        FlowId = flowId;
        Reason = reason;
    }
}

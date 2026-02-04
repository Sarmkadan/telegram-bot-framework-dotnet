// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Manages the full lifecycle of conversation flows: registration of flow blueprints,
/// per-user execution state, branching input processing, and history retrieval.
/// </summary>
/// <remarks>
/// Implement this interface to provide a custom storage or execution backend. The default
/// in-process implementation is <see cref="ConversationFlowEngine"/>.
/// </remarks>
public interface IConversationFlowEngine
{
    // -------------------------------------------------------------------------
    // Flow Registration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a <see cref="FlowDefinition"/>, making it available for execution.
    /// Registering a flow with an existing <see cref="FlowDefinition.FlowId"/> replaces the previous definition.
    /// </summary>
    /// <param name="flow">The flow blueprint to register.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task RegisterFlowAsync(FlowDefinition flow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a previously registered flow definition. Active user states for this flow are not affected.
    /// </summary>
    /// <param name="flowId">The identifier of the flow to remove.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task UnregisterFlowAsync(string flowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a registered flow definition by its identifier.
    /// Returns <c>null</c> if no flow with that identifier has been registered.
    /// </summary>
    /// <param name="flowId">The identifier of the flow to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<FlowDefinition?> GetFlowAsync(string flowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a snapshot of all currently registered flow definitions.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<FlowDefinition>> GetAllFlowsAsync(CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Flow Execution
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a new flow execution for the specified user, automatically aborting any
    /// existing active flow for that user before beginning.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="chatId">The Telegram chat identifier.</param>
    /// <param name="flowId">The identifier of the <see cref="FlowDefinition"/> to execute.</param>
    /// <param name="initialVariables">
    /// Optional seed variables injected into <see cref="UserFlowState.Variables"/> before the
    /// first step is entered. Useful for pre-populating known data (e.g., user name, order ID).
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>The newly created <see cref="UserFlowState"/> positioned at the initial step.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="flowId"/> is not registered.</exception>
    Task<UserFlowState> StartFlowAsync(
        long userId,
        long chatId,
        string flowId,
        Dictionary<string, string>? initialVariables = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes raw user input against the current step of the user's active flow. The method
    /// validates the input, stores accepted values in <see cref="UserFlowState.Variables"/>,
    /// evaluates outgoing transitions, and advances the flow or returns a validation error.
    /// </summary>
    /// <param name="userId">The Telegram user identifier whose active flow should receive the input.</param>
    /// <param name="input">The raw text submitted by the user.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// A <see cref="FlowStepResult"/> describing whether input was accepted, the next prompt to
    /// display, and whether the flow has reached a terminal state.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the user has no active flow.</exception>
    Task<FlowStepResult> ProcessInputAsync(
        long userId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the active (non-terminal) flow state for the specified user.
    /// Returns <c>null</c> when the user has no flow in progress.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<UserFlowState?> GetActiveFlowStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forcefully terminates the user's active flow, marking it as <see cref="FlowStateStatus.Aborted"/>
    /// and recording the provided reason. A <see cref="FlowAbortedEvent"/> is published when
    /// <see cref="ConversationFlowOptions.EnableFlowEvents"/> is <c>true</c>.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="reason">A human-readable explanation of why the flow was aborted.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task AbortFlowAsync(long userId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to resume a <see cref="FlowStateStatus.Suspended"/> flow for the specified user.
    /// Returns the restored <see cref="UserFlowState"/> on success, or <c>null</c> when no
    /// resumable state exists.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<UserFlowState?> ResumeFlowAsync(long userId, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Querying
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the most recent flow state records for the specified user, ordered by
    /// <see cref="UserFlowState.StartedAt"/> descending.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="limit">Maximum number of records to return. Defaults to 10.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<UserFlowState>> GetFlowHistoryAsync(
        long userId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if the user currently has a flow in
    /// <see cref="FlowStateStatus.Active"/> or <see cref="FlowStateStatus.WaitingForInput"/> status.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<bool> IsUserInFlowAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans all active flow states, marks timed-out ones as <see cref="FlowStateStatus.TimedOut"/>,
    /// removes them from the active set, and returns the count of states cleaned up.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<int> CleanupExpiredFlowStatesAsync(CancellationToken cancellationToken = default);
}

// ---------------------------------------------------------------------------
// Builder Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Provides a fluent API for constructing <see cref="FlowDefinition"/> instances without
/// manually initialising collection properties. Obtain an instance via
/// <see cref="ConversationFlowExtensions.CreateFlow"/>.
/// </summary>
public interface IFlowDefinitionBuilder
{
    /// <summary>Sets a human-readable description for the flow.</summary>
    /// <param name="description">The description text.</param>
    IFlowDefinitionBuilder WithDescription(string description);

    /// <summary>
    /// Overrides the default inactivity timeout for this specific flow.
    /// </summary>
    /// <param name="timeout">Duration of allowed inactivity before the flow is timed out.</param>
    IFlowDefinitionBuilder WithTimeout(TimeSpan timeout);

    /// <summary>
    /// Specifies the menu to navigate to after the flow completes successfully.
    /// </summary>
    /// <param name="menuId">The menu identifier passed to the bot orchestrator on completion.</param>
    IFlowDefinitionBuilder OnCompletionNavigateTo(string menuId);

    /// <summary>
    /// Controls whether users can resume this flow after a session restart or interruption.
    /// </summary>
    /// <param name="allow"><c>true</c> to allow resume; <c>false</c> to require a fresh start.</param>
    IFlowDefinitionBuilder AllowResume(bool allow = true);

    /// <summary>
    /// Appends a step to the flow definition. Steps are evaluated in registration order
    /// when resolving the initial step.
    /// </summary>
    /// <param name="step">The <see cref="FlowStep"/> to add.</param>
    IFlowDefinitionBuilder AddStep(FlowStep step);

    /// <summary>Attaches an arbitrary metadata key-value pair to the flow.</summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    IFlowDefinitionBuilder WithMetadata(string key, string value);

    /// <summary>
    /// Builds and returns the immutable <see cref="FlowDefinition"/> from the accumulated configuration.
    /// </summary>
    /// <returns>The constructed <see cref="FlowDefinition"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required properties (e.g., <see cref="FlowDefinition.InitialStepId"/>) are not set
    /// or when no steps have been added.
    /// </exception>
    FlowDefinition Build();
}

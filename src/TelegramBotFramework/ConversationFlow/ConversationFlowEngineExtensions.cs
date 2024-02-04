#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides extension methods for <see cref="IConversationFlowEngine"/> to simplify common
/// conversation flow operations and reduce boilerplate code when working with flows.
/// </summary>
/// <remarks>
/// All extension methods validate their parameters and throw appropriate exceptions for null inputs.
/// This class cannot be inherited.
/// </remarks>
public static class ConversationFlowEngineExtensions
{
    /// <summary>
    /// Determines whether the specified user has an active flow with the given flow identifier.
    /// </summary>
    /// <param name="engine">The conversation flow engine. Cannot be <c>null</c>.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="flowId">The flow identifier to check against. Cannot be <c>null</c> or empty.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <c>true</c> if the user has an active flow with the specified flow identifier;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="flowId"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="flowId"/> is empty or whitespace.</exception>
    public static async Task<bool> HasActiveFlowAsync(
        this IConversationFlowEngine engine,
        long userId,
        string flowId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(flowId);

        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state?.FlowId == flowId && state.Status is FlowStateStatus.Active or FlowStateStatus.WaitingForInput;
    }

    /// <summary>
    /// Gets the current step identifier for the user's active flow.
    /// </summary>
    /// <param name="engine">The conversation flow engine. Cannot be <c>null</c>.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The current step identifier if the user has an active flow;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is <c>null</c>.</exception>
    public static async Task<string?> GetCurrentStepIdAsync(
        this IConversationFlowEngine engine,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state?.CurrentStepId;
    }

    /// <summary>
    /// Gets the value of a specific variable from the user's active flow state.
    /// </summary>
    /// <param name="engine">The conversation flow engine. Cannot be <c>null</c>.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="variableName">The name of the variable to retrieve. Cannot be <c>null</c> or empty.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The variable value if it exists and the user has an active flow;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="variableName"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="variableName"/> is empty or whitespace.</exception>
    public static async Task<string?> GetVariableAsync(
        this IConversationFlowEngine engine,
        long userId,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(variableName);

        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state?.Variables.TryGetValue(variableName, out var value) == true ? value : null;
    }

    /// <summary>
    /// Gets the flow definition associated with the user's active flow.
    /// </summary>
    /// <param name="engine">The conversation flow engine. Cannot be <c>null</c>.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The flow definition if the user has an active flow;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is <c>null</c>.</exception>
    public static async Task<FlowDefinition?> GetActiveFlowAsync(
        this IConversationFlowEngine engine,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state == null ? null : await engine.GetFlowAsync(state.FlowId, cancellationToken).ConfigureAwait(false);
    }
}
#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides useful extension methods for <see cref="ConversationFlowEngine"/> to simplify common
/// conversation flow operations and reduce boilerplate code when working with the engine.
/// </summary>
public static class ConversationFlowEngineExtensions
{
    /// <summary>
    /// Determines whether the specified user has an active flow with the given flow identifier.
    /// </summary>
    /// <param name="engine">The conversation flow engine.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="flowId">The flow identifier to check against.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <c>true</c> if the user has an active flow with the specified flow identifier;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static async Task<bool> HasActiveFlowAsync(
        this ConversationFlowEngine engine,
        long userId,
        string flowId,
        CancellationToken cancellationToken = default)
    {
        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state?.FlowId == flowId && state.Status is FlowStateStatus.Active or FlowStateStatus.WaitingForInput;
    }

    /// <summary>
    /// Gets the current step identifier for the user's active flow.
    /// </summary>
    /// <param name="engine">The conversation flow engine.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The current step identifier if the user has an active flow;
    /// otherwise, <c>null</c>.
    /// </returns>
    public static async Task<string?> GetCurrentStepIdAsync(
        this ConversationFlowEngine engine,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state?.CurrentStepId;
    }

    /// <summary>
    /// Gets the value of a specific variable from the user's active flow state.
    /// </summary>
    /// <param name="engine">The conversation flow engine.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="variableName">The name of the variable to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The variable value if it exists and the user has an active flow;
    /// otherwise, <c>null</c>.
    /// </returns>
    public static async Task<string?> GetVariableAsync(
        this ConversationFlowEngine engine,
        long userId,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state?.Variables.TryGetValue(variableName, out var value) == true ? value : null;
    }

    /// <summary>
    /// Gets the flow definition associated with the user's active flow.
    /// </summary>
    /// <param name="engine">The conversation flow engine.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The flow definition if the user has an active flow;
    /// otherwise, <c>null</c>.
    /// </returns>
    public static async Task<FlowDefinition?> GetActiveFlowAsync(
        this ConversationFlowEngine engine,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var state = await engine.GetActiveFlowStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state == null ? null : await engine.GetFlowAsync(state.FlowId, cancellationToken).ConfigureAwait(false);
    }
}
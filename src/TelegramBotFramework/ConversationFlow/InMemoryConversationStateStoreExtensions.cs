#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides extension methods for <see cref="InMemoryConversationStateStore"/> to simplify common
/// state management operations beyond the basic CRUD interface.
/// </summary>
public static class InMemoryConversationStateStoreExtensions
{
    /// <summary>
    /// Attempts to load the state for the specified user and converts it to a strongly-typed
    /// record if present. Returns <c>null</c> when the user has no active state.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The loaded state or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is zero or negative.</exception>
    public static async Task<UserFlowState?> TryLoadStateAsync(
        this InMemoryConversationStateStore store,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(userId, 0);
        ArgumentNullException.ThrowIfNull(store);

        return await store.LoadStateAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether a state record exists for the specified user.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the user has an active state; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is zero or negative.</exception>
    public static async Task<bool> HasStateAsync(
        this InMemoryConversationStateStore store,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(userId, 0);
        ArgumentNullException.ThrowIfNull(store);

        var state = await store.LoadStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return state is not null;
    }

    /// <summary>
    /// Loads the state for a user and updates the <see cref="FlowStateStatus"/> to the specified value.
    /// The updated state is immediately persisted back to the store.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="newStatus">The status value to set on the state.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The updated state, or <c>null</c> if no state existed for the user.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is zero or negative.</exception>
    public static async Task<UserFlowState?> UpdateStateStatusAsync(
        this InMemoryConversationStateStore store,
        long userId,
        FlowStateStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(userId, 0);
        ArgumentNullException.ThrowIfNull(store);

        var state = await store.LoadStateAsync(userId, cancellationToken).ConfigureAwait(false);
        if (state is null)
        {
            return null;
        }

        state.Status = newStatus;
        await store.SaveStateAsync(state, cancellationToken).ConfigureAwait(false);
        return state;
    }

    /// <summary>
    /// Loads all active states (Active or WaitingForInput) and returns them as an immutable list.
    /// The result is suitable for batch operations like broadcasting to multiple users.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An immutable list of active states.</returns>
    public static async Task<IReadOnlyList<UserFlowState>> GetActiveStatesAsync(
        this InMemoryConversationStateStore store,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        return await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes all state records where the flow execution has reached a terminal state
    /// (Completed, Aborted, TimedOut, or Failed). Useful for cleanup operations.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of states that were removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
    public static async Task<int> RemoveTerminalStatesAsync(
        this InMemoryConversationStateStore store,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var activeStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        var terminalCount = 0;

        foreach (var state in activeStates)
        {
            if (state.Status is FlowStateStatus.Completed
                or FlowStateStatus.Aborted
                or FlowStateStatus.TimedOut
                or FlowStateStatus.Failed)
            {
                await store.DeleteStateAsync(state.UserId, cancellationToken).ConfigureAwait(false);
                terminalCount++;
            }
        }

        return terminalCount;
    }

    /// <summary>
    /// Updates the <see cref="UserFlowState.LastActivityAt"/> timestamp to the current UTC time.
    /// Useful for implementing inactivity timeouts without manual timestamp manipulation.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the state was updated; otherwise, <c>false</c> if no state existed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is zero or negative.</exception>
    public static async Task<bool> TouchStateAsync(
        this InMemoryConversationStateStore store,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(userId, 0);
        ArgumentNullException.ThrowIfNull(store);

        var state = await store.LoadStateAsync(userId, cancellationToken).ConfigureAwait(false);
        if (state is null)
        {
            return false;
        }

        state.LastActivityAt = DateTime.UtcNow;
        await store.SaveStateAsync(state, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the total number of states currently stored in the in-memory dictionary.
    /// This is a convenience wrapper around the <see cref="InMemoryConversationStateStore.Count"/> property.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <returns>The total count of stored states (active and terminal).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
    public static int GetStateCount(this InMemoryConversationStateStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        return store.Count;
    }

    /// <summary>
    /// Attempts to retrieve a state by its unique <see cref="UserFlowState.StateId"/> identifier.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="stateId">The unique state identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The state with the matching identifier, or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stateId"/> is <c>null</c>, empty, or whitespace.</exception>
    public static async Task<UserFlowState?> FindStateByIdAsync(
        this InMemoryConversationStateStore store,
        string stateId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateId);
        ArgumentNullException.ThrowIfNull(store);

        var allStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        return allStates.FirstOrDefault(s => string.Equals(s.StateId, stateId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Removes all state records that have not been updated since the specified cutoff time.
    /// Useful for implementing automatic cleanup of stale states.
    /// </summary>
    /// <param name="store">The state store instance.</param>
    /// <param name="cutoffUtc">The UTC timestamp threshold; states with <see cref="UserFlowState.LastActivityAt"/> before this time are removed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of states that were removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
    public static async Task<int> RemoveStaleStatesAsync(
        this InMemoryConversationStateStore store,
        DateTime cutoffUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var activeStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        var removedCount = 0;

        foreach (var state in activeStates)
        {
            if (state.LastActivityAt < cutoffUtc)
            {
                await store.DeleteStateAsync(state.UserId, cancellationToken).ConfigureAwait(false);
                removedCount++;
            }
        }

        return removedCount;
    }
}

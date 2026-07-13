#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides extension methods for <see cref="FileConversationStateStore"/> to simplify common
/// state management operations such as bulk state operations, state existence checks, and
/// state filtering by status or flow.
/// </summary>
public static class FileConversationStateStoreExtensions
{
    /// <summary>
    /// Determines whether a state file exists for the specified user.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if a state file exists; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    public static async Task<bool> ExistsAsync(
        this FileConversationStateStore store,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var path = store.GetFilePath(userId);
        return File.Exists(path);
    }

    /// <summary>
    /// Loads the state for the specified user, returning <c>null</c> if the state does not exist.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The loaded state, or <c>null</c> if no state file exists.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    public static async Task<UserFlowState?> TryLoadStateAsync(
        this FileConversationStateStore store,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        return await store.LoadStateAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to delete the state for the specified user if it exists.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the state file existed and was deleted; <c>false</c> if it did not exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    public static async Task<bool> TryDeleteStateAsync(
        this FileConversationStateStore store,
        long userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var path = store.GetFilePath(userId);
        if (!File.Exists(path))
        {
            return false;
        }

        await store.DeleteStateAsync(userId, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Loads all states with the specified status.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of states matching the specified status.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyList<UserFlowState>> LoadStatesByStatusAsync(
        this FileConversationStateStore store,
        FlowStateStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var allStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        return allStates.Where(s => s.Status == status).ToList().AsReadOnly();
    }

    /// <summary>
    /// Loads all states for the specified flow identifier.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="flowId">The flow identifier to filter by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of states for the specified flow.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="flowId"/> is <c>null</c> or whitespace.</exception>
    public static async Task<IReadOnlyList<UserFlowState>> LoadStatesByFlowAsync(
        this FileConversationStateStore store,
        string flowId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrEmpty(flowId, nameof(flowId));

        var allStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        return allStates.Where(s => string.Equals(s.FlowId, flowId, StringComparison.Ordinal)).ToList().AsReadOnly();
    }

    /// <summary>
    /// Loads all states for the specified flow identifier with the given status.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="flowId">The flow identifier to filter by.</param>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of states matching both the flow and status.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="flowId"/> is <c>null</c> or whitespace.</exception>
    public static async Task<IReadOnlyList<UserFlowState>> LoadStatesByFlowAndStatusAsync(
        this FileConversationStateStore store,
        string flowId,
        FlowStateStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrEmpty(flowId, nameof(flowId));

        var allStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        return allStates
            .Where(s => string.Equals(s.FlowId, flowId, StringComparison.Ordinal) && s.Status == status)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the file path where the state for the specified user is stored.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <returns>The full file system path to the state file.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    public static string GetStateFilePath(
        this FileConversationStateStore store,
        long userId)
    {
        ArgumentNullException.ThrowIfNull(store);

        return store.GetFilePath(userId);
    }

    /// <summary>
    /// Loads all states that have been inactive for longer than the specified threshold.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="inactivityThreshold">The duration of inactivity to consider stale.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of states that have been inactive beyond the threshold.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="inactivityThreshold"/> is negative.</exception>
    public static async Task<IReadOnlyList<UserFlowState>> LoadInactiveStatesAsync(
        this FileConversationStateStore store,
        TimeSpan inactivityThreshold,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (inactivityThreshold < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(inactivityThreshold), "Inactivity threshold cannot be negative.");
        }

        var now = DateTime.UtcNow;
        var allStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        return allStates
            .Where(s => now - s.LastActivityAt > inactivityThreshold)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Loads all completed states older than the specified age.
    /// </summary>
    /// <param name="store">The conversation state store instance.</param>
    /// <param name="maxAge">The maximum age of completed states to include.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of completed states older than the specified age.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxAge"/> is negative.</exception>
    public static async Task<IReadOnlyList<UserFlowState>> LoadOldCompletedStatesAsync(
        this FileConversationStateStore store,
        TimeSpan maxAge,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (maxAge < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAge), "Maximum age cannot be negative.");
        }

        var cutoff = DateTime.UtcNow - maxAge;
        var allStates = await store.LoadAllActiveStatesAsync(cancellationToken).ConfigureAwait(false);
        return allStates
            .Where(s => s.Status == FlowStateStatus.Completed && s.CompletedAt < cutoff)
            .ToList()
            .AsReadOnly();
    }
}

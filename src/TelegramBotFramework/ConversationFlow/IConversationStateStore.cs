#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Persistence contract for <see cref="UserFlowState"/> objects.
/// Implement this interface to store active conversation states in any backing
/// medium (in-memory, file system, database, distributed cache, etc.) so that
/// states survive process restarts and can be resumed by <see cref="IConversationFlowEngine"/>.
/// </summary>
public interface IConversationStateStore
{
    /// <summary>
    /// Persists or overwrites the given <paramref name="state"/>.
    /// Called whenever a flow state is created or mutated.
    /// </summary>
    /// <param name="state">The flow state to persist. Must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the active flow state for the given <paramref name="userId"/>.
    /// Returns <c>null</c> when no persisted state exists.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<UserFlowState?> LoadStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the persisted state for the given <paramref name="userId"/>.
    /// Silently succeeds when no state exists for that user.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task DeleteStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all states whose <see cref="UserFlowState.Status"/> is
    /// <see cref="FlowStateStatus.Active"/> or <see cref="FlowStateStatus.WaitingForInput"/>.
    /// Used by <see cref="IConversationFlowEngine"/> to rebuild its in-memory index on startup.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<UserFlowState>> LoadAllActiveStatesAsync(CancellationToken cancellationToken = default);
}

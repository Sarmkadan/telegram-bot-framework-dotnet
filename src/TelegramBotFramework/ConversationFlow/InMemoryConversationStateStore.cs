#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Thread-safe, in-process implementation of <see cref="IConversationStateStore"/>.
/// State is stored in a <see cref="ConcurrentDictionary{TKey,TValue}"/> and does not
/// survive process restarts. Suitable for development and single-instance deployments.
/// </summary>
public sealed class InMemoryConversationStateStore : IConversationStateStore, IInMemoryConversationStateStore
{
    private readonly ConcurrentDictionary<long, UserFlowState> _store = new();

    /// <inheritdoc/>
    public Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        _store.AddOrUpdate(state.UserId, state, (_, _) => state);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<UserFlowState?> LoadStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(userId, out var state);
        return Task.FromResult<UserFlowState?>(state);
    }

    /// <inheritdoc/>
    public Task DeleteStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(userId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<UserFlowState>> LoadAllActiveStatesAsync(CancellationToken cancellationToken = default)
    {
        var active = _store.Values
            .Where(s => s.Status is FlowStateStatus.Active or FlowStateStatus.WaitingForInput)
            .ToList();

        return Task.FromResult<IReadOnlyList<UserFlowState>>(active);
    }

    /// <summary>Gets the total number of persisted states (active and terminal).</summary>
    public int Count => _store.Count;
}

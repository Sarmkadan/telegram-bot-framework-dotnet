#nullable enable

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Interface for in-memory conversation state store.
/// </summary>
public interface IInMemoryConversationStateStore
{
    /// <summary>
    /// Saves the state asynchronously.
    /// </summary>
    /// <param name="state">The state to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the state asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the state or null if not found.</returns>
    Task<UserFlowState?> LoadStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the state asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all active states asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns a read-only list of active states.</returns>
    Task<IReadOnlyList<UserFlowState>> LoadAllActiveStatesAsync(CancellationToken cancellationToken = default);
}
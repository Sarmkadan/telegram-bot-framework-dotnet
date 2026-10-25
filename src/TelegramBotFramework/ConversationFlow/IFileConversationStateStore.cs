namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Defines a file-backed store for persisting per-user <see cref="UserFlowState"/> instances.
/// Each user's state is kept in its own file, so state can survive process restarts.
/// </summary>
public interface IFileConversationStateStore
{
    /// <summary>
    /// Persists the given flow state to the file associated with <see cref="UserFlowState.UserId"/>,
    /// overwriting any previously saved state for that user.
    /// </summary>
    /// <param name="state">The flow state to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the state has been written.</returns>
    Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the saved flow state for the specified user.
    /// </summary>
    /// <param name="userId">The Telegram user identifier whose state should be loaded.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that resolves to the stored <see cref="UserFlowState"/>, or <c>null</c> if no state exists for the user.
    /// </returns>
    Task<UserFlowState?> LoadStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the saved flow state for the specified user. Does nothing if no state exists.
    /// </summary>
    /// <param name="userId">The Telegram user identifier whose state should be deleted.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the state has been removed.</returns>
    Task DeleteStateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all flow states currently stored in the backing location.
    /// Typically used at startup to restore active conversations.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that resolves to a read-only list of every stored <see cref="UserFlowState"/>.</returns>
    Task<IReadOnlyList<UserFlowState>> LoadAllActiveStatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full file path where the flow state for the specified user is stored.
    /// </summary>
    /// <param name="userId">The Telegram user identifier.</param>
    /// <returns>The absolute or store-relative file path associated with the user's state.</returns>
    string GetFilePath(long userId);

    /// <summary>
    /// Releases the resources held by the store, such as file locks or handles.
    /// </summary>
    void Dispose();
}

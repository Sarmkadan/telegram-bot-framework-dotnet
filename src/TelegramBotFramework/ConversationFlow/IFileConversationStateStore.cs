namespace TelegramBotFramework.ConversationFlow;

public interface IFileConversationStateStore
{
    Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken = default);
    Task<UserFlowState?> LoadStateAsync(long userId, CancellationToken cancellationToken = default);
    Task DeleteStateAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserFlowState>> LoadAllActiveStatesAsync(CancellationToken cancellationToken = default);
    string GetFilePath(long userId);
    void Dispose();
}
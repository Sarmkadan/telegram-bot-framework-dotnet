namespace TelegramBotFramework.Repositories;

/// <summary>
/// Interface for in-memory message repository.
/// </summary>
public interface IInMemoryMessageRepository
{
    Task<Models.Message?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Models.Message> CreateAsync(Models.Message entity, CancellationToken cancellationToken = default);
    Task<Models.Message> UpdateAsync(Models.Message entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetByStatusAsync(Models.MessageStatus status, CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetByCommandAsync(string commandName, CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IList<Models.Message>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
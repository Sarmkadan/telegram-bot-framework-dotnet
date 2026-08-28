namespace TelegramBotFramework.Repositories;

public interface IInMemoryUserRepository
{
    Task<Models.BotUser?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IList<Models.BotUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Models.BotUser> CreateAsync(Models.BotUser entity, CancellationToken cancellationToken = default);
    Task<Models.BotUser> UpdateAsync(Models.BotUser entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<Models.BotUser?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<Models.BotUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IList<Models.BotUser>> GetByStatusAsync(Models.UserStatus status, CancellationToken cancellationToken = default);
    Task<IList<Models.BotUser>> GetByRoleAsync(Models.UserRole role, CancellationToken cancellationToken = default);
    Task<IList<Models.BotUser>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IList<Models.BotUser>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
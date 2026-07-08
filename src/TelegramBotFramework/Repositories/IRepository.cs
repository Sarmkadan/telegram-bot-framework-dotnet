#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations.
/// </summary>
public interface IRepository<T, in TId> where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for user operations.
/// </summary>
public interface IUserRepository : IRepository<Models.BotUser, long>
{
    Task<Models.BotUser?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);

    Task<Models.BotUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<IList<Models.BotUser>> GetByStatusAsync(Models.UserStatus status, CancellationToken cancellationToken = default);

    Task<IList<Models.BotUser>> GetByRoleAsync(Models.UserRole role, CancellationToken cancellationToken = default);

    Task<IList<Models.BotUser>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<IList<Models.BotUser>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for command operations.
/// </summary>
public interface ICommandRepository : IRepository<Models.Command, string>
{
    Task<Models.Command?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IList<Models.Command>> GetEnabledAsync(CancellationToken cancellationToken = default);

    Task<IList<Models.Command>> GetByTypeAsync(Models.CommandType type, CancellationToken cancellationToken = default);

    Task<IList<Models.Command>> GetAdminOnlyAsync(CancellationToken cancellationToken = default);

    Task<IList<Models.Command>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for message operations.
/// </summary>
public interface IMessageRepository : IRepository<Models.Message, long>
{
    Task<IList<Models.Message>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetByStatusAsync(Models.MessageStatus status, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetByCommandAsync(string commandName, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for session operations.
/// </summary>
public interface ISessionRepository : IRepository<Models.UserSession, string>
{
    Task<Models.UserSession?> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active session for a user. Equivalent to <see cref="GetActiveByUserIdAsync"/>.
    /// </summary>
    Task<Models.UserSession?> GetActiveSessionAsync(long userId, CancellationToken cancellationToken = default);

    Task<IList<Models.UserSession>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<IList<Models.UserSession>> GetExpiredAsync(CancellationToken cancellationToken = default);

    Task<IList<Models.UserSession>> GetByStateAsync(Models.SessionState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions currently in the active state.
    /// </summary>
    Task<IList<Models.UserSession>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    Task<int> CloseExpiredSessionsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for menu operations.
/// </summary>
public interface IMenuRepository : IRepository<Models.Menu, string>
{
    Task<IList<Models.Menu>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IList<Models.Menu>> GetByTypeAsync(Models.MenuType type, CancellationToken cancellationToken = default);

    Task<IList<Models.Menu>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
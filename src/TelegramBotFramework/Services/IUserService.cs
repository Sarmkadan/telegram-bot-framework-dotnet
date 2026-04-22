#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Service interface for user management operations.
/// </summary>
public interface IUserService
{
    Task<Models.BotUser> GetOrCreateUserAsync(long telegramId, string firstName, string? lastName = null, CancellationToken cancellationToken = default);

    Task<Models.BotUser?> GetUserByIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<Models.BotUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);

    Task<Models.BotUser> UpdateUserAsync(Models.BotUser user, CancellationToken cancellationToken = default);

    Task<bool> BanUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<bool> UnbanUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<IList<Models.BotUser>> GetAdministratorsAsync(CancellationToken cancellationToken = default);

    Task<bool> PromoteToAdminAsync(long userId, CancellationToken cancellationToken = default);

    Task<bool> DemoteAdminAsync(long userId, CancellationToken cancellationToken = default);

    Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default);

    Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default);

    Task RecordUserActivityAsync(long userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for command management and execution.
/// </summary>
public interface ICommandService
{
    Task<Models.Command?> GetCommandAsync(string commandName, CancellationToken cancellationToken = default);

    Task<Models.Command> RegisterCommandAsync(Models.Command command, CancellationToken cancellationToken = default);

    Task<bool> UnregisterCommandAsync(string commandName, CancellationToken cancellationToken = default);

    Task<IList<Models.Command>> GetAvailableCommandsAsync(Models.UserRole userRole = Models.UserRole.User, CancellationToken cancellationToken = default);

    Task<Models.ExecutionContext> ExecuteCommandAsync(Models.ExecutionContext context, CancellationToken cancellationToken = default);

    Task<bool> CanUserExecuteCommandAsync(long userId, string commandName, CancellationToken cancellationToken = default);

    Task<bool> IsCommandRateLimitedAsync(long userId, string commandName, CancellationToken cancellationToken = default);

    Task RecordCommandExecutionAsync(string commandName, CancellationToken cancellationToken = default);

    Task<int> GetCommandExecutionCountAsync(string commandName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for session management.
/// </summary>
public interface ISessionService
{
    Task<Models.UserSession> CreateSessionAsync(long userId, long chatId, CancellationToken cancellationToken = default);

    Task<Models.UserSession?> GetActiveSessionAsync(long userId, CancellationToken cancellationToken = default);

    Task<Models.UserSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<bool> UpdateSessionContextAsync(string sessionId, string contextKey, string value, CancellationToken cancellationToken = default);

    Task<string?> GetSessionContextAsync(string sessionId, string contextKey, CancellationToken cancellationToken = default);

    Task<bool> CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<int> CloseExpiredSessionsAsync(CancellationToken cancellationToken = default);

    Task<Models.UserSession> NavigateToMenuAsync(string sessionId, string menuId, CancellationToken cancellationToken = default);

    Task RecordSessionActivityAsync(string sessionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for menu management and navigation.
/// </summary>
public interface IMenuService
{
    Task<Models.Menu?> GetMenuAsync(string menuId, CancellationToken cancellationToken = default);

    Task<Models.Menu> CreateMenuAsync(Models.Menu menu, CancellationToken cancellationToken = default);

    Task<bool> DeleteMenuAsync(string menuId, CancellationToken cancellationToken = default);

    Task<Models.Menu> UpdateMenuAsync(Models.Menu menu, CancellationToken cancellationToken = default);

    Task<Models.MenuButton?> GetButtonAsync(string menuId, string callbackData, CancellationToken cancellationToken = default);

    Task<Models.Menu> AddButtonAsync(string menuId, Models.MenuButton button, CancellationToken cancellationToken = default);

    Task<bool> RemoveButtonAsync(string menuId, string callbackData, CancellationToken cancellationToken = default);

    Task<IList<Models.Menu>> GetActiveMenusAsync(CancellationToken cancellationToken = default);

    Task<List<List<Models.MenuButton>>> GetArrangedButtonsAsync(string menuId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for message processing.
/// </summary>
public interface IMessageService
{
    Task<Models.Message> ProcessIncomingMessageAsync(Models.Message message, CancellationToken cancellationToken = default);

    Task<Models.Message?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetUserMessagesAsync(long userId, int limit = 50, CancellationToken cancellationToken = default);

    Task<IList<Models.Message>> GetFailedMessagesAsync(int limit = 100, CancellationToken cancellationToken = default);

    Task<bool> MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken = default);

    Task<bool> MarkAsFailedAsync(long messageId, string errorMessage, CancellationToken cancellationToken = default);

    Task<int> GetUnprocessedMessageCountAsync(CancellationToken cancellationToken = default);

    Task ArchiveOldMessagesAsync(int daysOld = 30, CancellationToken cancellationToken = default);
}
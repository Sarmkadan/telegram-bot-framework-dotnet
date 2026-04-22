#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Implementation of session management service.
/// </summary>
public sealed class SessionService : ISessionService
{
    private readonly Repositories.ISessionRepository _sessionRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<SessionService> _logger;
    private readonly Models.BotConfiguration _configuration;

    public SessionService(
        Repositories.ISessionRepository sessionRepository,
        Models.BotConfiguration configuration,
        Microsoft.Extensions.Logging.ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.UserSession> CreateSessionAsync(
        long userId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var session = new Models.UserSession
        {
            SessionId = Guid.NewGuid().ToString(),
            UserId = userId,
            ChatId = chatId,
            State = Models.SessionState.Active,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_configuration.GetSessionTimeout())
        };

        session.Validate();
        var created = await _sessionRepository.CreateAsync(session, cancellationToken);
        _logger.LogInformation("Session created: {SessionId} for user {UserId}", session.SessionId, userId);
        return created;
    }

    public async Task<Models.UserSession?> GetActiveSessionAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _sessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
    }

    public async Task<Models.UserSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
    }

    public async Task<bool> UpdateSessionContextAsync(
        string sessionId,
        string contextKey,
        string value,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session  is null)
        {
            return false;
        }

        session.SetContextData(contextKey, value);
        session.UpdateActivity();
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        return true;
    }

    public async Task<string?> GetSessionContextAsync(string sessionId, string contextKey, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        return session?.GetContextData(contextKey);
    }

    public async Task<bool> CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session  is null)
        {
            return false;
        }

        session.State = Models.SessionState.Closed;
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        _logger.LogInformation("Session closed: {SessionId}", sessionId);
        return true;
    }

    public async Task<int> CloseExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var count = await _sessionRepository.CloseExpiredSessionsAsync(cancellationToken);
        _logger.LogInformation("Closed {Count} expired sessions", count);
        return count;
    }

    public async Task<Models.UserSession> NavigateToMenuAsync(
        string sessionId,
        string menuId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session  is null)
        {
            throw new Exceptions.SessionException($"Session {sessionId} not found", sessionId);
        }

        session.CurrentMenuId = menuId;
        session.CurrentContext = "menu";
        session.UpdateActivity();
        var updated = await _sessionRepository.UpdateAsync(session, cancellationToken);
        return updated;
    }

    public async Task RecordSessionActivityAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session  is not null && !session.IsExpired())
        {
            session.UpdateActivity();
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }
    }
}

/// <summary>
/// Implementation of menu management service.
/// </summary>
public sealed class MenuService : IMenuService
{
    private readonly Repositories.IMenuRepository _menuRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<MenuService> _logger;

    public MenuService(
        Repositories.IMenuRepository menuRepository,
        Microsoft.Extensions.Logging.ILogger<MenuService> logger)
    {
        _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.Menu?> GetMenuAsync(string menuId, CancellationToken cancellationToken = default)
    {
        return await _menuRepository.GetByIdAsync(menuId, cancellationToken);
    }

    public async Task<Models.Menu> CreateMenuAsync(Models.Menu menu, CancellationToken cancellationToken = default)
    {
        menu.Validate();
        var created = await _menuRepository.CreateAsync(menu, cancellationToken);
        _logger.LogInformation("Menu created: {MenuId}", menu.Id);
        return created;
    }

    public async Task<bool> DeleteMenuAsync(string menuId, CancellationToken cancellationToken = default)
    {
        var result = await _menuRepository.DeleteAsync(menuId, cancellationToken);
        if (result)
        {
            _logger.LogInformation("Menu deleted: {MenuId}", menuId);
        }
        return result;
    }

    public async Task<Models.Menu> UpdateMenuAsync(Models.Menu menu, CancellationToken cancellationToken = default)
    {
        menu.Validate();
        var updated = await _menuRepository.UpdateAsync(menu, cancellationToken);
        _logger.LogInformation("Menu updated: {MenuId}", menu.Id);
        return updated;
    }

    public async Task<Models.MenuButton?> GetButtonAsync(string menuId, string callbackData, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId, cancellationToken);
        return menu?.GetButton(callbackData);
    }

    public async Task<Models.Menu> AddButtonAsync(string menuId, Models.MenuButton button, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId, cancellationToken);
        if (menu  is null)
        {
            throw new InvalidOperationException($"Menu {menuId} not found");
        }

        menu.AddButton(button);
        return await _menuRepository.UpdateAsync(menu, cancellationToken);
    }

    public async Task<bool> RemoveButtonAsync(string menuId, string callbackData, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId, cancellationToken);
        if (menu  is null)
        {
            return false;
        }

        var removed = menu.RemoveButton(callbackData);
        if (removed)
        {
            await _menuRepository.UpdateAsync(menu, cancellationToken);
        }
        return removed;
    }

    public async Task<IList<Models.Menu>> GetActiveMenusAsync(CancellationToken cancellationToken = default)
    {
        return await _menuRepository.GetActiveAsync(cancellationToken);
    }

    public async Task<List<List<Models.MenuButton>>> GetArrangedButtonsAsync(string menuId, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId, cancellationToken);
        if (menu  is null)
        {
            return new List<List<Models.MenuButton>>();
        }

        return menu.GetArrangedButtons();
    }
}
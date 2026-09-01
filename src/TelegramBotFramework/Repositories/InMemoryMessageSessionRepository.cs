#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Repositories;

/// <summary>
/// In-memory implementation of message repository.
/// </summary>
public sealed class InMemoryMessageRepository : IMessageRepository, IInMemoryMessageRepository
{
    private readonly Dictionary<long, Models.Message> _messages = new();
    private long _messageIdCounter = 1;
    private readonly object _lockObj = new();

    public async Task<Models.Message?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.TryGetValue(id, out var msg) ? msg : null;
        }
    }

    public async Task<IList<Models.Message>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values.ToList();
        }
    }

    public async Task<Models.Message> CreateAsync(Models.Message entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        entity.Validate();
        lock (_lockObj)
        {
            entity.MessageId = _messageIdCounter++;
            _messages[entity.MessageId] = entity;
            return entity;
        }
    }

    public async Task<Models.Message> UpdateAsync(Models.Message entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        entity.Validate();
        lock (_lockObj)
        {
            _messages[entity.MessageId] = entity;
            return entity;
        }
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Remove(id);
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.ContainsKey(id);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Count;
        }
    }

    public async Task<IList<Models.Message>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values.Where(m => m.UserId == userId).ToList();
        }
    }

    public async Task<IList<Models.Message>> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values.Where(m => m.ChatId == chatId).ToList();
        }
    }

    public async Task<IList<Models.Message>> GetByStatusAsync(Models.MessageStatus status, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values.Where(m => m.Status == status).ToList();
        }
    }

    public async Task<IList<Models.Message>> GetByCommandAsync(string commandName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(commandName);
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values.Where(m => m.CommandName == commandName).ToList();
        }
    }

    public async Task<IList<Models.Message>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .ToList();
        }
    }

    public async Task<IList<Models.Message>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _messages.Values
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}

/// <summary>
/// In-memory implementation of session repository.
/// </summary>
public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly Dictionary<string, Models.UserSession> _sessions = new();
    private readonly object _lockObj = new();

    public async Task<Models.UserSession?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.TryGetValue(id, out var session) ? session : null;
        }
    }

    public async Task<IList<Models.UserSession>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Values.ToList();
        }
    }

    public async Task<Models.UserSession> CreateAsync(Models.UserSession entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        entity.Validate();
        lock (_lockObj)
        {
            _sessions[entity.SessionId] = entity;
            return entity;
        }
    }

    public async Task<Models.UserSession> UpdateAsync(Models.UserSession entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        entity.Validate();
        lock (_lockObj)
        {
            _sessions[entity.SessionId] = entity;
            return entity;
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Remove(id);
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.ContainsKey(id);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Count;
        }
    }

    public async Task<Models.UserSession?> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Values.FirstOrDefault(s => s.UserId == userId && s.State == Models.SessionState.Active);
        }
    }

    public Task<Models.UserSession?> GetActiveSessionAsync(long userId, CancellationToken cancellationToken = default) =>
        GetActiveByUserIdAsync(userId, cancellationToken);

    public async Task<IList<Models.UserSession>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Values.Where(s => s.State == Models.SessionState.Active).ToList();
        }
    }

    public async Task<IList<Models.UserSession>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Values.Where(s => s.UserId == userId).ToList();
        }
    }

    public async Task<IList<Models.UserSession>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Values.Where(s => s.IsExpired()).ToList();
        }
    }

    public async Task<IList<Models.UserSession>> GetByStateAsync(Models.SessionState state, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _sessions.Values.Where(s => s.State == state).ToList();
        }
    }

    public async Task<int> CloseExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            var expiredSessions = _sessions.Values.Where(s => s.IsExpired()).ToList();
            foreach (var session in expiredSessions)
            {
                session.State = Models.SessionState.Expired;
            }
            return expiredSessions.Count;
        }
    }
}

/// <summary>
/// In-memory implementation of menu repository.
/// </summary>
public sealed class InMemoryMenuRepository : IMenuRepository
{
    private readonly Dictionary<string, Models.Menu> _menus = new();
    private readonly object _lockObj = new();

    public async Task<Models.Menu?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.TryGetValue(id, out var menu) ? menu : null;
        }
    }

    public async Task<IList<Models.Menu>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.Values.ToList();
        }
    }

    public async Task<Models.Menu> CreateAsync(Models.Menu entity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        entity.Validate();
        lock (_lockObj)
        {
            _menus[entity.Id] = entity;
            return entity;
        }
    }

    public async Task<Models.Menu> UpdateAsync(Models.Menu entity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        entity.Validate();
        lock (_lockObj)
        {
            _menus[entity.Id] = entity;
            return entity;
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.Remove(id);
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.ContainsKey(id);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.Count;
        }
    }

    public async Task<IList<Models.Menu>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.Values.Where(m => m.IsActive).ToList();
        }
    }

    public async Task<IList<Models.Menu>> GetByTypeAsync(Models.MenuType type, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.Values.Where(m => m.Type == type).ToList();
        }
    }

    public async Task<IList<Models.Menu>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        lock (_lockObj)
        {
            return _menus.Values
                .OrderBy(m => m.DisplayOrder)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
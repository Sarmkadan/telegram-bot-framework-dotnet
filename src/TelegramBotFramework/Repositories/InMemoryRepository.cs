#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Repositories;

/// <summary>
/// In-memory implementation of user repository.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<long, Models.BotUser> _users = new();
    private readonly object _lockObj = new();

    public async Task<Models.BotUser?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.TryGetValue(id, out var user) ? user : null;
        }
    }

    public async Task<IList<Models.BotUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Values.ToList();
        }
    }

    public async Task<Models.BotUser> CreateAsync(Models.BotUser entity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        entity.Validate();
        lock (_lockObj)
        {
            _users[entity.TelegramId] = entity;
            return entity;
        }
    }

    public async Task<Models.BotUser> UpdateAsync(Models.BotUser entity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        entity.Validate();
        lock (_lockObj)
        {
            _users[entity.TelegramId] = entity;
            return entity;
        }
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Remove(id);
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.ContainsKey(id);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Count;
        }
    }

    public async Task<Models.BotUser?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Values.FirstOrDefault(u => u.TelegramId == telegramId);
        }
    }

    public async Task<Models.BotUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Values.FirstOrDefault(u => u.Username == username);
        }
    }

    public async Task<IList<Models.BotUser>> GetByStatusAsync(Models.UserStatus status, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Values.Where(u => u.Status == status).ToList();
        }
    }

    public async Task<IList<Models.BotUser>> GetByRoleAsync(Models.UserRole role, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Values.Where(u => u.Role == role).ToList();
        }
    }

    public async Task<IList<Models.BotUser>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        var lower = searchTerm.ToLower();
        lock (_lockObj)
        {
            return _users.Values
                .Where(u => u.FirstName?.ToLower().Contains(lower) == true ||
                           u.LastName?.ToLower().Contains(lower) == true ||
                           u.Username?.ToLower().Contains(lower) == true)
                .ToList();
        }
    }

    public async Task<IList<Models.BotUser>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _users.Values
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}

/// <summary>
/// In-memory implementation of command repository.
/// </summary>
public sealed class InMemoryCommandRepository : ICommandRepository
{
    private readonly Dictionary<string, Models.Command> _commands = new();
    private readonly object _lockObj = new();

    public async Task<Models.Command?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.TryGetValue(id, out var cmd) ? cmd : null;
        }
    }

    public async Task<IList<Models.Command>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Values.ToList();
        }
    }

    public async Task<Models.Command> CreateAsync(Models.Command entity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        entity.Validate();
        lock (_lockObj)
        {
            _commands[entity.Name] = entity;
            return entity;
        }
    }

    public async Task<Models.Command> UpdateAsync(Models.Command entity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        entity.Validate();
        lock (_lockObj)
        {
            _commands[entity.Name] = entity;
            return entity;
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Remove(id);
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.ContainsKey(id);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Count;
        }
    }

    public async Task<Models.Command?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Values.FirstOrDefault(c => c.Name == name);
        }
    }

    public async Task<IList<Models.Command>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Values.Where(c => c.IsEnabled).ToList();
        }
    }

    public async Task<IList<Models.Command>> GetByTypeAsync(Models.CommandType type, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Values.Where(c => c.Type == type).ToList();
        }
    }

    public async Task<IList<Models.Command>> GetAdminOnlyAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Values.Where(c => c.RequiresAdmin).ToList();
        }
    }

    public async Task<IList<Models.Command>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        lock (_lockObj)
        {
            return _commands.Values
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
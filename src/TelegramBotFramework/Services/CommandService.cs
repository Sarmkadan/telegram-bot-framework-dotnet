#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Implementation of command management service.
/// </summary>
public sealed class CommandService : ICommandService
{
    private readonly Repositories.ICommandRepository _commandRepository;
    private readonly IUserService _userService;
    private readonly Microsoft.Extensions.Logging.ILogger<CommandService> _logger;
    private readonly Dictionary<string, (int Count, DateTime WindowStart)> _commandExecutionRateLimiter = new();
    private readonly object _rateLimitLockObj = new();
    private DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(2);

    public CommandService(
        Repositories.ICommandRepository commandRepository,
        IUserService userService,
        Microsoft.Extensions.Logging.ILogger<CommandService> logger)
    {
        _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a command by name. Automatically normalizes names to include the "/" prefix.
    /// </summary>
    /// <param name="commandName">Command name with or without "/" prefix.</param>
    /// <returns>The command definition, or null if not registered.</returns>
    public async Task<Models.Command?> GetCommandAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var normalized = commandName.StartsWith("/") ? commandName : $"/{commandName}";
        return await _commandRepository.GetByNameAsync(normalized, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers a new bot command. Validates the command definition before persisting.
    /// </summary>
    /// <param name="command">Command definition including name, description, and handler.</param>
    /// <returns>The persisted command with any server-assigned fields populated.</returns>
    public async Task<Models.Command> RegisterCommandAsync(Models.Command command, CancellationToken cancellationToken = default)
    {
        command.Validate();
        var created = await _commandRepository.CreateAsync(command, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Command registered: {CommandName}", command.Name);
        return created;
    }

    public async Task<bool> UnregisterCommandAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var normalized = commandName.StartsWith("/") ? commandName : $"/{commandName}";
        var result = await _commandRepository.DeleteAsync(normalized, cancellationToken).ConfigureAwait(false);
        if (result)
        {
            _logger.LogInformation("Command unregistered: {CommandName}", normalized);
        }
        return result;
    }

    /// <summary>
    /// Returns commands available to the given user role. Admin-only commands are
    /// excluded for users below Administrator level.
    /// </summary>
    public async Task<IList<Models.Command>> GetAvailableCommandsAsync(
        Models.UserRole userRole = Models.UserRole.User,
        CancellationToken cancellationToken = default)
    {
        var allCommands = await _commandRepository.GetEnabledAsync(cancellationToken).ConfigureAwait(false);
        return allCommands
            .Where(c => !c.RequiresAdmin || userRole >= Models.UserRole.Administrator)
            .ToList();
    }

    public async Task<Models.ExecutionContext> ExecuteCommandAsync(
        Models.ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.Validate())
        {
            return context;
        }

        if (context.Command  is null)
        {
            context.AddError("Command not specified in context");
            return context;
        }

        try
        {
            if (!context.Command.IsEnabled)
            {
                context.AddError($"Command {context.Command.Name} is disabled");
                return context;
            }

            if (context.Command.RequiresAdmin && context.User?.Role < Models.UserRole.Administrator)
            {
                context.AddError("Insufficient permissions to execute this command");
                return context;
            }

            context.Command.RecordExecution();
            await _commandRepository.UpdateAsync(context.Command, cancellationToken).ConfigureAwait(false);

            context.SetState("executed", true);
            context.SetState("execution_time_ms", context.GetDuration().TotalMilliseconds);
            _logger.LogInformation("Command executed: {CommandName} for user {UserId}",
                context.Command.Name, context.UserId);
        }
        catch (Exception ex)
        {
            context.AddError($"Command execution failed: {ex.Message}");
            _logger.LogError(ex, "Command execution error: {CommandName}", context.Command.Name);
        }

        return context;
    }

    public async Task<bool> CanUserExecuteCommandAsync(long userId, string commandName, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user  is null || user.Status != Models.UserStatus.Active)
        {
            return false;
        }

        var command = await GetCommandAsync(commandName, cancellationToken).ConfigureAwait(false);
        if (command  is null || !command.IsEnabled)
        {
            return false;
        }

        return command.CanExecuteBy(user.Role);
    }

    public async Task<bool> IsCommandRateLimitedAsync(long userId, string commandName, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        var command = await GetCommandAsync(commandName, cancellationToken).ConfigureAwait(false);
        if (command?.RateLimitPerMinute  is null)
        {
            return false;
        }

        lock (_rateLimitLockObj)
        {
            // Periodically evict expired entries to prevent unbounded growth
            var now = DateTime.UtcNow;
            if (now - _lastCleanup > CleanupInterval)
            {
                var expiredKeys = _commandExecutionRateLimiter
                    .Where(kvp => now - kvp.Value.WindowStart > RateLimitWindow)
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var expired in expiredKeys)
                    _commandExecutionRateLimiter.Remove(expired);
                _lastCleanup = now;
            }

            var key = $"{userId}:{commandName}";
            if (!_commandExecutionRateLimiter.TryGetValue(key, out var entry) ||
                now - entry.WindowStart > RateLimitWindow)
            {
                _commandExecutionRateLimiter[key] = (1, now);
                return false;
            }

            if (entry.Count >= command.RateLimitPerMinute)
            {
                return true;
            }

            _commandExecutionRateLimiter[key] = (entry.Count + 1, entry.WindowStart);
            return false;
        }
    }

    public async Task RecordCommandExecutionAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var command = await GetCommandAsync(commandName, cancellationToken).ConfigureAwait(false);
        if (command  is not null)
        {
            command.RecordExecution();
            await _commandRepository.UpdateAsync(command, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<int> GetCommandExecutionCountAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var command = await GetCommandAsync(commandName, cancellationToken).ConfigureAwait(false);
        return command?.ExecutionCount ?? 0;
    }
}
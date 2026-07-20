#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using System.Collections.Concurrent;
using System.Reflection;

namespace TelegramBotFramework.Services;

/// <summary>
/// Implementation of command management service.
/// </summary>
public sealed class CommandService : ICommandService
{
    private readonly Repositories.ICommandRepository _commandRepository;
    private readonly IUserService _userService;
    private readonly ICommandUsageTracker _commandUsageTracker;
    private readonly Microsoft.Extensions.Logging.ILogger<CommandService> _logger;
    private readonly Dictionary<string, (int Count, DateTime WindowStart)> _commandExecutionRateLimiter = new();
    private readonly object _rateLimitLockObj = new();
    private DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(2);
private readonly ConcurrentDictionary<string, DateTime> _lastCommandInvocations = new();

    public CommandService(
        Repositories.ICommandRepository commandRepository,
        IUserService userService,
        ICommandUsageTracker commandUsageTracker,
        Microsoft.Extensions.Logging.ILogger<CommandService> logger)
    {
        _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandUsageTracker = commandUsageTracker ?? throw new ArgumentNullException(nameof(commandUsageTracker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.Command?> GetCommandAsync(string commandName, CancellationToken cancellationToken = default)
    {
    var normalized = commandName.StartsWith("/") ? commandName : $"/{commandName}";

    // First try to get the command by its primary name
    var command = await _commandRepository.GetByNameAsync(normalized, cancellationToken).ConfigureAwait(false);

    if (command != null)
    {
        return command;
    }

    // If not found, check if it's an alias
    // Get all commands and check their aliases
    var allCommands = await _commandRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    if (allCommands != null)
        {
                        foreach (var cmd in allCommands)
    {
        if (cmd.Aliases.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            return cmd;
        }
        }
    }

    return null;
    }

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

    public async Task<IList<Models.Command>> GetAvailableCommandsAsync(
        Models.UserRole userRole = Models.UserRole.User,
        CancellationToken cancellationToken = default)
    {
        var allCommands = await _commandRepository.GetEnabledAsync(cancellationToken).ConfigureAwait(false);
        return allCommands
            .Where(c => c.IsEnabled && (!c.RequiresAdmin || userRole >= Models.UserRole.Administrator))
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

        // Check cooldown
        var cooldownAttribute = context.Command.GetType()
            .GetCustomAttributes(typeof(Attributes.CooldownAttribute), false)
            .FirstOrDefault() as Attributes.CooldownAttribute;

        if (cooldownAttribute != null)
        {
            var key = $"{context.UserId}:{context.Command.Name}";
            var lastInvocation = _lastCommandInvocations.GetValueOrDefault(key);
            var cooldownPeriod = TimeSpan.FromSeconds(cooldownAttribute.Seconds);

            if (DateTime.UtcNow - lastInvocation < cooldownPeriod)
            {
                context.AddError($"Command {context.Command.Name} is on cooldown. Please wait {cooldownAttribute.Seconds} seconds.");
                return context;
            }

            _lastCommandInvocations[key] = DateTime.UtcNow;
        }

		// Record command usage statistics
		_commandUsageTracker.RecordCommandInvocation(context.Command.Name);



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
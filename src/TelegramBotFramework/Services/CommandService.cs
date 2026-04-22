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
    private readonly Dictionary<string, int> _commandExecutionRateLimiter = new();
    private readonly object _rateLimitLockObj = new();

    public CommandService(
        Repositories.ICommandRepository commandRepository,
        IUserService userService,
        Microsoft.Extensions.Logging.ILogger<CommandService> logger)
    {
        _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.Command?> GetCommandAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var normalized = commandName.StartsWith("/") ? commandName : $"/{commandName}";
        return await _commandRepository.GetByNameAsync(normalized, cancellationToken);
    }

    public async Task<Models.Command> RegisterCommandAsync(Models.Command command, CancellationToken cancellationToken = default)
    {
        command.Validate();
        var created = await _commandRepository.CreateAsync(command, cancellationToken);
        _logger.LogInformation("Command registered: {CommandName}", command.Name);
        return created;
    }

    public async Task<bool> UnregisterCommandAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var normalized = commandName.StartsWith("/") ? commandName : $"/{commandName}";
        var result = await _commandRepository.DeleteAsync(normalized, cancellationToken);
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
        var allCommands = await _commandRepository.GetEnabledAsync(cancellationToken);
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
            await _commandRepository.UpdateAsync(context.Command, cancellationToken);

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
        var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
        if (user  is null || user.Status != Models.UserStatus.Active)
        {
            return false;
        }

        var command = await GetCommandAsync(commandName, cancellationToken);
        if (command  is null || !command.IsEnabled)
        {
            return false;
        }

        return command.CanExecuteBy(user.Role);
    }

    public async Task<bool> IsCommandRateLimitedAsync(long userId, string commandName, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken);
        var command = await GetCommandAsync(commandName, cancellationToken);
        if (command?.RateLimitPerMinute  is null)
        {
            return false;
        }

        lock (_rateLimitLockObj)
        {
            var key = $"{userId}:{commandName}";
            if (!_commandExecutionRateLimiter.TryGetValue(key, out var count))
            {
                _commandExecutionRateLimiter[key] = 1;
                return false;
            }

            if (count >= command.RateLimitPerMinute)
            {
                return true;
            }

            _commandExecutionRateLimiter[key]++;
            return false;
        }
    }

    public async Task RecordCommandExecutionAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var command = await GetCommandAsync(commandName, cancellationToken);
        if (command  is not null)
        {
            command.RecordExecution();
            await _commandRepository.UpdateAsync(command, cancellationToken);
        }
    }

    public async Task<int> GetCommandExecutionCountAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var command = await GetCommandAsync(commandName, cancellationToken);
        return command?.ExecutionCount ?? 0;
    }
}
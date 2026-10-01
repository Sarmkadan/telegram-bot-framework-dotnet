#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Reflection;
using TelegramBotFramework.Attributes;
using TelegramBotFramework.Commands;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Services;

/// <summary>
/// Defines the contract for routing command text to the appropriate handler.
/// </summary>
public interface ICommandRouter
{
    /// <summary>
    /// Registers a command handler for the given command name.
    /// </summary>
    /// <param name="commandName">The command name without the leading slash.</param>
    /// <param name="handler">The handler to invoke for this command.</param>
    void Register(string commandName, ICommandHandler handler);

    /// <summary>
    /// Registers all <see cref="ICommandHandler"/> implementations found in the given assembly.
    /// Uses <see cref="CommandAttribute"/> to determine the command name.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="handlerFactory">Factory that creates handler instances (typically from DI).</param>
    /// <returns>The number of handlers registered.</returns>
    int RegisterFromAssembly(Assembly assembly, Func<Type, ICommandHandler> handlerFactory);

    /// <summary>
    /// Routes the input text to the matching handler and executes it.
    /// </summary>
    /// <param name="input">The raw command text (e.g. "/start arg1 arg2").</param>
    /// <param name="context">The execution context for this command invocation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated execution context, or <see langword="null"/> when no handler matched.</returns>
    Task<Models.ExecutionContext?> RouteAsync(string input, Models.ExecutionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a handler is registered for the given command name.
    /// </summary>
    bool HasHandler(string commandName);

    /// <summary>
    /// Gets the names of all registered commands.
    /// </summary>
    IReadOnlyList<string> RegisteredCommands { get; }
}

/// <summary>
/// Routes incoming command text to the matching <see cref="ICommandHandler"/> by name or alias.
/// Supports assembly scanning via <see cref="CommandAttribute"/> and per-command aliases.
/// </summary>
public sealed class CommandRouter : ICommandRouter
{
    private readonly ConcurrentDictionary<string, ICommandHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<CommandRouter> _logger;

    /// <inheritdoc />
    public IReadOnlyList<string> RegisteredCommands => _handlers.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRouter"/> class.
    /// </summary>
    public CommandRouter(ILogger<CommandRouter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Register(string commandName, ICommandHandler handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(handler);

        var normalized = commandName.TrimStart('/').ToLowerInvariant();
        if (!_handlers.TryAdd(normalized, handler))
        {
            _handlers[normalized] = handler;
            _logger.LogWarning("Replaced existing handler for command '{CommandName}'", normalized);
        }
        else
        {
            _logger.LogDebug("Registered handler for command '{CommandName}'", normalized);
        }
    }

    /// <inheritdoc />
    public int RegisterFromAssembly(Assembly assembly, Func<Type, ICommandHandler> handlerFactory)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(handlerFactory);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(ICommandHandler).IsAssignableFrom(t)
                        && t.GetCustomAttribute<CommandAttribute>() is not null);

        var count = 0;
        foreach (var type in handlerTypes)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>()!;
            var handler = handlerFactory(type);
            Register(attr.Name, handler);
            count++;
        }

        _logger.LogInformation("Registered {Count} command handlers from assembly {Assembly}", count, assembly.GetName().Name);
        return count;
    }

    /// <inheritdoc />
    public async Task<Models.ExecutionContext?> RouteAsync(string input, Models.ExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        ArgumentNullException.ThrowIfNull(context);

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts[0].TrimStart('/').ToLowerInvariant();

        if (!_handlers.TryGetValue(commandName, out var handler))
        {
            _logger.LogDebug("No handler registered for command '{CommandName}'", commandName);
            return null;
        }

        if (parts.Length > 1)
        {
            context.Parameters ??= new Dictionary<string, object>();
            context.Parameters["CommandArgs"] = parts[1];
        }

        _logger.LogDebug("Routing command '{CommandName}' to {HandlerType}", commandName, handler.GetType().Name);

        return await handler.HandleAsync(context, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool HasHandler(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        return _handlers.ContainsKey(commandName.TrimStart('/').ToLowerInvariant());
    }
}

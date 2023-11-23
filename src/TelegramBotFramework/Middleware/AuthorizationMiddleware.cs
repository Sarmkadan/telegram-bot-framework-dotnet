#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Middleware for handling user authorization and command permissions.
/// </summary>
public sealed class AuthorizationMiddleware : IBotMiddleware
{
    private readonly IUserService _userService;
    private readonly ICommandService _commandService;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(
        IUserService userService,
        ICommandService commandService,
        ILogger<AuthorizationMiddleware> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 30; // Authorization usually comes after logging and error handling, before rate limiting

    public async Task<ExecutionContext> ProcessAsync(
        ExecutionContext context,
        Func<ExecutionContext, Task<ExecutionContext>> next,
        CancellationToken cancellationToken)
    {
        if (!context.IsValid)
        {
            return await next(context).ConfigureAwait(false);
        }

        if (context.User == null)
        {
            context.AddError("User not found in context for authorization.");
            _logger.LogWarning("AuthorizationMiddleware: User not found for UserId: {UserId}", context.UserId);
            return await next(context).ConfigureAwait(false);
        }

        // Check if a command is being executed and if the user has permission
        if (context.Command != null)
        {
            var command = await _commandService.GetCommandAsync(context.Command.CommandName, cancellationToken)
                .ConfigureAwait(false);

            if (command != null && command.RequiredRoles != null && command.RequiredRoles.Any())
            {
                var userRoles = await _userService.GetUserRolesAsync(context.User.UserId, cancellationToken)
                    .ConfigureAwait(false);

                if (!command.RequiredRoles.Any(role => userRoles.Contains(role)))
                {
                    context.AddError($"User {context.User.UserId} is not authorized to execute command '{context.Command.CommandName}'.");
                    _logger.LogWarning("AuthorizationMiddleware: User {UserId} denied access to command {CommandName}",
                        context.User.UserId, context.Command.CommandName);
                    return context; // Stop processing if not authorized
                }
            }
        }

        // Add any other authorization checks here (e.g., specific user permissions based on message content)

        return await next(context).ConfigureAwait(false);
    }
}
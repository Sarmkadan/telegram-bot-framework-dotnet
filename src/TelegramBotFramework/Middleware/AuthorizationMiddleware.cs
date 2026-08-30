#nullable enable
// Pipeline order: authorization must run after authentication.
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

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken)
    {
        if (!context.IsValid)
        {
            return context;
        }

        if (context.User == null)
        {
            context.AddError("User not found in context for authorization.");
            _logger.LogWarning("AuthorizationMiddleware: User not found for UserId: {UserId}", context.UserId);
            return context;
        }

        if (context.Command != null)
        {
            var command = await _commandService.GetCommandAsync(context.Command.Name, cancellationToken)
                .ConfigureAwait(false);

            if (command != null && command.RequiresAdmin && context.User.Role < Models.UserRole.Admin)
            {
                context.AddError($"User {context.User.TelegramId} is not authorized to execute command '{context.Command.Name}'.");
                _logger.LogWarning("AuthorizationMiddleware: User {UserId} denied access to command {CommandName}",
                    context.User.TelegramId, context.Command.Name);
                return context;
            }
        }

        return await next(context).ConfigureAwait(false);
    }
}

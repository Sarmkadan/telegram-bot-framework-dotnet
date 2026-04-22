#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Exceptions;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Base middleware interface for request processing pipeline.
/// </summary>
public interface IBotMiddleware
{
    int Priority { get; }

    Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Middleware for logging execution details.
/// </summary>
public sealed class LoggingMiddleware : IBotMiddleware
{
    public int Priority => 100;

    private readonly Microsoft.Extensions.Logging.ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(Microsoft.Extensions.Logging.ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "Processing request - UserId: {UserId}, Command: {Command}, ContextId: {ContextId}",
            context.UserId,
            context.Command?.Name ?? "unknown",
            context.ContextId);

        try
        {
            var result = await next(context);
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Request completed - ContextId: {ContextId}, Duration: {DurationMs}ms, IsValid: {IsValid}",
                context.ContextId,
                duration.TotalMilliseconds,
                result.IsValid);

            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex,
                "Request failed - ContextId: {ContextId}, Duration: {DurationMs}ms",
                context.ContextId,
                duration.TotalMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Middleware for authorization checks.
/// </summary>
public sealed class AuthorizationMiddleware : IBotMiddleware
{
    public int Priority => 90;

    private readonly IUserService _userService;
    private readonly ICommandService _commandService;
    private readonly Microsoft.Extensions.Logging.ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(
        IUserService userService,
        ICommandService commandService,
        Microsoft.Extensions.Logging.ILogger<AuthorizationMiddleware> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        if (context.Command  is null)
        {
            return await next(context);
        }

        var canExecute = await _commandService.CanUserExecuteCommandAsync(
            context.UserId,
            context.Command.Name,
            cancellationToken);

        if (!canExecute)
        {
            context.AddError("User does not have permission to execute this command");
            _logger.LogWarning(
                "Authorization failed - UserId: {UserId}, Command: {Command}",
                context.UserId,
                context.Command.Name);
            return context;
        }

        return await next(context);
    }
}

/// <summary>
/// Middleware for rate limiting.
/// </summary>
public sealed class RateLimitMiddleware : IBotMiddleware
{
    public int Priority => 95;

    private readonly ICommandService _commandService;
    private readonly Models.BotConfiguration _configuration;
    private readonly Microsoft.Extensions.Logging.ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(
        ICommandService commandService,
        Models.BotConfiguration configuration,
        Microsoft.Extensions.Logging.ILogger<RateLimitMiddleware> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        if (!_configuration.EnableRateLimiting || context.Command  is null)
        {
            return await next(context);
        }

        var isRateLimited = await _commandService.IsCommandRateLimitedAsync(
            context.UserId,
            context.Command.Name,
            cancellationToken);

        if (isRateLimited)
        {
            context.AddError("Rate limit exceeded for this command");
            _logger.LogWarning(
                "Rate limit exceeded - UserId: {UserId}, Command: {Command}",
                context.UserId,
                context.Command.Name);
            return context;
        }

        return await next(context);
    }
}

/// <summary>
/// Middleware for error handling and recovery.
/// </summary>
public sealed class ErrorHandlingMiddleware : IBotMiddleware
{
    public int Priority => 10;

    private readonly Microsoft.Extensions.Logging.ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(Microsoft.Extensions.Logging.ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await next(context);
        }
        catch (Exceptions.BotFrameworkException ex)
        {
            context.AddError($"{ex.ErrorCode}: {ex.Message}");
            _logger.LogError(ex, "Bot framework error - ErrorCode: {ErrorCode}", ex.ErrorCode);
            return context;
        }
        catch (Exception ex)
        {
            context.AddError($"Unexpected error: {ex.Message}");
            _logger.LogError(ex, "Unexpected error occurred");
            return context;
        }
    }
}
#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Middleware;

using TelegramBotFramework.Exceptions;
using TelegramBotFramework.Models;

/// <summary>
/// Middleware for structured logging of bot execution contexts.
/// </summary>
/// <summary>
/// 
/// </summary>
public sealed class BotLoggingMiddleware : IBotMiddleware, IBotLoggingMiddleware
{
    private readonly ILogger<BotLoggingMiddleware> _logger;

    /// <summary>
    /// 
    /// </summary>
    public BotLoggingMiddleware(ILogger<BotLoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 
    /// </summary>
    public int Priority => 100;

    /// <summary>
    /// 
    /// </summary>
    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        var commandName = context.Command?.Name ?? context.Message?.CommandName ?? "<none>";

        _logger.LogInformation(
            "Bot request started - UserId: {UserId}, Command: {Command}, ContextId: {ContextId}",
            context.UserId,
            commandName,
            context.ContextId);

        var result = await next(context).ConfigureAwait(false);

        _logger.LogInformation(
            "Bot request completed - UserId: {UserId}, Command: {Command}, ContextId: {ContextId}, IsValid: {IsValid}",
            result.UserId,
            commandName,
            result.ContextId,
            result.IsValid);

        return result;
    }
}

/// <summary>
/// Middleware for translating bot framework exceptions into execution context errors.
/// </summary>
/// <summary>
/// 
/// </summary>
public sealed class BotErrorHandlingMiddleware : IBotMiddleware
{
    private readonly ILogger<BotErrorHandlingMiddleware> _logger;

    /// <summary>
    /// 
    /// </summary>
    public BotErrorHandlingMiddleware(ILogger<BotErrorHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 
    /// </summary>
    public int Priority => 10;

    /// <summary>
    /// 
    /// </summary>
    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await next(context).ConfigureAwait(false);
        }
        catch (BotFrameworkException ex)
        {
            context.AddError(ex.Message);
            _logger.LogError(
                ex,
                "Bot framework error - UserId: {UserId}, ContextId: {ContextId}, ErrorCode: {ErrorCode}",
                context.UserId,
                context.ContextId,
                ex.ErrorCode ?? "BOT_FRAMEWORK_ERROR");
            return context;
        }
    }
}
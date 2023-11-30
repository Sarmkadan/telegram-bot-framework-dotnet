#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Strategies; // Assuming RateLimitingStrategy is here

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Middleware for handling rate limiting of user requests.
/// </summary>
public sealed class RateLimitingMiddleware : IBotMiddleware
{
    private readonly ICommandService _commandService;
    private readonly BotConfiguration _configuration;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IRateLimitingStrategy _rateLimitingStrategy;

    public RateLimitingMiddleware(
        ICommandService commandService,
        BotConfiguration configuration,
        IRateLimitingStrategy rateLimitingStrategy,
        ILogger<RateLimitingMiddleware> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _rateLimitingStrategy = rateLimitingStrategy ?? throw new ArgumentNullException(nameof(rateLimitingStrategy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 20; // Rate limiting usually comes after authorization

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken)
    {
        if (!_configuration.EnableRateLimiting || !context.IsValid)
        {
            return await next(context).ConfigureAwait(false);
        }

        if (context.User == null)
        {
            context.AddError("User not found in context for rate limiting.");
            _logger.LogWarning("RateLimitingMiddleware: User not found for UserId: {UserId}", context.UserId);
            return await next(context).ConfigureAwait(false);
        }

        // Check if user is an admin or owner, bypass rate limiting for them
        if (_configuration.IsAdmin(context.User.TelegramId))
        {
            _logger.LogDebug("RateLimitingMiddleware: User {UserId} is admin, bypassing rate limit.", context.User.TelegramId);
            return await next(context).ConfigureAwait(false);
        }

        var key = $"RateLimit:{context.User.TelegramId}";
        var limit = _configuration.RateLimitPerMinute;
        var interval = TimeSpan.FromMinutes(1);

        var allowed = await _rateLimitingStrategy.IsActionAllowedAsync(key, limit, interval, cancellationToken)
            .ConfigureAwait(false);

        if (!allowed)
        {
            context.AddError($"Rate limit exceeded for user {context.User.TelegramId}. Please try again later.");
            _logger.LogWarning("RateLimitingMiddleware: User {UserId} exceeded rate limit.", context.User.TelegramId);
            return context;
        }

        return await next(context).ConfigureAwait(false);
    }
}
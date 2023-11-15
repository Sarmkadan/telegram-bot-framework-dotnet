#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Strategies;

/// <summary>
/// Async compatibility helpers for rate limiting strategies.
/// </summary>
public static class RateLimitingStrategyExtensions
{
    public static Task<bool> IsActionAllowedAsync(
        this IRateLimitingStrategy strategy,
        string identifier,
        int limit,
        TimeSpan interval,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(strategy.IsRequestAllowed(identifier));
    }
}

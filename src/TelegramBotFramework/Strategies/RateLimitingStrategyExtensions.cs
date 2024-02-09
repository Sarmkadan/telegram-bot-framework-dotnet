#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace TelegramBotFramework.Strategies;

/// <summary>
/// Async compatibility helpers for rate limiting strategies.
/// </summary>
public static class RateLimitingStrategyExtensions
{
    /// <summary>
    /// Determines whether the specified action is allowed based on the rate limiting strategy.
    /// </summary>
    /// <param name="strategy">The rate limiting strategy to use.</param>
    /// <param name="identifier">The identifier for the rate limiting bucket (e.g., user ID).</param>
    /// <param name="limit">The maximum number of requests allowed in the time window. Ignored as strategies are pre-configured.</param>
    /// <param name="interval">The time window for rate limiting. Ignored as strategies are pre-configured.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if the action is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="strategy"/> is null.</exception>
    public static Task<bool> IsActionAllowedAsync(
        this IRateLimitingStrategy strategy,
        string identifier,
        int limit,
        TimeSpan interval,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(strategy.IsRequestAllowed(identifier));
    }

    /// <summary>
    /// Determines whether the specified action is allowed based on the rate limiting strategy.
    /// </summary>
    /// <param name="strategy">The rate limiting strategy to use.</param>
    /// <param name="identifier">The identifier for the rate limiting bucket (e.g., user ID).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if the action is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="strategy"/> is null.</exception>
    public static Task<bool> IsActionAllowedAsync(
        this IRateLimitingStrategy strategy,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(strategy.IsRequestAllowed(identifier));
    }
}
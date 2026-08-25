namespace TelegramBotFramework.Middleware.Tests;

using System;
using System.Collections.Generic;

/// <summary>
/// Validation helpers for <see cref="RateLimitingMiddlewareTests"/>.
/// </summary>
public static class RateLimitingMiddlewareTestsValidation
{
    /// <summary>
    /// Validates the <see cref="RateLimitingMiddlewareTests"/> instance and returns a list of problems.
    /// </summary>
    /// <param name="value">The <see cref="RateLimitingMiddlewareTests"/> instance to validate.</param>
    /// <returns>A list of human-readable problems. Empty list if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitingMiddlewareTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="RateLimitingMiddlewareTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="RateLimitingMiddlewareTests"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this RateLimitingMiddlewareTests value)
    {
        return value != null;
    }

    /// <summary>
    /// Ensures the <see cref="RateLimitingMiddlewareTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="RateLimitingMiddlewareTests"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this RateLimitingMiddlewareTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}
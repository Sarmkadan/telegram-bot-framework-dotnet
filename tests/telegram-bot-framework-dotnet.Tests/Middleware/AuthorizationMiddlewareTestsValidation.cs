namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Validation helpers for <see cref="AuthorizationMiddlewareTests"/>.
/// </summary>
public static class AuthorizationMiddlewareTestsValidation
{
    /// <summary>
    /// Validates the <see cref="AuthorizationMiddlewareTests"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="AuthorizationMiddlewareTests"/> instance to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty list if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this AuthorizationMiddlewareTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // AuthorizationMiddlewareTests has no public state to validate; all instances are considered valid.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AuthorizationMiddlewareTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="AuthorizationMiddlewareTests"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this AuthorizationMiddlewareTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // AuthorizationMiddlewareTests has no public state to validate; all instances are considered valid.
        return true;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AuthorizationMiddlewareTests"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> if the instance is invalid.
    /// </summary>
    /// <param name="value">The <see cref="AuthorizationMiddlewareTests"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid.</exception>
    public static void EnsureValid(this AuthorizationMiddlewareTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // AuthorizationMiddlewareTests has no public state to validate; all instances are considered valid.
        // No action needed.
    }
}
#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="HttpErrorHandlingMiddleware"/> instances.
/// </summary>
public static class HttpErrorHandlingMiddlewareValidation
{
    /// <summary>
    /// Validates the specified <see cref="HttpErrorHandlingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HttpErrorHandlingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.ErrorCode))
        {
            problems.Add("ErrorCode must not be null or empty.");
        }

        if (string.IsNullOrEmpty(value.Message))
        {
            problems.Add("Message must not be null or empty.");
        }

        if (value.Timestamp == default)
        {
            problems.Add("Timestamp must be set to a non-default value.");
        }

        if (string.IsNullOrEmpty(value.Path))
        {
            problems.Add("Path must not be null or empty.");
        }

        if (string.IsNullOrEmpty(value.TraceId))
        {
            problems.Add("TraceId must not be null or empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="HttpErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this HttpErrorHandlingMiddleware? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="HttpErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this HttpErrorHandlingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HttpErrorHandlingMiddleware is invalid. Problems: {string.Join(" ", problems)}");
        }
    }
}
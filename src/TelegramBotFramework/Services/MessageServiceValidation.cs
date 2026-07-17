#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace TelegramBotFramework.Services;

/// <summary>
/// Provides validation helpers for <see cref="MessageService"/> instances.
/// </summary>
public static class MessageServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="MessageService"/> instance.
    /// </summary>
    /// <remarks>
    /// This validation checks that the service instance is not null.
    /// The <see cref="MessageService"/> class validates its dependencies in its constructor,
    /// and has no additional configurable properties to validate.
    /// </remarks>
    /// <param name="value">The message service to validate.</param>
    /// <returns>A list of validation errors; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null check via ArgumentNullException.ThrowIfNull")]
    public static IReadOnlyList<string> Validate(this MessageService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MessageService"/> instance is valid.
    /// </summary>
    /// <param name="value">The message service to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this MessageService? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MessageService"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The message service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the service is null (validation always passes for non-null services).</exception>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null check via ArgumentNullException.ThrowIfNull")]
    public static void EnsureValid(this MessageService? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}
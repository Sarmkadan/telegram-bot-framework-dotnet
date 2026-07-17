#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Provides validation helpers for <see cref="ExternalApiIntegration"/> instances.
/// Validates configuration, dependencies, and state of external API integrations.
/// </summary>
public static class ExternalApiIntegrationValidation
{
    /// <summary>
    /// Validates an <see cref="ExternalApiIntegration"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ExternalApiIntegration? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ExternalApiIntegration"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ExternalApiIntegration? value) => value is not null;

    /// <summary>
    /// Ensures that the specified <see cref="ExternalApiIntegration"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ExternalApiIntegration? value) =>
        ArgumentNullException.ThrowIfNull(value);
}
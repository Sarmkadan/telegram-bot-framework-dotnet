#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Provides validation helpers for <see cref="MessageService"/> instances.
/// </summary>
public static class MessageServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="MessageService"/> instance.
    /// </summary>
    /// <param name="value">The message service to validate.</param>
    /// <returns>A list of validation errors; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MessageService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MessageService"/> instance is valid.
    /// </summary>
    /// <param name="value">The message service to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    public static bool IsValid(this MessageService? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="MessageService"/> instance is valid.
    /// </summary>
    /// <param name="value">The message service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the service has validation errors.</exception>
    public static void EnsureValid(this MessageService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MessageService validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}
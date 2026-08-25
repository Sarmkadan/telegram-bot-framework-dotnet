#nullable enable

using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Validation helpers for <see cref="CallbackDataSignerTests"/>.
/// </summary>
public static class CallbackDataSignerTestsValidation
{
    /// <summary>
    /// Validates the <see cref="CallbackDataSignerTests"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="CallbackDataSignerTests"/> instance to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CallbackDataSignerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="CallbackDataSignerTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="CallbackDataSignerTests"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this CallbackDataSignerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true;
    }

    /// <summary>
    /// Ensures that the <see cref="CallbackDataSignerTests"/> instance is valid. Throws an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="value">The <see cref="CallbackDataSignerTests"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this CallbackDataSignerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // No validation rules to check for this type.
    }
}
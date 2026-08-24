#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Validation helpers for <see cref="MessageServiceTests"/>.
/// </summary>
public static class MessageServiceTestsValidation
{
    /// <summary>
    /// Validates the <see cref="MessageServiceTests"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="MessageServiceTests"/> instance to validate.</param>
    /// <returns>A list of validation error messages. Empty list if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this MessageServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // MessageServiceTests has no public state to validate; all state is private.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="MessageServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="MessageServiceTests"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this MessageServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // MessageServiceTests has no public state to validate; all state is private.
        return true;
    }

    /// <summary>
    /// Ensures the <see cref="MessageServiceTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The <see cref="MessageServiceTests"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">If the instance is invalid; contains validation error messages.</exception>
    public static void EnsureValid(this MessageServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var errors = value.Validate();
        if (errors.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors), nameof(value));
        }
    }
}
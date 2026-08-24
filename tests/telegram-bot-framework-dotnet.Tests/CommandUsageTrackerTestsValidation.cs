using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Validation helpers for <see cref="CommandUsageTrackerTests"/>.
/// </summary>
public static class CommandUsageTrackerTestsValidation
{
    /// <summary>
    /// Validates the <see cref="CommandUsageTrackerTests"/> instance and returns a list of errors.
    /// </summary>
    /// <param name="value">The <see cref="CommandUsageTrackerTests"/> instance to validate.</param>
    /// <returns>A list of error messages, or empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(CommandUsageTrackerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // CommandUsageTrackerTests has no public state to validate; all members are test methods.
        // Therefore, there are no validation rules based on instance state.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="CommandUsageTrackerTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="CommandUsageTrackerTests"/> instance to validate.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(CommandUsageTrackerTests value)
    {
        try
        {
            Validate(value);
            return true;
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the <see cref="CommandUsageTrackerTests"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> if the instance is invalid.
    /// </summary>
    /// <param name="value">The <see cref="CommandUsageTrackerTests"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">If the instance is invalid, with a list of validation errors.</exception>
    public static void EnsureValid(CommandUsageTrackerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Any())
        {
            throw new ArgumentException(
                $"The {nameof(CommandUsageTrackerTests)} instance is invalid. Errors: {string.Join("; ", errors)}",
                nameof(value));
        }
    }
}
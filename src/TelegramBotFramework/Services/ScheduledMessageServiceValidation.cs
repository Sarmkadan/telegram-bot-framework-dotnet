#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Services;

/// <summary>
/// Validation helpers for <see cref="ScheduledMessageService"/>.
/// </summary>
public static class ScheduledMessageServiceValidation
{
    /// <summary>
    /// Validates the <see cref="ScheduledMessageService"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="ScheduledMessageService"/> instance to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty list indicates the object is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ScheduledMessageService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // ScheduledMessageService has no public state to validate; all validation occurs via method parameters.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="ScheduledMessageService"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ScheduledMessageService"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ScheduledMessageService value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures the <see cref="ScheduledMessageService"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The <see cref="ScheduledMessageService"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing validation error messages.</exception>
    public static void EnsureValid(this ScheduledMessageService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
        }
    }
}
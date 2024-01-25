#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Provides validation helpers for <see cref="ScheduledTask"/> instances.
/// Validates null/empty values, out-of-range numbers, default dates, and logical inconsistencies.
/// </summary>
public static class ScheduledTaskManagerValidation
{
    /// <summary>
    /// Validates the specified <see cref="ScheduledTask"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ScheduledTask? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Id is null or whitespace.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name is null or whitespace.");
        }

        // Validate TaskFunc - can be null (valid state)
        if (value.TaskFunc is null)
        {
            problems.Add("TaskFunc is null.");
        }

        // Validate IsRecurring
        // No validation needed - boolean can always be valid

        // Validate Interval
        if (value.Interval.TotalMilliseconds <= 0)
        {
            problems.Add("Interval must be greater than zero.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt is default (DateTime.MinValue).");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CreatedAt is in the future.");
        }

        // Validate LastExecutedAt (if set)
        if (value.LastExecutedAt.HasValue)
        {
            if (value.LastExecutedAt.Value == default)
            {
                problems.Add("LastExecutedAt is default (DateTime.MinValue).");
            }
            else if (value.LastExecutedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("LastExecutedAt is in the future.");
            }
            else if (value.LastExecutedAt.Value < value.CreatedAt)
            {
                problems.Add("LastExecutedAt cannot be before CreatedAt.");
            }
        }

        // Validate LastSuccessAt (if set)
        if (value.LastSuccessAt.HasValue)
        {
            if (value.LastSuccessAt.Value == default)
            {
                problems.Add("LastSuccessAt is default (DateTime.MinValue).");
            }
            else if (value.LastSuccessAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("LastSuccessAt is in the future.");
            }
            else if (value.LastSuccessAt.Value < value.CreatedAt)
            {
                problems.Add("LastSuccessAt cannot be before CreatedAt.");
            }

            // If LastSuccessAt is set, LastExecutedAt should also be set and not after it
            if (value.LastExecutedAt.HasValue && value.LastExecutedAt.Value > value.LastSuccessAt.Value)
            {
                problems.Add("LastExecutedAt cannot be after LastSuccessAt.");
            }
        }

        // Validate LastErrorAt (if set)
        if (value.LastErrorAt.HasValue)
        {
            if (value.LastErrorAt.Value == default)
            {
                problems.Add("LastErrorAt is default (DateTime.MinValue).");
            }
            else if (value.LastErrorAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("LastErrorAt is in the future.");
            }
            else if (value.LastErrorAt.Value < value.CreatedAt)
            {
                problems.Add("LastErrorAt cannot be before CreatedAt.");
            }

            // If LastErrorAt is set, LastExecutedAt should also be set and not before it
            if (value.LastExecutedAt.HasValue && value.LastExecutedAt.Value < value.LastErrorAt.Value)
            {
                problems.Add("LastExecutedAt cannot be before LastErrorAt.");
            }

            // If LastErrorAt is set, LastError should also be set
            if (string.IsNullOrWhiteSpace(value.LastError))
            {
                problems.Add("LastError must be set when LastErrorAt is set.");
            }
        }

        // Validate ExecutionCount
        if (value.ExecutionCount < 0)
        {
            problems.Add("ExecutionCount cannot be negative.");
        }

        // Validate LastError (if LastErrorAt is set, this is already validated above)
        if (value.LastError is not null && string.IsNullOrWhiteSpace(value.LastError))
        {
            problems.Add("LastError is empty but not null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ScheduledTask"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ScheduledTask? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ScheduledTask"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this ScheduledTask? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ScheduledTask is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}
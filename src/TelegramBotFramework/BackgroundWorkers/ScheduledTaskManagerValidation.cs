#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

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
            problems.Add(ScheduledTaskManagerValidationConstants.IdIsNullOrWhitespace);
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add(ScheduledTaskManagerValidationConstants.NameIsNullOrWhitespace);
        }

        // Validate TaskFunc - can be null (valid state)
        if (value.TaskFunc is null)
        {
            problems.Add(ScheduledTaskManagerValidationConstants.TaskFuncIsNull);
        }

        // Validate IsRecurring
        // No validation needed - boolean can always be valid

        // Validate Interval
        if (value.Interval.TotalMilliseconds <= 0)
        {
            problems.Add(ScheduledTaskManagerValidationConstants.IntervalMustBeGreaterThanZero);
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add(ScheduledTaskManagerValidationConstants.CreatedAtIsDefault);
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(ScheduledTaskManagerValidationConstants.FutureCheckMinutes))
        {
            problems.Add(ScheduledTaskManagerValidationConstants.CreatedAtIsInTheFuture);
        }

        // Validate LastExecutedAt (if set)
        if (value.LastExecutedAt.HasValue)
        {
            if (value.LastExecutedAt.Value == default)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastExecutedAtIsDefault);
            }
            else if (value.LastExecutedAt.Value > DateTime.UtcNow.AddMinutes(ScheduledTaskManagerValidationConstants.FutureCheckMinutes))
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastExecutedAtIsInTheFuture);
            }
            else if (value.LastExecutedAt.Value < value.CreatedAt)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastExecutedAtCannotBeBeforeCreatedAt);
            }
        }

        // Validate LastSuccessAt (if set)
        if (value.LastSuccessAt.HasValue)
        {
            if (value.LastSuccessAt.Value == default)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastSuccessAtIsDefault);
            }
            else if (value.LastSuccessAt.Value > DateTime.UtcNow.AddMinutes(ScheduledTaskManagerValidationConstants.FutureCheckMinutes))
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastSuccessAtIsInTheFuture);
            }
            else if (value.LastSuccessAt.Value < value.CreatedAt)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastSuccessAtCannotBeBeforeCreatedAt);
            }

            // If LastSuccessAt is set, LastExecutedAt should also be set and not after it
            if (value.LastExecutedAt.HasValue && value.LastSuccessAt.HasValue
                && value.LastExecutedAt.Value > value.LastSuccessAt.Value)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastExecutedAtCannotBeAfterLastSuccessAt);
            }
        }

        // Validate LastErrorAt (if set)
        if (value.LastErrorAt.HasValue)
        {
            if (value.LastErrorAt.Value == default)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastErrorAtIsDefault);
            }
            else if (value.LastErrorAt.Value > DateTime.UtcNow.AddMinutes(ScheduledTaskManagerValidationConstants.FutureCheckMinutes))
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastErrorAtIsInTheFuture);
            }
            else if (value.LastErrorAt.Value < value.CreatedAt)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastErrorAtCannotBeBeforeCreatedAt);
            }

            // If LastErrorAt is set, LastExecutedAt should also be set and not before it
            if (value.LastExecutedAt.HasValue && value.LastErrorAt.HasValue
                && value.LastExecutedAt.Value < value.LastErrorAt.Value)
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastExecutedAtCannotBeBeforeLastErrorAt);
            }

            // If LastErrorAt is set, LastError should also be set
            if (string.IsNullOrWhiteSpace(value.LastError))
            {
                problems.Add(ScheduledTaskManagerValidationConstants.LastErrorMustBeSetWhenLastErrorAtIsSet);
            }
        }

        // Validate ExecutionCount
        if (value.ExecutionCount < 0)
        {
            problems.Add(ScheduledTaskManagerValidationConstants.ExecutionCountCannotBeNegative);
        }

        // Validate LastError (if LastErrorAt is set, this is already validated above)
        if (value.LastError is not null && string.IsNullOrWhiteSpace(value.LastError))
        {
            problems.Add(ScheduledTaskManagerValidationConstants.LastErrorIsEmptyButNotNull);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ScheduledTask"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ScheduledTask? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ScheduledTask"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid([NotNull] this ScheduledTask? value)
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

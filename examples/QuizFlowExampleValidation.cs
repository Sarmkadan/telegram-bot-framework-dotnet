#nullable enable

namespace TelegramBotFramework.Examples;

/// <summary>
/// Validation helpers for <see cref="QuizFlowExample"/>.
/// </summary>
public static class QuizFlowExampleValidation
{
    /// <summary>
    /// Validates the <see cref="QuizFlowExample"/> instance and returns a list of problems.
    /// </summary>
    /// <param name="value">The <see cref="QuizFlowExample"/> instance to validate.</param>
    /// <returns>A read-only list of human-readable problems. Empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this QuizFlowExample value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="QuizFlowExample"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="QuizFlowExample"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this QuizFlowExample value) => value is not null;

    /// <summary>
    /// Ensures the <see cref="QuizFlowExample"/> instance is valid. Throws an <see cref="ArgumentException"/> if the instance is invalid.
    /// </summary>
    /// <param name="value">The <see cref="QuizFlowExample"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">If the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this QuizFlowExample value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"The {nameof(QuizFlowExample)} instance is invalid. Problems: {string.Join("; ", problems)}", nameof(value));
        }
    }
}
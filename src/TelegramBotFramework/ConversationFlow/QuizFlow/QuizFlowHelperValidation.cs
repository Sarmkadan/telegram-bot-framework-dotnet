using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// Validation helpers for <see cref="QuizFlowHelper"/>.
/// </summary>
public static class QuizFlowHelperValidation
{
    /// <summary>
    /// Validates the <see cref="QuizFlowHelper"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="QuizFlowHelper"/> instance to validate.</param>
    /// <returns>A list of validation error messages. Empty if the instance is valid.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this QuizFlowHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Name))
            errors.Add("Name must not be null, empty, or whitespace.");

        if (!string.IsNullOrWhiteSpace(value.Description) && string.IsNullOrWhiteSpace(value.Description))
            errors.Add("Description must not be empty or whitespace when provided.");

        if (!string.IsNullOrWhiteSpace(value.CompletionMenuId) && string.IsNullOrWhiteSpace(value.CompletionMenuId))
            errors.Add("CompletionMenuId must not be empty or whitespace when provided.");

        if (string.IsNullOrWhiteSpace(value.FlowId))
            errors.Add("FlowId must not be null, empty, or whitespace.");

        return errors;
    }

    /// <summary>
    /// Determines whether the <see cref="QuizFlowHelper"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="QuizFlowHelper"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this QuizFlowHelper value) =>
        Validate(value).Count == 0;

    /// <summary>
    /// Ensures the <see cref="QuizFlowHelper"/> instance is valid, throwing an <see cref="System.ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The <see cref="QuizFlowHelper"/> instance to validate.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="System.ArgumentException">Thrown when the instance is invalid, containing all validation error messages.</exception>
    public static void EnsureValid(this QuizFlowHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(System.Environment.NewLine, errors), nameof(value));
        }
    }
}
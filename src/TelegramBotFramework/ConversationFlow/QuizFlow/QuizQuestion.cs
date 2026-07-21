#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// Represents a single quiz question with multiple choice options and the correct answer index.
/// </summary>
public sealed record QuizQuestion
{
    /// <summary>
    /// Gets the unique identifier for this question within the quiz.
    /// </summary>
    public required string QuestionId { get; init; }

    /// <summary>
    /// Gets the question text to display to the user.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the list of answer options (e.g., A, B, C, D).
    /// </summary>
    public required IReadOnlyList<string> Options { get; init; }

    /// <summary>
    /// Gets the index (0-based) of the correct answer in the Options list.
    /// </summary>
    public required int CorrectAnswerIndex { get; init; }

    /// <summary>
    /// Gets the score value for answering this question correctly.
    /// Defaults to 1.
    /// </summary>
    public int Score { get; init; } = 1;

    /// <summary>
    /// Gets optional feedback text shown after the user answers.
    /// </summary>
    public string? Feedback { get; init; }

    /// <summary>
    /// Gets the maximum time allowed to answer this question (in seconds).
    /// When null, uses the flow's default timeout.
    /// </summary>
    public int? TimeoutSeconds { get; init; }

    /// <summary>
    /// Validates the question configuration.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the question is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Text))
            throw new ArgumentException("Question text cannot be empty.", nameof(Text));

        if (Options == null || Options.Count == 0)
            throw new ArgumentException("At least one option must be provided.", nameof(Options));

        if (Options.Count < 2)
            throw new ArgumentException("At least two options are required.", nameof(Options));

        if (CorrectAnswerIndex < 0 || CorrectAnswerIndex >= Options.Count)
            throw new ArgumentException(
                $"CorrectAnswerIndex must be between 0 and {Options.Count - 1}.",
                nameof(CorrectAnswerIndex));

        if (Score < 0)
            throw new ArgumentException("Score cannot be negative.", nameof(Score));
    }

    /// <summary>
    /// Formats the question with numbered options for display.
    /// </summary>
    /// <returns>Formatted question text with options.</returns>
    public string FormatQuestion()
    {
        var optionsText = new System.Text.StringBuilder();
        optionsText.AppendLine(Text);
        optionsText.AppendLine();

        for (int i = 0; i < Options.Count; i++)
        {
            optionsText.AppendLine($"{i + 1}. {Options[i]}");
        }

        return optionsText.ToString().Trim();
    }

    /// <summary>
    /// Checks if the provided answer (by index) is correct.
    /// </summary>
    /// <param name="selectedIndex">The 0-based index of the selected answer.</param>
    /// <returns>True if the answer is correct; otherwise, false.</returns>
    public bool IsCorrect(int selectedIndex)
    {
        return selectedIndex == CorrectAnswerIndex;
    }
}

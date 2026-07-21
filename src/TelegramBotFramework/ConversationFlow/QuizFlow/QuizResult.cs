#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// Represents the final results of a completed quiz.
/// </summary>
public sealed record QuizResult
{
    /// <summary>
    /// Gets the total score achieved by the user.
    /// </summary>
    public required int TotalScore { get; init; }

    /// <summary>
    /// Gets the maximum possible score for the quiz.
    /// </summary>
    public required int MaxScore { get; init; }

    /// <summary>
    /// Gets the percentage score (0-100).
    /// </summary>
    public double Percentage => MaxScore > 0 ? (double)TotalScore / MaxScore * 100 : 0;

    /// <summary>
    /// Gets the grade/feedback based on the percentage score.
    /// </summary>
    public string Grade
    {
        get
        {
            if (Percentage >= 90) return "A - Excellent!";
            if (Percentage >= 80) return "B - Very Good";
            if (Percentage >= 70) return "C - Good";
            if (Percentage >= 60) return "D - Satisfactory";
            return "F - Needs Improvement";
        }
    }

    /// <summary>
    /// Gets the list of questions with user answers and correctness.
    /// </summary>
    public required IReadOnlyList<QuestionResult> QuestionResults { get; init; }

    /// <summary>
    /// Gets the timestamp when the quiz was completed.
    /// </summary>
    public required DateTime CompletedAt { get; init; }

    /// <summary>
    /// Gets optional user identifier.
    /// </summary>
    public long? UserId { get; init; }

    /// <summary>
    /// Gets optional chat identifier.
    /// </summary>
    public long? ChatId { get; init; }

    /// <summary>
    /// Formats the result as a detailed summary.
    /// </summary>
    /// <returns>Formatted result summary.</returns>
    public string FormatSummary()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("📊 **Quiz Results Summary** 📊");
        summary.AppendLine();
        summary.AppendLine($"🎯 **Score:** {TotalScore}/{MaxScore} ({Percentage:F1}%)");
        summary.AppendLine($"📈 **Grade:** {Grade}");
        summary.AppendLine();

        summary.AppendLine("📝 **Detailed Results:**");

        foreach (var questionResult in QuestionResults)
        {
            var status = questionResult.IsCorrect ? "✅" : "❌";
            summary.AppendLine();
            summary.AppendLine($"{status} **Question {questionResult.QuestionNumber}:** {questionResult.QuestionText}");
            summary.AppendLine($"   Your answer: {questionResult.UserAnswerText}");
            summary.AppendLine($"   Correct answer: {questionResult.CorrectAnswerText}");

            if (!string.IsNullOrEmpty(questionResult.Feedback))
            {
                summary.AppendLine($"   💡 {questionResult.Feedback}");
            }
        }

        summary.AppendLine();
        summary.AppendLine("🏆 **Thank you for taking the quiz!**");

        return summary.ToString().Trim();
    }
}

/// <summary>
/// Represents the result of a single quiz question.
/// </summary>
public sealed record QuestionResult
{
    /// <summary>
    /// Gets the question number (1-based index).
    /// </summary>
    public required int QuestionNumber { get; init; }

    /// <summary>
    /// Gets the question text.
    /// </summary>
    public required string QuestionText { get; init; }

    /// <summary>
    /// Gets the user's selected answer text.
    /// </summary>
    public required string UserAnswerText { get; init; }

    /// <summary>
    /// Gets the correct answer text.
    /// </summary>
    public required string CorrectAnswerText { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user's answer was correct.
    /// </summary>
    public required bool IsCorrect { get; init; }

    /// <summary>
    /// Gets the score awarded for this question.
    /// </summary>
    public required int ScoreAwarded { get; init; }

    /// <summary>
    /// Gets optional feedback text.
    /// </summary>
    public string? Feedback { get; init; }
}

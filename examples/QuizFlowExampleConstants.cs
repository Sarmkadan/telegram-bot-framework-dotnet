#nullable enable

namespace TelegramBotFramework.Examples;

/// <summary>
/// Constants for QuizFlowExample to avoid magic values.
/// </summary>
internal static class QuizFlowExampleConstants
{
    // Quiz identifiers
    public const string SampleQuizId = "sample_quiz";
    public const string SampleQuizName = "General Knowledge Quiz";
    public const string SampleQuizDescription = "Test your general knowledge with 5 multiple choice questions.";
    public const string CompletionMenuId = "main_menu";

    // Quiz internal keys
    public const string QuizScoreVariableKey = "_quiz_score";

    // Quiz answer format
    public const string QuizAnswerFormat = "answer:{0}";

    // Messages
    public const string QuizRegisteredSuccessMessage = "✅ Sample quiz 'General Knowledge Quiz' registered successfully!";
    public const string StartingQuizMessage = "Starting quiz for User {0} in Chat {1}";
    public const string QuizStartedMessage = "Quiz started! State ID: {0}";
    public const string QuestionResultFormat = "Question {0} result - Valid: {1}, Completed: {2}";
    public const string QuizCompletedHeader = "\n📊 **QUIZ COMPLETED!**";
    public const string QuizScoreFormat = "Score: {0}/{1} ({2:F1}%)";
    public const string QuizGradeFormat = "Grade: {0}";
    public const string NoActiveQuizMessage = "No active quiz for this user.";
    public const string ActiveQuizHeader = "User {0} has an active quiz:";
    public const string FlowIdFormat = "- Flow ID: {0}";
    public const string CurrentStepFormat = "- Current Step: {0}";
    public const string StatusFormat = "- Status: {0}";
    public const string StartedAtFormat = "- Started: {0}";
    public const string ScoreFormat = "- Score: {0}";
    public const string QuizAbortedMessage = "Quiz for user {0} aborted.";
    public const string ExampleCleanupReason = "Example cleanup";
}
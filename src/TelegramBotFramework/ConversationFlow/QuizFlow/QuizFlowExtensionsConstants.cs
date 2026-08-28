#nullable enable

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// Constants for QuizFlowExtensions.
/// </summary>
internal static class QuizFlowExtensionsConstants
{
    public const string QuizIdEmptyExceptionMessage = "Quiz ID cannot be empty.";
    public const string QuizNameEmptyExceptionMessage = "Quiz name cannot be empty.";
    public const string WelcomeMessageFormat = "📚 **{0}**\n\nWelcome to the quiz! You will be asked {1} questions.\n\nType /start to begin.";
    public const string QuizCompletedMessage = "🎉 **Quiz Completed!**";
    public const string QuizResultsCalculatingMessage = "Your results are being calculated...";
    public const string ValidationErrorMessageFormat = "Please select a valid option (1-{0})";
    public const string MetadataQuizTypeKey = "QuizType";
    public const string MetadataQuestionCountKey = "QuestionCount";
    public const string QuizTypeValue = "MultipleChoice";
    public const string StepIdStartSuffix = "_start";
    public const string StepIdQuestionPrefixSuffix = "_question_";
    public const string StepIdResultSuffix = "_result";
    public const int ValidationMinLength = 1;
    public const int ValidationMaxLength = 1;
}
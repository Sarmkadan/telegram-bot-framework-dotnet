#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// Constants for <see cref="QuizFlowHelper"/>.
/// </summary>
internal static class QuizFlowHelperConstants
{
    /// <summary>
    /// Log message for adding a question to the quiz.
    /// </summary>
    public const string AddedQuestionLog = "Added question '{QuestionId}' to quiz '{FlowId}'";

    /// <summary>
    /// Log message for disposing the quiz flow helper.
    /// </summary>
    public const string DisposedLog = "QuizFlowHelper '{FlowId}' disposed";
}
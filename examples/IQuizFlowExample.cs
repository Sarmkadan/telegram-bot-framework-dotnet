#nullable enable

namespace TelegramBotFramework.Examples;

/// <summary>
/// Interface for QuizFlowExample.
/// </summary>
public interface IQuizFlowExample
{
    Task RegisterSampleQuizAsync();
    Task RunQuizExampleAsync(long userId, long chatId);
    Task CheckQuizStatusAsync(long userId);
    Task AbortQuizExampleAsync(long userId);
}
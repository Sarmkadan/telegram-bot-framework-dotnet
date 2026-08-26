#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents the execution context for a command or operation.
/// </summary>
public interface IExecutionContext
{
    string ContextId { get; set; }

    long UserId { get; set; }

    long ChatId { get; set; }

    BotUser? User { get; set; }

    UserSession? Session { get; set; }

    Command? Command { get; set; }

    Message? Message { get; set; }

    Dictionary<string, object>? Parameters { get; set; }

    DateTime CreatedAt { get; set; }

    Dictionary<string, object> States { get; set; }

    List<string>? Errors { get; set; }

    bool IsValid { get; set; }

    T? GetParameter<T>(string key);

    void SetParameter(string key, object value);

    T? GetState<T>(string key);

    void SetState(string? key, object value);

    string? PendingResponse { get; }

    bool IsStopped { get; }

    void RespondAndStop(string responseMessage);

    void StopProcessing();
}
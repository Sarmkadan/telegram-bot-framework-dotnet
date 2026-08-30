#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Constants for IBotOrchestrator.
/// </summary>
internal static class IBotOrchestratorConstants
{
    public const char CommandPrefix = '/';
    public const long UnknownChatId = 0;
    public const string ErrorSeparator = "; ";
    public const string CommandNotFoundFormat = "Command '{0}' not found";
    public const string MenuNotFoundFormat = "Menu '{0}' not found";
    public const string NoActiveSessionFormat = "No active session for user {0}";
    public const string MessageProcessedLogTemplate =
        "Message processed - UserId: {UserId}, ContextId: {ContextId}, IsValid: {IsValid}";
    public const string CommandExecutedLogTemplate =
        "Command executed - UserId: {UserId}, Command: {Command}, IsValid: {IsValid}";
    public const string MenuDisplayedLogTemplate = "Menu displayed - UserId: {UserId}, MenuId: {MenuId}";
    public const string ButtonNotFoundLogTemplate =
        "Button not found - MenuId: {MenuId}, CallbackData: {CallbackData}";
    public const string UnknownButtonActionLogTemplate = "Unknown button action - Action: {Action}";
    public const string ButtonHandledLogTemplate =
        "Button handled - UserId: {UserId}, CallbackData: {CallbackData}";
    public const string SessionEndedLogTemplate =
        "Session ended - UserId: {UserId}, SessionId: {SessionId}";
}

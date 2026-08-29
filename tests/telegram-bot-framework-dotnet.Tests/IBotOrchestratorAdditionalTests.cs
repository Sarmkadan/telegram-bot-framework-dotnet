#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for additional test cases for the <see cref="BotOrchestrator"/> class.
/// Contains additional test methods for edge cases, boundary conditions, and specific scenarios.
/// </summary>
public interface IBotOrchestratorAdditionalTests
{
    Task ProcessUserMessageAsync_WithEmptyMessageContent_AddsErrorToContext();
    Task ProcessUserMessageAsync_WithNullLastName_ProcessesSuccessfully();
    Task ProcessUserMessageAsync_WithVeryLongMessageContent_ProcessesSuccessfully();
    Task ExecuteUserCommandAsync_WithParameters_StoresParametersInContext();
    Task ExecuteUserCommandAsync_WithNonExistentCommand_AddsErrorToContext();
    Task DisplayMenuAsync_WithNullSession_DoesNotThrow();
    Task HandleMenuButtonAsync_WithOpenUrlAction_DoesNotThrow();
    Task HandleMenuButtonAsync_WithSwitchInlineAction_DoesNotThrow();
    Task GetUserSessionAsync_WithNoActiveSession_ThrowsSessionException();
    Task EndUserSessionAsync_WithNoActiveSession_ReturnsFalse();
    void ExtractCommandName_WithMultipleSpaces_ReturnsCommandName();
    void ExtractCommandName_WithLeadingAndTrailingSpaces_ReturnsCommandName();
    void ExtractCommandName_WithTabCharacters_ReturnsCommandName();
}
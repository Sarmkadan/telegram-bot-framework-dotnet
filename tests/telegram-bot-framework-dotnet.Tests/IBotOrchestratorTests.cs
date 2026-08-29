#nullable enable

using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Defines the contract for BotOrchestratorTests.
/// </summary>
public interface IBotOrchestratorTests
{
    void Constructor_WithNullUserService_ThrowsArgumentNullException();
    void Constructor_WithNullCommandService_ThrowsArgumentNullException();
    void Constructor_WithNullSessionService_ThrowsArgumentNullException();
    void Constructor_WithNullMessageService_ThrowsArgumentNullException();
    void Constructor_WithNullMenuService_ThrowsArgumentNullException();
    void Constructor_WithNullMiddlewares_ThrowsArgumentNullException();
    void Constructor_WithNullConfiguration_ThrowsArgumentNullException();
    void Constructor_WithNullLogger_ThrowsArgumentNullException();
    Task ProcessUserMessageAsync_WithValidMessage_ReturnsValidContext();
    Task ProcessUserMessageAsync_WithCommandMessage_ExtractsCommand();
    Task ProcessUserMessageAsync_WithInvalidMessage_MarksAsFailed();
    Task ExecuteUserCommandAsync_WithValidCommand_ReturnsValidContext();
    Task ExecuteUserCommandAsync_WithNonExistentCommand_ReturnsContextWithError();
    Task DisplayMenuAsync_WithValidMenuId_ReturnsMenu();
    Task DisplayMenuAsync_WithNonExistentMenu_ThrowsInvalidOperationException();
    Task HandleMenuButtonAsync_WithExecuteCommandButton_ExecutesCommand();
    Task HandleMenuButtonAsync_WithNavigateMenuButton_NavigatesToMenu();
    Task HandleMenuButtonAsync_WithUnknownButtonAction_ReturnsFalse();
    Task GetUserSessionAsync_WithActiveSession_ReturnsSession();
}
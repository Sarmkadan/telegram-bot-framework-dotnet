#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using Xunit;
using ExecutionContext = TelegramBotFramework.Models.ExecutionContext;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Contains additional test cases for the <see cref="BotOrchestrator"/> class.
/// Tests edge cases, boundary conditions, and specific scenarios not covered in the main test suite.
/// </summary>
public sealed class BotOrchestratorAdditionalTests : IBotOrchestratorAdditionalTests
{
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<ICommandService> _mockCommandService = new();
    private readonly Mock<ISessionService> _mockSessionService = new();
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IMenuService> _mockMenuService = new();
    private readonly Mock<ILogger<BotOrchestrator>> _mockLogger = new();
    private readonly Mock<Middleware.IBotMiddleware> _mockMiddleware = new();
    private readonly BotConfiguration _configuration = new()
    {
        BotToken = "test-token",
        BotUsername = "TestBot"
    };
    private readonly BotOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotOrchestratorAdditionalTests"/> class.
    /// Sets up mock dependencies and creates a <see cref="BotOrchestrator"/> instance for testing.
    /// </summary>
    public BotOrchestratorAdditionalTests()
    {
        var middlewares = new List<Middleware.IBotMiddleware> { _mockMiddleware.Object };

        _orchestrator = new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            _mockSessionService.Object,
            _mockMessageService.Object,
            _mockMenuService.Object,
            middlewares,
            _configuration,
            _mockLogger.Object);
    }

    /// <summary>
    /// Tests that processing a user message with empty content adds an error to the execution context.
    /// Verifies that the orchestrator properly handles empty message content by marking the message as failed
    /// and adding an appropriate error message to the context.
    /// </summary>
    [Fact]
    public async Task ProcessUserMessageAsync_WithEmptyMessageContent_AddsErrorToContext()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "", Type = MessageType.Text };
        var processedMessage = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "", Type = MessageType.Text };

        _mockUserService.Setup(s => s.GetOrCreateUserAsync(123, "John", "Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionService.Setup(s => s.CreateSessionAsync(123, 456, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockUserService.Setup(s => s.RecordUserActivityAsync(123, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSessionService.Setup(s => s.RecordSessionActivityAsync("session-123", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMessageService.Setup(s => s.ProcessIncomingMessageAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processedMessage);
        _mockMessageService.Setup(s => s.MarkAsFailedAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMiddleware.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) =>
            {
                if (!string.IsNullOrEmpty(ctx.Message?.Content))
                {
                    return next(ctx);
                }
                ctx.AddError("Message content is empty");
                return Task.FromResult(ctx);
            });

        // Act
        var result = await _orchestrator.ProcessUserMessageAsync(123, 456, "", "John", "Doe").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("empty"));
        _mockMessageService.Verify(s => s.MarkAsFailedAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that processing a user message with a null last name processes successfully.
    /// Verifies that the orchestrator can handle messages where the user's last name is null,
    /// which should not cause any exceptions and should process the message normally.
    /// </summary>
    [Fact]
    public async Task ProcessUserMessageAsync_WithNullLastName_ProcessesSuccessfully()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "Hello", Type = MessageType.Text };
        var processedMessage = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "Hello", Type = MessageType.Text };

        _mockUserService.Setup(s => s.GetOrCreateUserAsync(123, "John", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionService.Setup(s => s.CreateSessionAsync(123, 456, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockUserService.Setup(s => s.RecordUserActivityAsync(123, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSessionService.Setup(s => s.RecordSessionActivityAsync("session-123", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMessageService.Setup(s => s.ProcessIncomingMessageAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processedMessage);
        _mockMessageService.Setup(s => s.MarkAsProcessedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMiddleware.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.ProcessUserMessageAsync(123, 456, "Hello", "John").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(123);
        result.ChatId.Should().Be(456);
        result.User.Should().Be(user);
        result.Session.Should().Be(session);
        result.Message.Should().Be(processedMessage);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that processing a user message with very long content (4000 characters) processes successfully.
    /// Verifies that the orchestrator can handle messages with maximum allowed content length without errors.
    /// </summary>
    [Fact]
    public async Task ProcessUserMessageAsync_WithVeryLongMessageContent_ProcessesSuccessfully()
    {
        // Arrange
        var longMessage = new string('x', 4000);
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = longMessage, Type = MessageType.Text };
        var processedMessage = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = longMessage, Type = MessageType.Text };

        _mockUserService.Setup(s => s.GetOrCreateUserAsync(123, "John", "Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionService.Setup(s => s.CreateSessionAsync(123, 456, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockUserService.Setup(s => s.RecordUserActivityAsync(123, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSessionService.Setup(s => s.RecordSessionActivityAsync("session-123", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMessageService.Setup(s => s.ProcessIncomingMessageAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processedMessage);
        _mockMessageService.Setup(s => s.MarkAsProcessedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMiddleware.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.ProcessUserMessageAsync(123, 456, longMessage, "John", "Doe").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that executing a user command with parameters stores the parameters in the execution context.
    /// Verifies that command parameters are properly extracted and stored in the context for later use by command handlers.
    /// </summary>
    [Fact]
    public async Task ExecuteUserCommandAsync_WithParameters_StoresParametersInContext()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var command = new Command { Name = "/test", IsEnabled = true, HandlerType = "TestHandler" };
        var parameters = new Dictionary<string, object> { { "param1", "value1" }, { "param2", 123 } };

        _mockUserService.Setup(s => s.GetUserByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockCommandService.Setup(s => s.GetCommandAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);
        _mockCommandService.Setup(s => s.RecordCommandExecutionAsync("test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMiddleware.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.ExecuteUserCommandAsync(123, 456, "test", parameters).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Parameters.Should().NotBeNull();
        result.Parameters.Should().HaveCount(2);
        result.Parameters.Should().ContainKey("param1").WhoseValue.Should().Be("value1");
        result.Parameters.Should().ContainKey("param2").WhoseValue.Should().Be(123);
    }

    /// <summary>
    /// Tests that executing a user command with a non-existent command adds an error to the context.
    /// Verifies that attempting to execute a command that doesn't exist results in an appropriate error message
    /// and the execution context is marked as invalid.
    /// </summary>
    [Fact]
    public async Task ExecuteUserCommandAsync_WithNonExistentCommand_AddsErrorToContext()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };

        _mockUserService.Setup(s => s.GetUserByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockCommandService.Setup(s => s.GetCommandAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act
        var result = await _orchestrator.ExecuteUserCommandAsync(123, 456, "nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    /// <summary>
    /// Tests that displaying a menu with a null session does not throw an exception.
    /// Verifies that the orchestrator can handle cases where there is no active session for the user
    /// without throwing exceptions.
    /// </summary>
    [Fact]
    public async Task DisplayMenuAsync_WithNullSession_DoesNotThrow()
    {
        // Arrange
        var menu = new Menu { MenuId = "main", Title = "Main Menu", Buttons = new List<MenuButton>() };

        _mockMenuService.Setup(s => s.GetMenuAsync("main", It.IsAny<CancellationToken>()))
            .ReturnsAsync(menu);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _orchestrator.DisplayMenuAsync(123, "main").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.MenuId.Should().Be("main");
    }

    /// <summary>
    /// Tests that handling a menu button with an OpenUrl action does not throw an exception.
    /// Verifies that the orchestrator can process menu buttons configured with URL actions without errors.
    /// </summary>
    [Fact]
    public async Task HandleMenuButtonAsync_WithOpenUrlAction_DoesNotThrow()
    {
        // Arrange
        var button = new MenuButton
        {
            CallbackData = "https://example.com",
            Action = ButtonAction.OpenUrl
        };

        _mockMenuService.Setup(s => s.GetButtonAsync("main", "https://example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(button);

        // Act
        var result = await _orchestrator.HandleMenuButtonAsync(123, "main", "https://example.com").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that handling a menu button with a SwitchInline action does not throw an exception.
    /// Verifies that the orchestrator can process menu buttons configured with inline query actions without errors.
    /// </summary>
    [Fact]
    public async Task HandleMenuButtonAsync_WithSwitchInlineAction_DoesNotThrow()
    {
        // Arrange
        var button = new MenuButton
        {
            CallbackData = "inline_query",
            Action = ButtonAction.SwitchInline
        };

        _mockMenuService.Setup(s => s.GetButtonAsync("main", "inline_query", It.IsAny<CancellationToken>()))
            .ReturnsAsync(button);

        // Act
        var result = await _orchestrator.HandleMenuButtonAsync(123, "main", "inline_query").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that getting a user session with no active session throws a SessionException.
    /// Verifies that attempting to retrieve a session when none exists results in the appropriate exception.
    /// </summary>
    [Fact]
    public async Task GetUserSessionAsync_WithNoActiveSession_ThrowsSessionException()
    {
        // Arrange
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act & Assert
        await _orchestrator.Invoking(o => o.GetUserSessionAsync(123))
            .Should().ThrowAsync<Exceptions.SessionException>();
    }

    /// <summary>
    /// Tests that ending a user session with no active session returns false.
    /// Verifies that attempting to end a session when none exists gracefully returns false instead of throwing.
    /// </summary>
    [Fact]
    public async Task EndUserSessionAsync_WithNoActiveSession_ReturnsFalse()
    {
        // Arrange
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _orchestrator.EndUserSessionAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that extracting a command name with multiple spaces returns the correct command name.
    /// Verifies that the ExtractCommandName method properly handles messages with multiple spaces between
    /// the command and its parameters.
    /// <para><param name="messageContent">The message content containing the command with parameters</param></para>
    /// <returns>The extracted command name without parameters</returns>
    /// </summary>
    [Fact]
    public void ExtractCommandName_WithMultipleSpaces_ReturnsCommandName()
    {
        // Arrange
        var messageContent = "/start param1 param2";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().Be("start");
    }

    /// <summary>
    /// Tests that extracting a command name with leading and trailing spaces returns the correct command name.
    /// Verifies that the ExtractCommandName method properly trims whitespace from the message content
    /// before extracting the command name.
    /// <para><param name="messageContent">The message content with leading and trailing spaces</param></para>
    /// <returns>The extracted command name without surrounding whitespace</returns>
    /// </summary>
    [Fact]
    public void ExtractCommandName_WithLeadingAndTrailingSpaces_ReturnsCommandName()
    {
        // Arrange
        var messageContent = " /start ";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().Be("start");
    }

    /// <summary>
    /// Tests that extracting a command name with tab characters returns the correct command name.
    /// Verifies that the ExtractCommandName method properly handles messages containing tab characters
    /// between the command and its parameters.
    /// <para><param name="messageContent">The message content containing a tab character</param></para>
    /// <returns>The extracted command name without tab characters</returns>
    /// </summary>
    [Fact]
    public void ExtractCommandName_WithTabCharacters_ReturnsCommandName()
    {
        // Arrange
        var messageContent = "/start\tparam1";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().Be("start");
    }
}
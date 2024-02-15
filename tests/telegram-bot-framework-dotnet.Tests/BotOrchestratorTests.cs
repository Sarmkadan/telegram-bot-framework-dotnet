#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using Xunit;
using ExecutionContext = TelegramBotFramework.Models.ExecutionContext;

namespace TelegramBotFramework.Tests;

public sealed class BotOrchestratorTests
{
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<ICommandService> _mockCommandService = new();
    private readonly Mock<ISessionService> _mockSessionService = new();
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IMenuService> _mockMenuService = new();
    private readonly Mock<ILogger<BotOrchestrator>> _mockLogger = new();
    private readonly Mock<Middleware.IBotMiddleware> _mockMiddleware1 = new();
    private readonly Mock<Middleware.IBotMiddleware> _mockMiddleware2 = new();
    private readonly BotConfiguration _configuration = new()
    {
        BotToken = "test-token",
        BotUsername = "TestBot"
    };
    private readonly BotOrchestrator _orchestrator;

    public BotOrchestratorTests()
    {
        var middlewares = new List<Middleware.IBotMiddleware> { _mockMiddleware1.Object, _mockMiddleware2.Object };

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

    [Fact]
    public void Constructor_WithNullUserService_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            null!,
            _mockCommandService.Object,
            _mockSessionService.Object,
            _mockMessageService.Object,
            _mockMenuService.Object,
            middlewares,
            _configuration,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullCommandService_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            null!,
            _mockSessionService.Object,
            _mockMessageService.Object,
            _mockMenuService.Object,
            middlewares,
            _configuration,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("commandService");
    }

    [Fact]
    public void Constructor_WithNullSessionService_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            null!,
            _mockMessageService.Object,
            _mockMenuService.Object,
            middlewares,
            _configuration,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("sessionService");
    }

    [Fact]
    public void Constructor_WithNullMessageService_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            _mockSessionService.Object,
            null!,
            _mockMenuService.Object,
            middlewares,
            _configuration,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("messageService");
    }

    [Fact]
    public void Constructor_WithNullMenuService_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            _mockSessionService.Object,
            _mockMessageService.Object,
            null!,
            middlewares,
            _configuration,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("menuService");
    }

    [Fact]
    public void Constructor_WithNullMiddlewares_ThrowsArgumentNullException()
    {
        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            _mockSessionService.Object,
            _mockMessageService.Object,
            _mockMenuService.Object,
            null!,
            _configuration,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("middleware");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            _mockSessionService.Object,
            _mockMessageService.Object,
            _mockMenuService.Object,
            middlewares,
            null!,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var middlewares = new List<Middleware.IBotMiddleware>();

        Action act = () => new BotOrchestrator(
            _mockUserService.Object,
            _mockCommandService.Object,
            _mockSessionService.Object,
            _mockMessageService.Object,
            _mockMenuService.Object,
            middlewares,
            _configuration,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task ProcessUserMessageAsync_WithValidMessage_ReturnsValidContext()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "Hello", Type = MessageType.Text };
        var processedMessage = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "Hello", Type = MessageType.Text };

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

        _mockMiddleware1.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));
        _mockMiddleware2.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.ProcessUserMessageAsync(123, 456, "Hello", "John", "Doe");

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(123);
        result.ChatId.Should().Be(456);
        result.User.Should().Be(user);
        result.Session.Should().Be(session);
        result.Message.Should().Be(processedMessage);
        result.IsValid.Should().BeTrue();

        _mockMessageService.Verify(s => s.MarkAsProcessedAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessUserMessageAsync_WithCommandMessage_ExtractsCommand()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var command = new Command { Name = "/start", IsEnabled = true };
        var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "/start", Type = MessageType.Text };
        var processedMessage = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "/start", Type = MessageType.Text };

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
        _mockCommandService.Setup(s => s.GetCommandAsync("start", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);
        _mockMessageService.Setup(s => s.MarkAsProcessedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMiddleware1.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));
        _mockMiddleware2.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.ProcessUserMessageAsync(123, 456, "/start", "John", "Doe");

        // Assert
        result.Command.Should().NotBeNull();
        result.Command!.Name.Should().Be("/start");
    }

    [Fact]
    public async Task ProcessUserMessageAsync_WithInvalidMessage_MarksAsFailed()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = false };
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

        var contextWithErrors = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = user,
            Session = session,
            Message = processedMessage,
            Errors = new List<string> { "Validation failed" },
            IsValid = false
        };

        _mockMiddleware1.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contextWithErrors);

        _mockMessageService.Setup(s => s.MarkAsFailedAsync(1, "Validation failed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _orchestrator.ProcessUserMessageAsync(123, 456, "", "John", "Doe");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Validation failed");
        _mockMessageService.Verify(s => s.MarkAsFailedAsync(1, "Validation failed", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteUserCommandAsync_WithValidCommand_ReturnsValidContext()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
        var command = new Command { Name = "/test", IsEnabled = true };

        _mockUserService.Setup(s => s.GetUserByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockCommandService.Setup(s => s.GetCommandAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);
        _mockCommandService.Setup(s => s.RecordCommandExecutionAsync("test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMiddleware1.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));
        _mockMiddleware2.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.ExecuteUserCommandAsync(123, 456, "test");

        // Assert
        result.Should().NotBeNull();
        result.Command.Should().NotBeNull();
        result.Command!.Name.Should().Be("/test");
        result.IsValid.Should().BeTrue();
        _mockCommandService.Verify(s => s.RecordCommandExecutionAsync("test", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteUserCommandAsync_WithNonExistentCommand_ReturnsContextWithError()
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
        var result = await _orchestrator.ExecuteUserCommandAsync(123, 456, "nonexistent");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Command 'nonexistent' not found");
    }

    [Fact]
    public async Task DisplayMenuAsync_WithValidMenuId_ReturnsMenu()
    {
        // Arrange
        var menu = new Menu { MenuId = "main", Title = "Main Menu", Buttons = new List<MenuButton>() };

        _mockMenuService.Setup(s => s.GetMenuAsync("main", It.IsAny<CancellationToken>()))
            .ReturnsAsync(menu);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true });
        _mockSessionService.Setup(s => s.NavigateToMenuAsync("session-123", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true });

        // Act
        var result = await _orchestrator.DisplayMenuAsync(123, "main");

        // Assert
        result.Should().NotBeNull();
        result.MenuId.Should().Be("main");
        _mockSessionService.Verify(s => s.NavigateToMenuAsync("session-123", "main", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisplayMenuAsync_WithNonExistentMenu_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockMenuService.Setup(s => s.GetMenuAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await _orchestrator.Invoking(o => o.DisplayMenuAsync(123, "nonexistent"))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleMenuButtonAsync_WithExecuteCommandButton_ExecutesCommand()
    {
        // Arrange
        var button = new MenuButton
        {
            CallbackData = "/start",
            Action = ButtonAction.ExecuteCommand
        };

        _mockMenuService.Setup(s => s.GetButtonAsync("main", "/start", It.IsAny<CancellationToken>()))
            .ReturnsAsync(button);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true });

        _mockCommandService.Setup(s => s.GetCommandAsync("start", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Command { Name = "/start", IsEnabled = true });
        _mockCommandService.Setup(s => s.RecordCommandExecutionAsync("start", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMiddleware1.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));
        _mockMiddleware2.Setup(m => m.ProcessAsync(It.IsAny<ExecutionContext>(), It.IsAny<Func<ExecutionContext, Task<ExecutionContext>>>(), It.IsAny<CancellationToken>()))
            .Returns((ExecutionContext ctx, Func<ExecutionContext, Task<ExecutionContext>> next, CancellationToken ct) => next(ctx));

        // Act
        var result = await _orchestrator.HandleMenuButtonAsync(123, "main", "/start");

        // Assert
        result.Should().BeTrue();
        _mockCommandService.Verify(s => s.RecordCommandExecutionAsync("start", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleMenuButtonAsync_WithNavigateMenuButton_NavigatesToMenu()
    {
        // Arrange
        var button = new MenuButton
        {
            CallbackData = "submenu",
            Action = ButtonAction.NavigateMenu
        };

        _mockMenuService.Setup(s => s.GetButtonAsync("main", "submenu", It.IsAny<CancellationToken>()))
            .ReturnsAsync(button);
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true });
        _mockMenuService.Setup(s => s.GetMenuAsync("submenu", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Menu { MenuId = "submenu", Title = "Sub Menu", Buttons = new List<MenuButton>() });
        _mockSessionService.Setup(s => s.NavigateToMenuAsync("session-123", "submenu", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true, CurrentMenuId = "submenu" });

        // Act
        var result = await _orchestrator.HandleMenuButtonAsync(123, "main", "submenu");

        // Assert
        result.Should().BeTrue();
        _mockSessionService.Verify(s => s.NavigateToMenuAsync("session-123", "submenu", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleMenuButtonAsync_WithUnknownButtonAction_ReturnsFalse()
    {
        // Arrange
        var button = new MenuButton
        {
            CallbackData = "unknown",
            Action = (ButtonAction)999 // Unknown action
        };

        _mockMenuService.Setup(s => s.GetButtonAsync("main", "unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(button);

        // Act
        var result = await _orchestrator.HandleMenuButtonAsync(123, "main", "unknown");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserSessionAsync_WithActiveSession_ReturnsSession()
    {
        // Arrange
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };

        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _orchestrator.GetUserSessionAsync(123);

        // Assert
        result.Should().Be(session);
    }

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

    [Fact]
    public async Task EndUserSessionAsync_WithActiveSession_ClosesSession()
    {
        // Arrange
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };

        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionService.Setup(s => s.CloseSessionAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _orchestrator.EndUserSessionAsync(123);

        // Assert
        result.Should().BeTrue();
        _mockSessionService.Verify(s => s.CloseSessionAsync("session-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndUserSessionAsync_WithNoActiveSession_ReturnsFalse()
    {
        // Arrange
        _mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _orchestrator.EndUserSessionAsync(123);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ExtractCommandName_WithCommand_ReturnsCommandName()
    {
        // Arrange
        var messageContent = "/start param1 param2";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().Be("start");
    }

    [Fact]
    public void ExtractCommandName_WithCommandOnly_ReturnsCommandName()
    {
        // Arrange
        var messageContent = "/start";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().Be("start");
    }

    [Fact]
    public void ExtractCommandName_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var messageContent = "";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractCommandName_WithNoSlash_ReturnsEmptyString()
    {
        // Arrange
        var messageContent = "start";

        // Act
        var result = BotOrchestrator.ExtractCommandName(messageContent);

        // Assert
        result.Should().BeEmpty();
    }
}

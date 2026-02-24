#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// High-level orchestrator for bot operations, coordinating multiple services.
/// </summary>
public interface IBotOrchestrator
{
    Task<Models.ExecutionContext> ProcessUserMessageAsync(
        long userId,
        long chatId,
        string content,
        string firstName,
        string? lastName = null,
        CancellationToken cancellationToken = default);

    Task<Models.ExecutionContext> ExecuteUserCommandAsync(
        long userId,
        long chatId,
        string commandName,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    Task<Models.Menu> DisplayMenuAsync(
        long userId,
        string menuId,
        CancellationToken cancellationToken = default);

    Task<bool> HandleMenuButtonAsync(
        long userId,
        string menuId,
        string buttonCallbackData,
        CancellationToken cancellationToken = default);

    Task<Models.UserSession> GetUserSessionAsync(long userId, CancellationToken cancellationToken = default);

    Task<bool> EndUserSessionAsync(long userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of bot orchestrator.
/// </summary>
public sealed class BotOrchestrator : IBotOrchestrator
{
    private readonly IUserService _userService;
    private readonly ICommandService _commandService;
    private readonly ISessionService _sessionService;
    private readonly IMessageService _messageService;
    private readonly IMenuService _menuService;
    private readonly Microsoft.Extensions.Logging.ILogger<BotOrchestrator> _logger;
    private readonly List<Middleware.IBotMiddleware> _middleware;

    public BotOrchestrator(
        IUserService userService,
        ICommandService commandService,
        ISessionService sessionService,
        IMessageService messageService,
        IMenuService menuService,
        Microsoft.Extensions.Logging.ILogger<BotOrchestrator> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize middleware pipeline (ordered by priority)
        _middleware = new List<Middleware.IBotMiddleware>
        {
            new Middleware.ErrorHandlingMiddleware(logger),
            new Middleware.LoggingMiddleware(logger),
            new Middleware.AuthorizationMiddleware(userService, commandService, logger),
            new Middleware.RateLimitMiddleware(commandService, new Models.BotConfiguration(), logger)
        };
    }

    public async Task<Models.ExecutionContext> ProcessUserMessageAsync(
        long userId,
        long chatId,
        string content,
        string firstName,
        string? lastName = null,
        CancellationToken cancellationToken = default)
    {
        // Get or create user
        var user = await _userService.GetOrCreateUserAsync(userId, firstName, lastName, cancellationToken);

        // Get or create session
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        session ??= await _sessionService.CreateSessionAsync(userId, chatId, cancellationToken);

        // Record activity
        await _userService.RecordUserActivityAsync(userId, cancellationToken);
        await _sessionService.RecordSessionActivityAsync(session.SessionId, cancellationToken);

        // Process message
        var message = new Models.Message
        {
            UserId = userId,
            ChatId = chatId,
            Content = content,
            Type = Models.MessageType.Text,
            CreatedAt = DateTime.UtcNow
        };

        message.Validate();
        var processedMessage = await _messageService.ProcessIncomingMessageAsync(message, cancellationToken);

        // Create context
        var context = new Models.ExecutionContext
        {
            UserId = userId,
            ChatId = chatId,
            User = user,
            Session = session,
            Message = processedMessage,
            CreatedAt = DateTime.UtcNow
        };

        // Check if message is a command
        if (content.StartsWith(Constants.BotConstants.CommandPrefix))
        {
            var commandName = ExtractCommandName(content);
            var command = await _commandService.GetCommandAsync(commandName, cancellationToken);
            if (command  is not null)
            {
                context.Command = command;
            }
        }

        context.Validate();

        // Process through middleware pipeline
        var finalContext = await ExecuteMiddlewarePipelineAsync(context, cancellationToken);

        if (finalContext.IsValid)
        {
            await _messageService.MarkAsProcessedAsync(processedMessage.MessageId, cancellationToken);
        }
        else if (finalContext.Errors?.Count > 0)
        {
            await _messageService.MarkAsFailedAsync(
                processedMessage.MessageId,
                string.Join("; ", finalContext.Errors),
                cancellationToken);
        }

        _logger.LogInformation(
            "Message processed - UserId: {UserId}, ContextId: {ContextId}, IsValid: {IsValid}",
            userId, finalContext.ContextId, finalContext.IsValid);

        return finalContext;
    }

    public async Task<Models.ExecutionContext> ExecuteUserCommandAsync(
        long userId,
        long chatId,
        string commandName,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        var command = await _commandService.GetCommandAsync(commandName, cancellationToken);

        var context = new Models.ExecutionContext
        {
            UserId = userId,
            ChatId = chatId,
            User = user,
            Session = session,
            Command = command,
            Parameters = parameters,
            CreatedAt = DateTime.UtcNow
        };

        context.Validate();

        if (context.Command  is null)
        {
            context.AddError($"Command '{commandName}' not found");
            return context;
        }

        context = await ExecuteMiddlewarePipelineAsync(context, cancellationToken);

        if (context.IsValid)
        {
            await _commandService.RecordCommandExecutionAsync(commandName, cancellationToken);
        }

        _logger.LogInformation(
            "Command executed - UserId: {UserId}, Command: {Command}, IsValid: {IsValid}",
            userId, commandName, context.IsValid);

        return context;
    }

    public async Task<Models.Menu> DisplayMenuAsync(
        long userId,
        string menuId,
        CancellationToken cancellationToken = default)
    {
        var menu = await _menuService.GetMenuAsync(menuId, cancellationToken);
        if (menu  is null)
        {
            throw new InvalidOperationException($"Menu '{menuId}' not found");
        }

        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        if (session  is not null)
        {
            await _sessionService.NavigateToMenuAsync(session.SessionId, menuId, cancellationToken);
        }

        _logger.LogInformation("Menu displayed - UserId: {UserId}, MenuId: {MenuId}", userId, menuId);
        return menu;
    }

    public async Task<bool> HandleMenuButtonAsync(
        long userId,
        string menuId,
        string buttonCallbackData,
        CancellationToken cancellationToken = default)
    {
        var button = await _menuService.GetButtonAsync(menuId, buttonCallbackData, cancellationToken);
        if (button  is null)
        {
            _logger.LogWarning("Button not found - MenuId: {MenuId}, CallbackData: {CallbackData}", menuId, buttonCallbackData);
            return false;
        }

        switch (button.Action)
        {
            case Models.ButtonAction.ExecuteCommand:
                await ExecuteUserCommandAsync(userId, 0, buttonCallbackData, null, cancellationToken);
                break;

            case Models.ButtonAction.NavigateMenu:
                await DisplayMenuAsync(userId, buttonCallbackData, cancellationToken);
                break;

            case Models.ButtonAction.OpenUrl:
                // URL handling would be done at the presentation layer
                break;

            case Models.ButtonAction.SwitchInline:
                // Inline mode handling
                break;

            default:
                _logger.LogWarning("Unknown button action - Action: {Action}", button.Action);
                return false;
        }

        _logger.LogInformation("Button handled - UserId: {UserId}, CallbackData: {CallbackData}", userId, buttonCallbackData);
        return true;
    }

    public async Task<Models.UserSession> GetUserSessionAsync(long userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        if (session  is null)
        {
            throw new Exceptions.SessionException($"No active session for user {userId}");
        }

        return session;
    }

    public async Task<bool> EndUserSessionAsync(long userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
        if (session  is null)
        {
            return false;
        }

        var result = await _sessionService.CloseSessionAsync(session.SessionId, cancellationToken);
        if (result)
        {
            _logger.LogInformation("Session ended - UserId: {UserId}, SessionId: {SessionId}", userId, session.SessionId);
        }

        return result;
    }

    /// <summary>
    /// Executes the middleware pipeline.
    /// </summary>
    private async Task<Models.ExecutionContext> ExecuteMiddlewarePipelineAsync(
        Models.ExecutionContext context,
        CancellationToken cancellationToken)
    {
        var sortedMiddleware = _middleware.OrderByDescending(m => m.Priority).ToList();

        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> executeNext = null!;
        executeNext = async (ctx) =>
        {
            if (sortedMiddleware.Count == 0)
                return ctx;

            var middleware = sortedMiddleware.First();
            sortedMiddleware.RemoveAt(0);
            return await middleware.ProcessAsync(ctx, executeNext, cancellationToken);
        };

        return await executeNext(context);
    }

    /// <summary>
    /// Extracts command name from message.
    /// </summary>
    private static string ExtractCommandName(string messageContent)
    {
        var parts = messageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0].TrimStart('/') : string.Empty;
    }
}
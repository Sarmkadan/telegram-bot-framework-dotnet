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
    private readonly IEnumerable<Middleware.IBotMiddleware> _middleware;
    private readonly Models.BotConfiguration _configuration;

    public BotOrchestrator(
        IUserService userService,
        ICommandService commandService,
        ISessionService sessionService,
        IMessageService messageService,
        IMenuService menuService,
        IEnumerable<Middleware.IBotMiddleware> middleware,
        Models.BotConfiguration configuration,
        Microsoft.Extensions.Logging.ILogger<BotOrchestrator> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _middleware = middleware?.OrderByDescending(m => m.Priority).ToList() ?? throw new ArgumentNullException(nameof(middleware));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Models.ExecutionContext> ProcessUserMessageAsync(
        long userId,
        long chatId,
        string content,
        string firstName,
        string? lastName = null,
        CancellationToken cancellationToken = default)
    {
        // Get or create user
        var user = await _userService.GetOrCreateUserAsync(userId, firstName, lastName, cancellationToken).ConfigureAwait(false);

        // Get or create session
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        session ??= await _sessionService.CreateSessionAsync(userId, chatId, cancellationToken).ConfigureAwait(false);

        // Record activity
        await _userService.RecordUserActivityAsync(userId, cancellationToken).ConfigureAwait(false);
        await _sessionService.RecordSessionActivityAsync(session.SessionId, cancellationToken).ConfigureAwait(false);

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
        var processedMessage = await _messageService.ProcessIncomingMessageAsync(message, cancellationToken).ConfigureAwait(false);

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
            var command = await _commandService.GetCommandAsync(commandName, cancellationToken).ConfigureAwait(false);
            if (command  is not null)
            {
                context.Command = command;
            }
        }

        context.Validate();

        // Process through middleware pipeline
        var finalContext = await ExecuteMiddlewarePipelineAsync(context, cancellationToken).ConfigureAwait(false);

        if (finalContext.IsValid)
        {
            await _messageService.MarkAsProcessedAsync(processedMessage.MessageId, cancellationToken).ConfigureAwait(false);
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
        var user = await _userService.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        var command = await _commandService.GetCommandAsync(commandName, cancellationToken).ConfigureAwait(false);

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

        context = await ExecuteMiddlewarePipelineAsync(context, cancellationToken).ConfigureAwait(false);

        if (context.IsValid)
        {
            await _commandService.RecordCommandExecutionAsync(commandName, cancellationToken).ConfigureAwait(false);
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
        var menu = await _menuService.GetMenuAsync(menuId, cancellationToken).ConfigureAwait(false);
        if (menu  is null)
        {
            throw new InvalidOperationException($"Menu '{menuId}' not found");
        }

        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        if (session  is not null)
        {
            await _sessionService.NavigateToMenuAsync(session.SessionId, menuId, cancellationToken).ConfigureAwait(false);
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
        var button = await _menuService.GetButtonAsync(menuId, buttonCallbackData, cancellationToken).ConfigureAwait(false);
        if (button  is null)
        {
            _logger.LogWarning("Button not found - MenuId: {MenuId}, CallbackData: {CallbackData}", menuId, buttonCallbackData);
            return false;
        }

        switch (button.Action)
        {
            case Models.ButtonAction.ExecuteCommand:
                await ExecuteUserCommandAsync(userId, 0, buttonCallbackData, null, cancellationToken).ConfigureAwait(false);
                break;

            case Models.ButtonAction.NavigateMenu:
                await DisplayMenuAsync(userId, buttonCallbackData, cancellationToken).ConfigureAwait(false);
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
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        if (session  is null)
        {
            throw new Exceptions.SessionException($"No active session for user {userId}");
        }

        return session;
    }

    public async Task<bool> EndUserSessionAsync(long userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        if (session  is null)
        {
            return false;
        }

        var result = await _sessionService.CloseSessionAsync(session.SessionId, cancellationToken).ConfigureAwait(false);
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
        var middlewareList = _middleware.ToList(); // Create a copy to avoid modifying the original collection
        var index = 0;

        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next = null!;
        next = async (ctx) =>
        {
            if (ctx.IsStopped)
                return ctx; // A middleware called RespondAndStop — skip remaining middleware.

            if (index < middlewareList.Count)
            {
                var currentMiddleware = middlewareList[index];
                index++;
                return await currentMiddleware.ProcessAsync(ctx, next, cancellationToken).ConfigureAwait(false);
            }
            return ctx; // All middleware processed, return the context
        };

        return await next(context).ConfigureAwait(false);
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
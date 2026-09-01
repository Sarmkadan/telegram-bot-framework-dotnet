#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// High-level orchestrator for bot operations, coordinating multiple services.
/// </summary>
/// <remarks>
/// Provides a unified interface for processing user messages, executing commands,
/// managing menus and sessions, and handling the overall bot workflow.
/// </remarks>
public interface IBotOrchestrator
{
    /// <summary>
    /// Processes an incoming user message through the bot pipeline.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who sent the message.</param>
    /// <param name="chatId">The unique identifier of the chat containing the message.</param>
    /// <param name="content">The message text to process.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name, or <see langword="null"/> when it is unavailable.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result contains the execution context produced by the bot pipeline.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Models.ExecutionContext> ProcessUserMessageAsync(
        long userId,
        long chatId,
        string content,
        string firstName,
        string? lastName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command for a user through the bot pipeline.
    /// </summary>
    /// <param name="userId">The unique identifier of the user executing the command.</param>
    /// <param name="chatId">The unique identifier of the chat in which the command is executed.</param>
    /// <param name="commandName">The name of the command to execute.</param>
    /// <param name="parameters">The command parameters, or <see langword="null"/> when the command has no parameters.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result contains the execution context produced by command execution.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Models.ExecutionContext> ExecuteUserCommandAsync(
        long userId,
        long chatId,
        string commandName,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a menu and records navigation to it for the user's active session.
    /// </summary>
    /// <param name="userId">The unique identifier of the user viewing the menu.</param>
    /// <param name="menuId">The unique identifier of the menu to display.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result is the requested menu.</returns>
    /// <exception cref="InvalidOperationException">No menu exists with the specified <paramref name="menuId"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Models.Menu> DisplayMenuAsync(
        long userId,
        string menuId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a menu button action for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who selected the button.</param>
    /// <param name="menuId">The unique identifier of the menu containing the button.</param>
    /// <param name="buttonCallbackData">The callback data associated with the selected button.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> when the button action is recognized and handled;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The button navigates to a menu that does not exist.
    /// </exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<bool> HandleMenuButtonAsync(
        long userId,
        string menuId,
        string buttonCallbackData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's active session.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose session is requested.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result is the user's active session.</returns>
    /// <exception cref="Exceptions.SessionException">The user has no active session.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Models.UserSession> GetUserSessionAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the user's active session.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose session should end.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> when an active session is closed successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<bool> EndUserSessionAsync(long userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of bot orchestrator.
/// </summary>
/// <remarks>
/// Coordinates all bot services to process user interactions, manage state,
/// and execute commands through the middleware pipeline.
/// </remarks>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BotOrchestrator"/> class.
    /// </summary>
    /// <param name="userService">The service used to manage users.</param>
    /// <param name="commandService">The service used to retrieve and execute commands.</param>
    /// <param name="sessionService">The service used to manage user sessions.</param>
    /// <param name="messageService">The service used to process and track messages.</param>
    /// <param name="menuService">The service used to retrieve menus and buttons.</param>
    /// <param name="middleware">The middleware components that make up the processing pipeline.</param>
    /// <param name="configuration">The bot configuration.</param>
    /// <param name="logger">The logger used to record orchestration activity.</param>
    /// <exception cref="ArgumentNullException">Any constructor argument is <see langword="null"/>.</exception>
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
    }

    /// <summary>
    /// Processes an incoming user message through the complete bot pipeline.
    /// </summary>
    /// <param name="userId">The Telegram user ID.</param>
    /// <param name="chatId">The chat ID where the message was sent.</param>
    /// <param name="content">The message content/text.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name (optional).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Execution context containing the result of message processing.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
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
                string.Join(IBotOrchestratorConstants.ErrorSeparator, finalContext.Errors),
                cancellationToken);
        }

        _logger.LogInformation(
            IBotOrchestratorConstants.MessageProcessedLogTemplate,
            userId, finalContext.ContextId, finalContext.IsValid);

        return finalContext;
    }

    /// <summary>
    /// Executes a command for a user through the middleware pipeline.
    /// </summary>
    /// <param name="userId">The unique identifier of the user executing the command.</param>
    /// <param name="chatId">The unique identifier of the chat in which the command is executed.</param>
    /// <param name="commandName">The name of the command to execute.</param>
    /// <param name="parameters">The command parameters, or <see langword="null"/> when the command has no parameters.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result contains the execution context produced by command execution.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
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
            context.AddError(string.Format(IBotOrchestratorConstants.CommandNotFoundFormat, commandName));
            return context;
        }

        context = await ExecuteMiddlewarePipelineAsync(context, cancellationToken).ConfigureAwait(false);

        if (context.IsValid)
        {
            await _commandService.RecordCommandExecutionAsync(commandName, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation(
            IBotOrchestratorConstants.CommandExecutedLogTemplate,
            userId, commandName, context.IsValid);

        return context;
    }

    /// <summary>
    /// Retrieves a menu and records navigation to it for the user's active session.
    /// </summary>
    /// <param name="userId">The unique identifier of the user viewing the menu.</param>
    /// <param name="menuId">The unique identifier of the menu to display.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result is the requested menu.</returns>
    /// <exception cref="InvalidOperationException">No menu exists with the specified <paramref name="menuId"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Models.Menu> DisplayMenuAsync(
        long userId,
        string menuId,
        CancellationToken cancellationToken = default)
    {
        var menu = await _menuService.GetMenuAsync(menuId, cancellationToken).ConfigureAwait(false);
        if (menu  is null)
        {
            throw new InvalidOperationException(string.Format(IBotOrchestratorConstants.MenuNotFoundFormat, menuId));
        }

        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        if (session  is not null)
        {
            await _sessionService.NavigateToMenuAsync(session.SessionId, menuId, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation(IBotOrchestratorConstants.MenuDisplayedLogTemplate, userId, menuId);
        return menu;
    }

    /// <summary>
    /// Handles a menu button action for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who selected the button.</param>
    /// <param name="menuId">The unique identifier of the menu containing the button.</param>
    /// <param name="buttonCallbackData">The callback data associated with the selected button.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> when the button action is recognized and handled;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The button navigates to a menu that does not exist.
    /// </exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<bool> HandleMenuButtonAsync(
        long userId,
        string menuId,
        string buttonCallbackData,
        CancellationToken cancellationToken = default)
    {
        var button = await _menuService.GetButtonAsync(menuId, buttonCallbackData, cancellationToken).ConfigureAwait(false);
        if (button  is null)
        {
            _logger.LogWarning(IBotOrchestratorConstants.ButtonNotFoundLogTemplate, menuId, buttonCallbackData);
            return false;
        }

        var activeSession = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        var chatId = activeSession?.ChatId ?? IBotOrchestratorConstants.UnknownChatId;

        switch (button.Action)
        {
            case Models.ButtonAction.ExecuteCommand:
                await ExecuteUserCommandAsync(
                    userId,
                    chatId,
                    buttonCallbackData.TrimStart(IBotOrchestratorConstants.CommandPrefix),
                    null,
                    cancellationToken).ConfigureAwait(false);
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
                _logger.LogWarning(IBotOrchestratorConstants.UnknownButtonActionLogTemplate, button.Action);
                return false;
        }

        _logger.LogInformation(IBotOrchestratorConstants.ButtonHandledLogTemplate, userId, buttonCallbackData);
        return true;
    }

    /// <summary>
    /// Gets the user's active session.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose session is requested.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task whose result is the user's active session.</returns>
    /// <exception cref="Exceptions.SessionException">The user has no active session.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Models.UserSession> GetUserSessionAsync(long userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);
        if (session  is null)
        {
            throw new Exceptions.SessionException(
                string.Format(IBotOrchestratorConstants.NoActiveSessionFormat, userId));
        }

        return session;
    }

    /// <summary>
    /// Ends the user's active session.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose session should end.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> when an active session is closed successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
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
            _logger.LogInformation(IBotOrchestratorConstants.SessionEndedLogTemplate, userId, session.SessionId);
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
    internal static string ExtractCommandName(string messageContent)
    {
        var trimmed = messageContent.Trim();
        if (trimmed.Length == 0 || trimmed[0] != IBotOrchestratorConstants.CommandPrefix)
            return string.Empty;

        var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0
            ? parts[0].TrimStart(IBotOrchestratorConstants.CommandPrefix)
            : string.Empty;
    }
}

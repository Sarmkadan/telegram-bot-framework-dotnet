#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Services;

/// <summary>
/// Defines the contract for processing incoming Telegram updates.
/// </summary>
public interface IUpdateHandler
{
    /// <summary>
    /// Processes a single incoming update from Telegram.
    /// </summary>
    /// <param name="update">The Telegram update to process.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The execution context produced by the processing pipeline.</returns>
    Task<Models.ExecutionContext> HandleUpdateAsync(Update update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles an error that occurred during update processing.
    /// </summary>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total number of updates processed since the handler was created.
    /// </summary>
    long ProcessedCount { get; }

    /// <summary>
    /// Gets the total number of updates that failed processing.
    /// </summary>
    long FailedCount { get; }
}

/// <summary>
/// Processes incoming Telegram updates by dispatching them to the appropriate
/// service based on the update type (message, callback query, inline query, etc.).
/// </summary>
public sealed class UpdateHandler : IUpdateHandler
{
    private readonly ICommandService _commandService;
    private readonly ISessionService _sessionService;
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateHandler> _logger;
    private long _processedCount;
    private long _failedCount;
    private readonly ConcurrentDictionary<long, DateTime> _lastUpdatePerUser = new();

    /// <inheritdoc />
    public long ProcessedCount => Interlocked.Read(ref _processedCount);

    /// <inheritdoc />
    public long FailedCount => Interlocked.Read(ref _failedCount);

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateHandler"/> class.
    /// </summary>
    public UpdateHandler(
        ICommandService commandService,
        ISessionService sessionService,
        IMessageService messageService,
        IUserService userService,
        ILogger<UpdateHandler> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Models.ExecutionContext> HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        _logger.LogDebug("Processing update {UpdateId} of type {UpdateType}", update.Id, update.Type);

        try
        {
            var context = update.Type switch
            {
                UpdateType.Message => await HandleMessageUpdateAsync(update.Message!, cancellationToken).ConfigureAwait(false),
                UpdateType.CallbackQuery => await HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken).ConfigureAwait(false),
                UpdateType.EditedMessage => await HandleMessageUpdateAsync(update.EditedMessage!, cancellationToken).ConfigureAwait(false),
                _ => CreateEmptyContext(update)
            };

            Interlocked.Increment(ref _processedCount);

            if (context.UserId > 0)
            {
                _lastUpdatePerUser[context.UserId] = DateTime.UtcNow;
            }

            return context;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failedCount);
            _logger.LogError(ex, "Failed to process update {UpdateId}", update.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _logger.LogError(exception, "Telegram bot polling error: {Message}", exception.Message);
        return Task.CompletedTask;
    }

    private async Task<Models.ExecutionContext> HandleMessageUpdateAsync(Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
    {
        var userId = message.From?.Id ?? 0;
        var chatId = message.Chat.Id;
        var text = message.Text ?? string.Empty;

        var firstName = message.From?.FirstName ?? string.Empty;
        var lastName = message.From?.LastName;
        var user = await _userService.GetOrCreateUserAsync(userId, firstName, lastName, cancellationToken).ConfigureAwait(false);
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false)
                      ?? await _sessionService.CreateSessionAsync(userId, chatId, cancellationToken).ConfigureAwait(false);

        await _userService.RecordUserActivityAsync(userId, cancellationToken).ConfigureAwait(false);

        var incomingMessage = new Models.Message
        {
            UserId = userId,
            ChatId = chatId,
            Content = text,
            CreatedAt = DateTime.UtcNow
        };
        var processed = await _messageService.ProcessIncomingMessageAsync(incomingMessage, cancellationToken).ConfigureAwait(false);

        var context = new Models.ExecutionContext
        {
            UserId = userId,
            ChatId = chatId,
            User = user,
            Session = session,
            Message = processed
        };

        if (text.StartsWith('/'))
        {
            var commandName = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? text;
            var command = await _commandService.GetCommandAsync(commandName.TrimStart('/'), cancellationToken).ConfigureAwait(false);
            if (command is not null)
            {
                context.Command = command;
                context = await _commandService.ExecuteCommandAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        return context;
    }

    private async Task<Models.ExecutionContext> HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var chatId = callbackQuery.Message?.Chat.Id ?? 0;
        var user = await _userService.GetOrCreateUserAsync(userId, callbackQuery.From.FirstName, callbackQuery.From.LastName, cancellationToken).ConfigureAwait(false);
        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false);

        return new Models.ExecutionContext
        {
            UserId = userId,
            ChatId = chatId,
            User = user,
            Session = session,
            Parameters = new Dictionary<string, object>
            {
                ["CallbackData"] = callbackQuery.Data ?? string.Empty,
                ["CallbackQueryId"] = callbackQuery.Id
            }
        };
    }

    private static Models.ExecutionContext CreateEmptyContext(Update update)
    {
        return new Models.ExecutionContext
        {
            Parameters = new Dictionary<string, object>
            {
                ["UpdateType"] = update.Type.ToString(),
                ["UpdateId"] = update.Id
            }
        };
    }
}

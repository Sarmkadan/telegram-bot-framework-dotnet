#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBotFramework.Controllers;

namespace TelegramBotFramework.Services;

/// <summary>
/// Defines the contract for the top-level bot host that manages startup and shutdown.
/// </summary>
public interface IBotHost : IAsyncDisposable
{
    /// <summary>
    /// Starts the bot in the configured mode (polling or webhook).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the startup.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the bot gracefully.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the shutdown.</param>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the bot is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the bot's username as returned by the Telegram API, available after startup.
    /// </summary>
    string? BotUsername { get; }
}

/// <summary>
/// Top-level host that coordinates bot startup: validates the token, resolves the
/// bot username, starts either long-polling or the <see cref="IWebhookServer"/>,
/// and runs a periodic session-cleanup task.
/// </summary>
public sealed class BotHost : IBotHost
{
    private readonly Models.BotConfiguration _configuration;
    private readonly IUpdateHandler _updateHandler;
    private readonly ISessionManager _sessionManager;
    private readonly IWebhookServer? _webhookServer;
    private readonly ILogger<BotHost> _logger;
    private ITelegramBotClient? _botClient;
    private CancellationTokenSource? _pollingCts;
    private Timer? _sessionCleanupTimer;
    private bool _isRunning;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public string? BotUsername { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BotHost"/> class.
    /// </summary>
    /// <param name="configuration">Bot configuration with token and mode settings.</param>
    /// <param name="updateHandler">The handler for processing updates.</param>
    /// <param name="sessionManager">The session manager for periodic cleanup.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="webhookServer">
    /// Optional webhook server; when <see langword="null"/>, the bot runs in polling mode.
    /// </param>
    public BotHost(
        Models.BotConfiguration configuration,
        IUpdateHandler updateHandler,
        ISessionManager sessionManager,
        ILogger<BotHost> logger,
        IWebhookServer? webhookServer = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webhookServer = webhookServer;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Bot is already running");
            return;
        }

        if (string.IsNullOrWhiteSpace(_configuration.BotToken))
        {
            throw new InvalidOperationException("BotToken is not configured.");
        }

        _botClient = new TelegramBotClient(_configuration.BotToken);

        var me = await _botClient.GetMe(cancellationToken).ConfigureAwait(false);
        BotUsername = me.Username;
        _logger.LogInformation("Bot authenticated as @{Username} (id: {Id})", me.Username, me.Id);

        if (_webhookServer is not null)
        {
            await _webhookServer.StartAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Bot started in webhook mode");
        }
        else
        {
            StartPolling();
            _logger.LogInformation("Bot started in polling mode");
        }

        _sessionCleanupTimer = new Timer(
            _ => _ = CleanupSessionsAsync(),
            null,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5));

        _isRunning = true;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Bot is not running");
            return;
        }

        if (_sessionCleanupTimer is not null)
        {
            await _sessionCleanupTimer.DisposeAsync().ConfigureAwait(false);
            _sessionCleanupTimer = null;
        }

        if (_webhookServer is not null)
        {
            await _webhookServer.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _pollingCts?.Cancel();
            _pollingCts?.Dispose();
            _pollingCts = null;
        }

        _isRunning = false;
        _logger.LogInformation("Bot stopped. Processed: {Processed}, Failed: {Failed}",
            _updateHandler.ProcessedCount, _updateHandler.FailedCount);
    }

    /// <summary>
    /// Disposes of all managed resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isRunning)
        {
            await StopAsync().ConfigureAwait(false);
        }

        _sessionManager.Dispose();

        if (_sessionCleanupTimer is not null)
        {
            await _sessionCleanupTimer.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void StartPolling()
    {
        _pollingCts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.EditedMessage,
                UpdateType.InlineQuery
            },
            DropPendingUpdates = true
        };

        _botClient!.StartReceiving(
            async (client, update, ct) =>
            {
                try
                {
                    await _updateHandler.HandleUpdateAsync(update, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _updateHandler.HandleErrorAsync(ex, ct).ConfigureAwait(false);
                }
            },
            async (client, ex, ct) =>
            {
                await _updateHandler.HandleErrorAsync(ex, ct).ConfigureAwait(false);
            },
            receiverOptions,
            _pollingCts.Token);
    }

    private async Task CleanupSessionsAsync()
    {
        try
        {
            var expired = await _sessionManager.ExpireInactiveSessionsAsync().ConfigureAwait(false);
            if (expired > 0)
            {
                _logger.LogDebug("Session cleanup: expired {Count} sessions", expired);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session cleanup failed");
        }
    }
}

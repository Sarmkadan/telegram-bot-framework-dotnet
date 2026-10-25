#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Production implementation of <see cref="IWebhookService"/>.
/// Registers and unregisters the Telegram webhook automatically as a hosted service.
/// Validated updates are placed on a bounded queue so the webhook endpoint can answer Telegram at once;
/// background workers then dispatch them to subscribed handlers.
/// </summary>
public sealed class WebhookService : IWebhookService, IHostedService
{
    private readonly ITelegramApiClient _apiClient;
    private readonly WebhookHandler _webhookHandler;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookService> _logger;
    private readonly List<Task> _workers = [];

    private volatile Channel<TelegramUpdate> _updateQueue;
    private CancellationTokenSource? _workerCancellation;
    private volatile bool _isQueueClosed;

    private bool _isRegistered;
    private DateTime? _registeredAt;
    private long _updatesDispatched;
    private long _updatesDropped;

    /// <summary>
    /// Raised whenever a validated update is taken from the queue for processing.
    /// Subscribe to this event to receive and process updates.
    /// </summary>
    public event Func<TelegramUpdate, Task>? OnUpdateReceived;

    /// <summary>
    /// Initialises a new instance of <see cref="WebhookService"/>.
    /// </summary>
    /// <param name="apiClient">The Telegram API client used to register the webhook.</param>
    /// <param name="options">The webhook configuration, including the queue capacity and full-queue policy.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    public WebhookService(
        ITelegramApiClient apiClient,
        WebhookOptions options,
        ILogger<WebhookService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webhookHandler = new WebhookHandler();
        _updateQueue = CreateUpdateQueue();
    }

    /// <summary>
    /// Gets the webhook configuration options.
    /// </summary>
    public WebhookOptions Options => _options;

    // -------------------------------------------------------------------------
    // IHostedService
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts the background workers that dispatch queued updates, then registers the webhook.
    /// Updates received before this point stay queued and are processed once the workers start.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        StartUpdateWorkers();
        await RegisterAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stops accepting updates, waits for queued updates to be processed, then unregisters the webhook.
    /// </summary>
    /// <param name="cancellationToken">
    /// Bounds how long the queue is drained. When it fires, remaining queued updates are abandoned and logged.
    /// </param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DrainUpdateQueueAsync(cancellationToken).ConfigureAwait(false);
        await UnregisterAsync(cancellationToken).ConfigureAwait(false);
    }

    // -------------------------------------------------------------------------
    // IWebhookService
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _options.Validate();

            var success = (await _apiClient.SetWebhookAsync(_options.Url, _options.SecretToken).ConfigureAwait(false)).Success;

            if (success)
            {
                _isRegistered = true;
                _registeredAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Webhook registered successfully — URL: {WebhookUrl}, MaxConnections: {MaxConnections}, SecretTokenEnabled: {SecretTokenEnabled}",
                    _options.Url, _options.MaxConnections, _options.HasSecretToken());
            }
            else
            {
                _logger.LogError("Failed to register webhook at URL: {WebhookUrl}", _options.Url);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while registering webhook");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRegistered)
            return;

        try
        {
            var success = (await _apiClient.RemoveWebhookAsync().ConfigureAwait(false)).Success;

            if (success)
            {
                _isRegistered = false;
                _logger.LogInformation("Webhook unregistered successfully");
            }
            else
            {
                _logger.LogWarning("Webhook unregistration request returned a failure response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while unregistering webhook");
        }
    }

    /// <inheritdoc/>
    public async Task DispatchUpdateAsync(TelegramUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        System.Threading.Interlocked.Increment(ref _updatesDispatched);

        var handler = OnUpdateReceived;
        if (handler is null)
        {
            _logger.LogDebug(
                "Received update {UpdateId} but no handlers are subscribed", update.UpdateId);
            return;
        }

        try
        {
            await handler(update).ConfigureAwait(false);

            _logger.LogDebug(
                "Dispatched update {UpdateId} of type {UpdateType}", update.UpdateId, update.MessageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error dispatching update {UpdateId}", update.UpdateId);
        }
    }

    /// <inheritdoc/>
    public WebhookEnqueueResult EnqueueUpdate(TelegramUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (_updateQueue.Writer.TryWrite(update))
        {
            return WebhookEnqueueResult.Accepted;
        }

        if (_isQueueClosed)
        {
            return WebhookEnqueueResult.Closed;
        }

        _logger.LogWarning(WebhookQueueConstants.LogQueueFullRejected, _options.QueueCapacity, update.UpdateId);
        return WebhookEnqueueResult.Rejected;
    }

    /// <inheritdoc/>
    public WebhookInfo GetInfo() => new()
    {
        IsRegistered = _isRegistered,
        Url = _isRegistered ? _options.Url : null,
        RegisteredAt = _registeredAt,
        UpdatesDispatched = _updatesDispatched,
        QueueDepth = _updateQueue.Reader.Count,
        QueueCapacity = _options.QueueCapacity,
        UpdatesDropped = Interlocked.Read(ref _updatesDropped)
    };

    private Channel<TelegramUpdate> CreateUpdateQueue()
    {
        var channelOptions = new BoundedChannelOptions(_options.QueueCapacity)
        {
            FullMode = _options.QueueFullPolicy switch
            {
                WebhookQueueFullPolicy.DropOldest => BoundedChannelFullMode.DropOldest,
                _ => BoundedChannelFullMode.Wait
            }
        };

        return Channel.CreateBounded<TelegramUpdate>(channelOptions, OnQueuedUpdateDropped);
    }

    private void OnQueuedUpdateDropped(TelegramUpdate evictedUpdate)
    {
        Interlocked.Increment(ref _updatesDropped);
        _logger.LogWarning(
            WebhookQueueConstants.LogQueueFullDroppedOldest, _options.QueueCapacity, evictedUpdate.UpdateId);
    }

    private void StartUpdateWorkers()
    {
        if (_workers.Count > 0)
        {
            return;
        }

        if (_isQueueClosed)
        {
            _updateQueue = CreateUpdateQueue();
            _isQueueClosed = false;
        }

        _workerCancellation = new CancellationTokenSource();
        var reader = _updateQueue.Reader;

        for (var i = 0; i < _options.QueueWorkerCount; i++)
        {
            _workers.Add(RunUpdateWorkerAsync(reader, _workerCancellation.Token));
        }
    }

    private async Task RunUpdateWorkerAsync(ChannelReader<TelegramUpdate> reader, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var update in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                await DispatchUpdateAsync(update, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Abandoned by StopAsync after its drain timeout; the remaining depth is logged there.
        }
    }

    private async Task DrainUpdateQueueAsync(CancellationToken cancellationToken)
    {
        _isQueueClosed = true;
        _updateQueue.Writer.TryComplete();

        try
        {
            await Task.WhenAll(_workers).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _workerCancellation?.Cancel();
        }
        finally
        {
            var remaining = _updateQueue.Reader.Count;
            if (remaining > 0)
            {
                _logger.LogWarning(WebhookQueueConstants.LogQueueDrainIncomplete, remaining);
            }

            _workers.Clear();
            _workerCancellation = null;
        }
    }

    /// <summary>
    /// Parses and validates a raw JSON payload received at the webhook endpoint.
    /// Returns the parsed update, or <c>null</c> if the payload is invalid or the
    /// secret-token check fails.
    /// </summary>
    /// <param name="jsonBody">The raw request body.</param>
    /// <param name="secretTokenHeader">
    /// Value of the <c>X-Telegram-Bot-Api-Secret-Token</c> header, if present.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    public async Task<TelegramUpdate?> ParseAndValidateAsync(
        string jsonBody,
        string? secretTokenHeader,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_webhookHandler.ValidateSecretToken(secretTokenHeader, _options.SecretToken))
        {
            _logger.LogWarning("Rejected webhook request: secret token validation failed");
            return null;
        }

        return await _webhookHandler.ProcessUpdateAsync(jsonBody, cancellationToken).ConfigureAwait(false);
    }
}
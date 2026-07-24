#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Implements polling strategy for fetching Telegram updates.
/// Used as an alternative to webhooks for receiving bot updates.
/// </summary>
public sealed class PollingStrategy
{
    private readonly ITelegramApiClient _apiClient;
    private readonly WebhookHandler _webhookHandler;
    private readonly ILogger<PollingStrategy> _logger;
    private long _lastUpdateId = 0;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollingTask;

    public PollingStrategy(ITelegramApiClient apiClient, ILogger<PollingStrategy>? logger = null)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? new ConsoleLogger<PollingStrategy>();
        _webhookHandler = new WebhookHandler();
    }

    /// <summary>
    /// Raised when a new update is received.
    /// </summary>
    public event Func<TelegramUpdate, Task>? OnUpdateReceived;

    /// <summary>
    /// Starts the polling loop that continuously fetches updates from Telegram.
    /// </summary>
    public void Start(TimeSpan? pollInterval = null)
    {
        if (_pollingTask  is not null && !_pollingTask.IsCompleted)
        {
            _logger.LogWarning("Polling is already running");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var interval = pollInterval ?? TimeSpan.FromSeconds(1);

        _pollingTask = Task.Run(() => PollAsync(interval, _cancellationTokenSource.Token), _cancellationTokenSource.Token);

        _logger.LogInformation("Polling started with interval {IntervalMs}ms", interval.TotalMilliseconds);
    }

    /// <summary>
    /// Stops the polling loop gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        if (_cancellationTokenSource  is null)
            return;

        _cancellationTokenSource.Cancel();

        if (_pollingTask  is not null)
        {
            try
            {
                await _pollingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
        _pollingTask = null;

        _logger.LogInformation("Polling stopped");
    }

    /// <summary>
    /// Gets the current polling status.
    /// </summary>
    public PollingStatus GetStatus()
    {
        return new PollingStatus
        {
            IsRunning = _pollingTask  is not null && !_pollingTask.IsCompleted,
            LastUpdateId = _lastUpdateId,
            LastPollTime = LastPollTime
        };
    }

    public DateTime? LastPollTime { get; private set; }

    private async Task PollAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                LastPollTime = DateTime.UtcNow;

                _logger.LogDebug("Polling for updates, last update ID: {LastUpdateId}", _lastUpdateId);

                var offset = _lastUpdateId > 0 ? _lastUpdateId + 1 : 0;
                var updates = await _apiClient.GetUpdatesAsync(offset).ConfigureAwait(false);

                foreach (var updateElement in updates)
                {
                    var update = await _webhookHandler.ProcessUpdateAsync(updateElement.GetRawText()).ConfigureAwait(false);
                    if (update is not null)
                    {
                        await ProcessUpdateAsync(update).ConfigureAwait(false);
                    }
                    else if (updateElement.TryGetProperty("update_id", out var updateIdElement) &&
                             updateIdElement.TryGetInt64(out var rawUpdateId))
                    {
                        // Advance the offset even when the update cannot be parsed;
                        // otherwise the same malformed update is fetched forever and
                        // the polling loop spins without making progress.
                        if (rawUpdateId > _lastUpdateId)
                        {
                            _lastUpdateId = rawUpdateId;
                        }

                        _logger.LogWarning("Skipping unparseable update {UpdateId}", rawUpdateId);
                    }
                }

                if (updates.Count == 0)
                {
                    // Small delay to avoid hammering the API when there is nothing to fetch
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during polling");
                // Continue polling even on error, but with backoff
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Processes an update received from polling, advancing the last-seen update ID and
    /// invoking <see cref="OnUpdateReceived"/>.
    /// </summary>
    public async Task ProcessUpdateAsync(TelegramUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            _lastUpdateId = update.UpdateId;

            if (OnUpdateReceived  is not null)
            {
                await OnUpdateReceived.Invoke(update).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update {UpdateId}", update.UpdateId);
        }
    }
}

/// <summary>
/// Represents the current polling status.
/// </summary>
public sealed class PollingStatus
{
    public bool IsRunning { get; set; }
    public long LastUpdateId { get; set; }
    public DateTime? LastPollTime { get; set; }
}
#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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
    private readonly IUpdateOffsetStore _offsetStore;
    private long _lastUpdateId = 0;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollingTask;

    // Backoff configuration constants
    private const int BasePollIntervalMs = 1000; // Default 1 second
    private const int MaxBackoffMs = 30000; // Maximum 30 seconds
    private const double BackoffMultiplier = 1.5; // Exponential backoff multiplier
    private const double JitterFactor = 0.1; // 10% jitter to avoid thundering herd

    // Backoff tracking state
    private int _currentBackoffMs = 0;
    private int _consecutiveFailureCount = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PollingStrategy"/> class with an in-memory offset store.
    /// </summary>
    /// <param name="apiClient">The Telegram API client to use for fetching updates.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiClient is null.</exception>
    public PollingStrategy(ITelegramApiClient apiClient, ILogger<PollingStrategy>? logger = null)
        : this(apiClient, new InMemoryUpdateOffsetStore(), logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PollingStrategy"/> class with a custom offset store.
    /// </summary>
    /// <param name="apiClient">The Telegram API client to use for fetching updates.</param>
    /// <param name="offsetStore">The offset store for persisting the last processed update.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiClient or offsetStore is null.</exception>
    public PollingStrategy(
        ITelegramApiClient apiClient,
        IUpdateOffsetStore offsetStore,
        ILogger<PollingStrategy>? logger = null)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _offsetStore = offsetStore ?? throw new ArgumentNullException(nameof(offsetStore));
        _logger = logger ?? new ConsoleLogger<PollingStrategy>();
        _webhookHandler = new WebhookHandler();

        // Initialize with offset from store
        _lastUpdateId = _offsetStore.GetLastOffset();
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
        if (_pollingTask is not null && !_pollingTask.IsCompleted)
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
        if (_cancellationTokenSource is null)
            return;

        _cancellationTokenSource.Cancel();

        if (_pollingTask is not null)
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
            IsRunning = _pollingTask is not null && !_pollingTask.IsCompleted,
            LastUpdateId = _lastUpdateId,
            LastPollTime = LastPollTime,
            CurrentBackoffMs = _currentBackoffMs,
            ConsecutiveFailureCount = _consecutiveFailureCount,
            BasePollIntervalMs = BasePollIntervalMs
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

                // Reset failure count on successful poll
                if (_consecutiveFailureCount > 0)
                {
                    _logger.LogDebug("Resetting failure count after successful poll");
                    _consecutiveFailureCount = 0;
                    _currentBackoffMs = 0;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _consecutiveFailureCount++;
                _logger.LogError(ex, "Error during polling (failure #{FailureCount})", _consecutiveFailureCount);

                // Apply adaptive backoff with exponential decay and jitter
                int backoffDelayMs = CalculateBackoffDelay();

                _logger.LogWarning(
                    "Applying backoff delay of {BackoffMs}ms after {FailureCount} consecutive failures",
                    backoffDelayMs,
                    _consecutiveFailureCount);

                await Task.Delay(backoffDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Calculates the adaptive backoff delay using exponential backoff with jitter.
    /// </summary>
    /// <returns>The backoff delay in milliseconds.</returns>
    private int CalculateBackoffDelay()
    {
        // Calculate exponential backoff with jitter
        double exponentialBackoff = Math.Pow(BackoffMultiplier, _consecutiveFailureCount - 1);
        double backoffMs = BasePollIntervalMs * exponentialBackoff;

        // Apply jitter to avoid thundering herd problems
        double jitterRange = backoffMs * JitterFactor;
        double jitter = (Random.Shared.NextDouble() * 2 - 1) * jitterRange; // Range: [-jitterRange, +jitterRange]
        backoffMs += jitter;

        // Clamp to maximum backoff
        int result = (int)Math.Min(backoffMs, MaxBackoffMs);

        // Ensure we don't go below base interval
        _currentBackoffMs = Math.Max(result, BasePollIntervalMs);

        return _currentBackoffMs;
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
            await _offsetStore.SetLastOffset(_lastUpdateId);
            await _offsetStore.PersistAsync();

            if (OnUpdateReceived is not null)
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
    /// <summary>
    /// Gets or sets a value indicating whether the polling loop is currently active.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the last processed update identifier.
    /// </summary>
    public long LastUpdateId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last successful polling request.
    /// </summary>
    public DateTime? LastPollTime { get; set; }

    /// <summary>
    /// Gets or sets the current backoff delay in milliseconds.
    /// </summary>
    public int CurrentBackoffMs { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive failures since the last successful poll.
    /// </summary>
    public int ConsecutiveFailureCount { get; set; }

    /// <summary>
    /// Gets or sets the base poll interval in milliseconds.
    /// </summary>
    public int BasePollIntervalMs { get; set; }
}
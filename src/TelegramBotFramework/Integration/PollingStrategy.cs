#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using TelegramBotFramework.Events;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Implements polling strategy for fetching Telegram updates.
/// Used as an alternative to webhooks for receiving bot updates.
/// </summary>
public sealed class PollingStrategy : IHostedService
{
    private readonly ITelegramApiClient _apiClient;
    private readonly WebhookHandler _webhookHandler;
    private readonly ILogger<PollingStrategy> _logger;
    private readonly IUpdateOffsetStore _offsetStore;
    private readonly EventPublisher _eventPublisher;
    private long _lastUpdateId = 0;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollingTask;
    private readonly List<Task> _inFlightHandlers = [];
    private readonly object _inFlightLock = new();
    private readonly PollingOptions _options;
    private TimeSpan _shutdownTimeout;
    private bool _isShuttingDown = false;

    // Update flood protection configuration
    private int _maxUpdatesPerBatch = 100; // Maximum updates to process per polling cycle
    private int _maxInFlightUpdates = 1000; // Maximum concurrent in-flight updates

    // Backoff configuration constants
    private const int BasePollIntervalMs = 1000; // Default 1 second
    private const int MaxBackoffMs = 30000; // Maximum 30 seconds
    private const double BackoffMultiplier = 1.5; // Exponential backoff multiplier
    private const double JitterFactor = 0.1; // 10% jitter to avoid thundering herd

    // Backoff tracking state
    private int _currentBackoffMs = 0;
    private int _consecutiveFailureCount = 0;
    private int _updatesProcessedThisBatch = 0;
    private bool _updateFloodDetected = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PollingStrategy"/> class with an in-memory offset store.
    /// </summary>
    /// <param name="apiClient">The Telegram API client to use for fetching updates.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiClient is null.</exception>
    public PollingStrategy(ITelegramApiClient apiClient, ILogger<PollingStrategy>? logger = null)
        : this(apiClient, new InMemoryUpdateOffsetStore(), logger)
    {
        _logger.LogInformation("PollingStrategy initialized with in-memory offset store");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PollingStrategy"/> class with a custom offset store.
    /// </summary>
    /// <param name="apiClient">The Telegram API client to use for fetching updates.</param>
    /// <param name="offsetStore">The offset store for persisting the last processed update.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <param name="eventPublisher">Optional event publisher for state change notifications.</param>
    /// <param name="maxUpdatesPerBatch">Optional maximum updates to process per polling cycle. Defaults to <see cref="PollingOptions.MaxUpdatesPerBatch"/>.</param>
    /// <param name="maxInFlightUpdates">Optional maximum concurrent in-flight updates. Defaults to <see cref="PollingOptions.MaxInFlightUpdates"/>.</param>
    /// <param name="options">Optional configuration options. When omitted, defaults matching the previous hardcoded behavior are used.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiClient or offsetStore is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxUpdatesPerBatch, maxInFlightUpdates, or an options value is invalid.</exception>
    public PollingStrategy(
        ITelegramApiClient apiClient,
        IUpdateOffsetStore offsetStore,
        ILogger<PollingStrategy>? logger = null,
        EventPublisher? eventPublisher = null,
        int? maxUpdatesPerBatch = null,
        int? maxInFlightUpdates = null,
        PollingOptions? options = null)
    {
        if (logger != null)
        {
            _logger = logger;
        }
        else
        {
            _logger = new ConsoleLogger<PollingStrategy>();
        }
        _logger.LogInformation("Initializing PollingStrategy");
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _offsetStore = offsetStore ?? throw new ArgumentNullException(nameof(offsetStore));
        _webhookHandler = new WebhookHandler();
        _eventPublisher = eventPublisher ?? new EventPublisher(new InMemoryEventBus());

        // Apply configuration; defaults mirror the previously hardcoded values
        _options = options ?? new PollingOptions();
        _shutdownTimeout = _options.ShutdownTimeout;

        // Explicit limits passed to the constructor take precedence over the options instance
        _maxUpdatesPerBatch = maxUpdatesPerBatch ?? _options.MaxUpdatesPerBatch;
        _maxInFlightUpdates = maxInFlightUpdates ?? _options.MaxInFlightUpdates;

        if (_options.PollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Poll interval must be positive");
        }

        if (_options.ShutdownTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Shutdown timeout must be positive");
        }

        if (_maxUpdatesPerBatch < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxUpdatesPerBatch), "Maximum updates per batch must be at least 1");
        }

        if (_maxInFlightUpdates < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxInFlightUpdates), "Maximum in-flight updates must be at least 1");
        }

        // Initialize with offset from store
        _lastUpdateId = _offsetStore.GetLastOffset();
        _logger.LogInformation("PollingStrategy initialized successfully. LastUpdateId: {LastUpdateId}", _lastUpdateId);
    }

    /// <summary>
    /// Raised when a new update is received.
    /// </summary>
    public event Func<TelegramUpdate, Task>? OnUpdateReceived;

    /// <summary>
    /// Starts the polling loop that continuously fetches updates from Telegram.
    /// </summary>
    /// <param name="pollInterval">Optional polling interval. If not specified, uses the default 1 second.</param>
    /// <exception cref="InvalidOperationException">Thrown when polling is already running.</exception>
    public void Start(TimeSpan? pollInterval = null)
    {
        _logger.LogInformation("Starting polling strategy with pollInterval={PollIntervalMs}ms", pollInterval?.TotalMilliseconds ?? _options.PollInterval.TotalMilliseconds);

        if (_pollingTask is not null && !_pollingTask.IsCompleted)
        {
            _logger.LogWarning("Polling is already running, not starting another polling task");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var interval = pollInterval ?? _options.PollInterval;

        _pollingTask = Task.Run(() => PollAsync(interval, _cancellationTokenSource.Token), _cancellationTokenSource.Token);

        _logger.LogInformation("Polling started successfully with interval {IntervalMs}ms", interval.TotalMilliseconds);
    }

    /// <summary>
    /// Starts the polling service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        Start();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the polling loop gracefully, waiting for in-flight updates to complete.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cancellationTokenSource is null)
        {
            _logger.LogDebug("Polling was not running, nothing to stop");
            return;
        }

        _logger.LogInformation("Initiating graceful shutdown of polling strategy");

        // Signal shutdown start
        _isShuttingDown = true;

        // Stop fetching new updates
        _cancellationTokenSource.Cancel();

        // Wait for polling task to complete
        if (_pollingTask is not null && !_pollingTask.IsCompleted)
        {
            try
            {
                await _pollingTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
                _logger.LogDebug("Polling task cancelled");
            }
        }

        // Wait for in-flight handlers to complete with timeout
        await DrainInFlightHandlersAsync(cancellationToken).ConfigureAwait(false);

        // Persist the final offset
        await PersistFinalOffsetAsync().ConfigureAwait(false);

        // Publish state change event
        await PublishBotStoppedEventAsync().ConfigureAwait(false);

        // Cleanup
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
        _pollingTask = null;

        _logger.LogInformation("Polling stopped gracefully");
    }

    /// <summary>
    /// Stops the polling service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return StopAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current polling status including drain state.
    /// </summary>
    /// <returns>A <see cref="PollingStatus"/> object representing the current state.</returns>
    public PollingStatus GetStatus()
    {
        lock (_inFlightLock)
        {
            return new PollingStatus
            {
                IsRunning = _pollingTask is not null && !_pollingTask.IsCompleted,
                LastUpdateId = _lastUpdateId,
                LastPollTime = LastPollTime,
                CurrentBackoffMs = _currentBackoffMs,
                ConsecutiveFailureCount = _consecutiveFailureCount,
                BasePollIntervalMs = BasePollIntervalMs,
                IsDraining = _isShuttingDown,
                IsDrainComplete = _isShuttingDown && _inFlightHandlers.Count == 0,
                InFlightCount = _inFlightHandlers.Count,
                MaxUpdatesPerBatch = _maxUpdatesPerBatch,
                MaxInFlightUpdates = _maxInFlightUpdates,
                CurrentUpdatesPerBatch = _updatesProcessedThisBatch,
                IsUpdateFloodDetected = _updateFloodDetected
            };
        }
    }

    public DateTime? LastPollTime { get; private set; }

    public bool Equals(PollingStrategy? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // Compare IsRunning: computed from _pollingTask
        bool thisIsRunning = _pollingTask is not null && !_pollingTask.IsCompleted;
        bool otherIsRunning = other._pollingTask is not null && !other._pollingTask.IsCompleted;

        return thisIsRunning == otherIsRunning &&
               _lastUpdateId == other._lastUpdateId &&
               LastPollTime == other.LastPollTime;
    }

    public override bool Equals(object? obj)
    {
        if (obj is PollingStrategy other)
            return Equals(other);
        return false;
    }

    public override int GetHashCode()
    {
        // Compute IsRunning for hash code
        bool isRunning = _pollingTask is not null && !_pollingTask.IsCompleted;
        return HashCode.Combine(isRunning, _lastUpdateId, LastPollTime);
    }

    public static bool operator ==(PollingStrategy? left, PollingStrategy? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(PollingStrategy? left, PollingStrategy? right)
    {
        return !(left == right);
    }

    private async Task PollAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Polling loop started");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                LastPollTime = DateTime.UtcNow;
                _updatesProcessedThisBatch = 0;
                _updateFloodDetected = false;

                _logger.LogDebug("Polling for updates, last update ID: {LastUpdateId}", _lastUpdateId);

                var offset = _lastUpdateId > 0 ? _lastUpdateId + 1 : 0;
                var updates = await _apiClient.GetUpdatesAsync(offset).ConfigureAwait(false);

                _logger.LogInformation("Received {UpdateCount} updates from Telegram API", updates.Count);

                // Apply update flood protection - limit updates per batch
                var updatesToProcess = ApplyUpdateFloodProtection(updates);

                foreach (var updateElement in updatesToProcess)
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
                    _logger.LogDebug("No updates received, waiting {IntervalMs}ms before next poll", interval.TotalMilliseconds);
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                }

                // Reset failure count on successful poll
                if (_consecutiveFailureCount > 0)
                {
                    _logger.LogInformation("Polling successful after {FailureCount} consecutive failures, resetting failure count", _consecutiveFailureCount);
                    _consecutiveFailureCount = 0;
                    _currentBackoffMs = 0;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Polling loop cancelled");
                break;
            }
            catch (Exception ex)
            {
                _consecutiveFailureCount++;
                _logger.LogError(ex, "Error during polling (failure #{FailureCount})", _consecutiveFailureCount);

                // Apply adaptive backoff with exponential decay and jitter
                int backoffDelayMs = CalculateBackoffDelay();

                _logger.LogWarning(
                    "Applying backoff delay of {BackoffDelayMs}ms after {FailureCount} consecutive failures",
                    backoffDelayMs,
                    _consecutiveFailureCount);

                await Task.Delay(backoffDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }
        _logger.LogInformation("Polling loop stopped");
    }

    /// <summary>
    /// Applies update flood protection by limiting the number of updates processed per batch.
    /// </summary>
    /// <param name="updates">The raw updates received from Telegram API.</param>
    /// <returns>A filtered collection of updates to process.</returns>
    private IReadOnlyList<JsonElement> ApplyUpdateFloodProtection(IReadOnlyList<JsonElement> updates)
    {
        _logger.LogDebug("Applying update flood protection. Received {UpdateCount} updates, max allowed per batch: {MaxUpdatesPerBatch}", updates.Count, _maxUpdatesPerBatch);

        if (updates.Count <= _maxUpdatesPerBatch)
        {
            _logger.LogDebug("Update count within limits, no flood protection needed");
            return updates;
        }

        _updateFloodDetected = true;
        _logger.LogWarning(
            "Update flood detected: {TotalUpdates} updates received, but only {MaxAllowed} will be processed per batch. " +
            "Consider increasing maxUpdatesPerBatch if this is expected behavior.",
            updates.Count,
            _maxUpdatesPerBatch);

        // Return only the first N updates to prevent memory exhaustion
        var limitedUpdates = updates.Take(_maxUpdatesPerBatch).ToList();
        _logger.LogInformation("Limited updates from {OriginalCount} to {LimitedCount} due to flood protection", updates.Count, limitedUpdates.Count);
        return limitedUpdates;
    }

    /// <summary>
    /// Calculates the adaptive backoff delay using exponential backoff with jitter.
    /// </summary>
    /// <returns>The backoff delay in milliseconds.</returns>
    private int CalculateBackoffDelay()
    {
        _logger.LogDebug("Calculating backoff delay. ConsecutiveFailureCount: {FailureCount}", _consecutiveFailureCount);

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

        _logger.LogInformation("Calculated backoff delay: {BackoffDelayMs}ms (failure count: {FailureCount})", _currentBackoffMs, _consecutiveFailureCount);

        return _currentBackoffMs;
    }

    /// <summary>
    /// Processes an update received from polling, advancing the last-seen update ID and
    /// invoking <see cref="OnUpdateReceived"/>.
    /// </summary>
    /// <param name="update">The update to process.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when update is null.</exception>
    public async Task ProcessUpdateAsync(TelegramUpdate update)
    {
        _logger.LogInformation("Processing update {UpdateId}", update.UpdateId);
        ArgumentNullException.ThrowIfNull(update);

        // Track in-flight handler
        Task? handlerTask = null;
        lock (_inFlightLock)
        {
            if (_isShuttingDown)
            {
                _logger.LogWarning("Update {UpdateId} received during shutdown, skipping processing", update.UpdateId);
                return;
            }

            // Apply in-flight update limit to prevent memory exhaustion
            if (_inFlightHandlers.Count >= _maxInFlightUpdates)
            {
                _updateFloodDetected = true;
                _logger.LogWarning(
                    "In-flight update limit reached: {CurrentCount} updates in flight, but maximum is {MaxAllowed}. " +
                    "Update {UpdateId} will be dropped to prevent memory exhaustion. " +
                    "Consider increasing maxInFlightUpdates if this is expected behavior.",
                    _inFlightHandlers.Count,
                    _maxInFlightUpdates,
                    update.UpdateId);
                return;
            }

            _lastUpdateId = update.UpdateId;
            _updatesProcessedThisBatch++;
            handlerTask = OnUpdateReceived?.Invoke(update);

            if (handlerTask is not null)
            {
                _inFlightHandlers.Add(handlerTask);
            }
        }

        try
        {
            if (handlerTask is not null)
            {
                await handlerTask.ConfigureAwait(false);
            }

            await _offsetStore.SetLastOffset(_lastUpdateId).ConfigureAwait(false);
            await _offsetStore.PersistAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully processed update {UpdateId}", update.UpdateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update {UpdateId}", update.UpdateId);
        }
        finally
        {
            // Remove completed handler from tracking
            if (handlerTask is not null)
            {
                lock (_inFlightLock)
                {
                    _inFlightHandlers.Remove(handlerTask);
                }
            }
        }
    }

    /// <summary>
    /// Drains all in-flight update handlers with a configurable timeout.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task DrainInFlightHandlersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to drain {Count} in-flight update handlers", _inFlightHandlers.Count);

        var drainTimeout = _shutdownTimeout;
        var startTime = DateTime.UtcNow;

        while (true)
        {
            lock (_inFlightLock)
            {
                if (_inFlightHandlers.Count == 0)
                {
                    _logger.LogInformation("All in-flight handlers completed");
                    return;
                }

                // Check if we've exceeded the timeout
                if (DateTime.UtcNow - startTime > drainTimeout)
                {
                    _logger.LogWarning("Timeout reached while waiting for in-flight handlers. {RemainingCount} handlers still running", _inFlightHandlers.Count);
                    return;
                }
            }

            // Log progress every 5 seconds
            var elapsed = DateTime.UtcNow - startTime;
            if (elapsed.TotalSeconds % 5 < 0.1) // Small tolerance for timing
            {
                _logger.LogInformation("Waiting for in-flight handlers... {Elapsed}s elapsed, {RemainingCount} remaining",
                    (int)elapsed.TotalSeconds, _inFlightHandlers.Count);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Persists the final update offset before shutdown completes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task PersistFinalOffsetAsync()
    {
        _logger.LogInformation("Persisting final offset: {LastUpdateId}", _lastUpdateId);
        try
        {
            await _offsetStore.SetLastOffset(_lastUpdateId).ConfigureAwait(false);
            await _offsetStore.PersistAsync().ConfigureAwait(false);
            _logger.LogInformation("Final offset persisted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist final offset");
            throw;
        }
    }

    /// <summary>
    /// Publishes a BotStateChangedEvent(Stopped) event to notify subscribers of shutdown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task PublishBotStoppedEventAsync()
    {
        _logger.LogInformation("Publishing BotStateChangedEvent(Stopped)");
        try
        {
            await _eventPublisher.PublishBotStateChangedAsync(
                "Running",
                "Stopped",
                "Graceful shutdown completed"
            ).ConfigureAwait(false);
            _logger.LogInformation("Bot state change event published successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish bot state changed event");
            // Don't throw - event publishing should not fail the shutdown
        }
    }

    /// <summary>
    /// Sets the shutdown timeout for graceful shutdown.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    public void SetShutdownTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive");
        }

        _shutdownTimeout = timeout;
        _logger.LogInformation("Shutdown timeout set to {TimeoutSeconds}s", (int)timeout.TotalSeconds);
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

    /// <summary>
    /// Gets or sets a value indicating whether the polling strategy is currently draining in-flight handlers.
    /// </summary>
    public bool IsDraining { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all in-flight handlers have completed draining.
    /// </summary>
    public bool IsDrainComplete { get; set; }

    /// <summary>
    /// Gets or sets the number of currently in-flight update handlers.
    /// </summary>
    public int InFlightCount { get; set; }

        /// <summary>
        /// Gets or sets the configured maximum updates per batch limit.
        /// </summary>
        public int MaxUpdatesPerBatch { get; set; }

        /// <summary>
        /// Gets or sets the configured maximum in-flight updates limit.
        /// </summary>
        public int MaxInFlightUpdates { get; set; }

        /// <summary>
        /// Gets or sets the number of updates processed in the current batch.
        /// </summary>
        public int CurrentUpdatesPerBatch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an update flood was detected.
        /// </summary>
        public bool IsUpdateFloodDetected { get; set; }
}

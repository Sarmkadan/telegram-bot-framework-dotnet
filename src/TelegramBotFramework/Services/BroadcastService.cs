#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Services;

/// <summary>
/// Service for broadcasting messages to multiple chat IDs with configurable rate limiting,
/// failure collection, progress callbacks, and cancellation support.
/// </summary>
public sealed class BroadcastService : IBroadcastService, IDisposable
{
    private readonly ITelegramApiClient _telegramApiClient;
    private readonly ILogger<BroadcastService> _logger;
    private readonly SemaphoreSlim _rateLimiter;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly object _statsLock = new();

    private long _totalMessagesSent;
    private long _totalMessagesFailed;
    private int _currentConcurrency;
    private DateTime _lastStatsReset = DateTime.UtcNow;

    public BroadcastService(
        ITelegramApiClient telegramApiClient,
        ILogger<BroadcastService>? logger = null)
    {
        _telegramApiClient = telegramApiClient ?? throw new ArgumentNullException(nameof(telegramApiClient));
        _logger = logger ?? new ConsoleLogger<BroadcastService>();
        _rateLimiter = new SemaphoreSlim(1, 1);
        _concurrencyLimiter = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Broadcasts a message to the distinct chat IDs, preserving their first-occurrence order.
    /// </summary>
    /// <param name="chatIds">The chat IDs to broadcast to.</param>
    /// <param name="messageText">The message text to send.</param>
    /// <param name="options">Optional broadcast configuration.</param>
    /// <param name="progressCallback">An optional callback invoked after each batch.</param>
    /// <param name="cancellationToken">A token used to cancel the broadcast.</param>
    /// <returns>The result of the broadcast.</returns>
    public async Task<BroadcastResult> BroadcastAsync(
        IReadOnlyList<long> chatIds,
        string messageText,
        BroadcastOptions? options = null,
        Func<BroadcastProgress, Task>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (chatIds == null || chatIds.Count == 0)
        {
            _logger.LogWarning("Broadcast called with empty chat IDs list");
            return BroadcastResult.Success(0, Array.Empty<long>(), "No chats to broadcast to");
        }

        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new ArgumentException("Message text cannot be empty", nameof(messageText));
        }

        var distinctChatIds = RemoveDuplicateChatIds(chatIds);
        var config = options ?? new BroadcastOptions();
        var startTime = DateTime.UtcNow;
        var successfulChatIds = new List<long>();
        var failures = new List<FailedChat>();
        var processedCount = 0;
        var successCount = 0;
        var failedCount = 0;

        _logger.LogInformation("Starting broadcast to {ChatCount} chats with rate limit: {MessagesPerSecond} msg/s, concurrency: {MaxConcurrency}",
            distinctChatIds.Count, config.MessagesPerSecond, config.MaxConcurrency);

        try
        {
            // Process chats in batches based on rate limit
            var batchSize = Math.Max(1, config.MessagesPerSecond);
            var batchDelay = config.BatchDelay ?? CalculateBatchDelay(config.MessagesPerSecond);

            for (int i = 0; i < distinctChatIds.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = distinctChatIds.Skip(i).Take(batchSize).ToList();
                var batchTasks = new List<Task>();

                foreach (var chatId in batch)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    batchTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Wait for concurrency slot
                            await _concurrencyLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
                            Interlocked.Increment(ref _currentConcurrency);

                            // Apply rate limiting if configured
                            if (config.MessagesPerSecond > 0)
                            {
                                await _rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
                            }

                            var messageToSend = config.MessageFormatter?.Invoke(messageText, chatId) ?? messageText;
                            var success = await _telegramApiClient.SendMessageAsync(chatId, messageToSend).ConfigureAwait(false);

                            Interlocked.Increment(ref _totalMessagesSent);
                            Interlocked.Increment(ref processedCount);

                            if (success)
                            {
                                lock (successfulChatIds)
                                {
                                    successfulChatIds.Add(chatId);
                                }
                                Interlocked.Increment(ref successCount);
                                _logger.LogDebug("Successfully sent message to chat {ChatId}", chatId);
                            }
                            else
                            {
                                var failure = new FailedChat(chatId, "Failed to send message", 0);
                                lock (failures)
                                {
                                    failures.Add(failure);
                                }
                                Interlocked.Increment(ref failedCount);
                                Interlocked.Increment(ref _totalMessagesFailed);
                                _logger.LogWarning("Failed to send message to chat {ChatId}", chatId);
                            }
                        }
                        catch (Exception ex) when (config.ContinueOnError)
                        {
                            var failure = new FailedChat(chatId, ex.Message, 0);
                            lock (failures)
                            {
                                failures.Add(failure);
                            }
                            Interlocked.Increment(ref failedCount);
                            Interlocked.Increment(ref _totalMessagesFailed);
                            _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
                        }
                        catch (Exception ex)
                        {
                            throw new BroadcastException($"Broadcast failed for chat {chatId}", ex);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _currentConcurrency);
                            _concurrencyLimiter.Release();

                            if (config.MessagesPerSecond > 0)
                            {
                                _rateLimiter.Release();
                            }
                        }
                    }, cancellationToken));
                }

                // Wait for batch to complete
                await Task.WhenAll(batchTasks).ConfigureAwait(false);

                // Report progress
                if (progressCallback != null)
                {
                    var progress = CreateProgress(
                        distinctChatIds.Count,
                        processedCount,
                        successCount,
                        failedCount,
                        failures,
                        startTime,
                        config);
                    try
                    {
                        await progressCallback(progress).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Progress callback failed");
                    }
                }

                // Apply batch delay if configured
                if (batchDelay > TimeSpan.Zero && i + batchSize < distinctChatIds.Count)
                {
                    await Task.Delay(batchDelay, cancellationToken).ConfigureAwait(false);
                }
            }

            var summary = failedCount == 0
                ? $"Successfully broadcast to {successCount} chats"
                : $"Broadcast completed: {successCount} succeeded, {failedCount} failed";

            return BroadcastResult.Mixed(
                distinctChatIds.Count,
                successfulChatIds,
                failures,
                summary);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Broadcast was cancelled");
            var summary = $"Broadcast cancelled: {successCount} succeeded, {failedCount} failed";
            return BroadcastResult.Mixed(
                distinctChatIds.Count,
                successfulChatIds,
                failures,
                summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Broadcast failed completely");
            var failure = new FailedChat(-1, ex.Message, 0);
            return BroadcastResult.Failure(distinctChatIds.Count, new[] { failure }, $"Broadcast failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Simulates broadcasting a message to the distinct chat IDs, preserving their
    /// first-occurrence order, without calling the Telegram API.
    /// </summary>
    /// <param name="chatIds">The chat IDs to include in the simulation.</param>
    /// <param name="messageText">The message text that would be sent.</param>
    /// <param name="options">Optional broadcast configuration used for progress reporting.</param>
    /// <param name="progressCallback">An optional callback invoked after each simulated chat.</param>
    /// <param name="cancellationToken">A token used to cancel the simulation.</param>
    /// <returns>A successful result for every processed chat.</returns>
    public async Task<BroadcastResult> BroadcastDryRunAsync(
        IReadOnlyList<long> chatIds,
        string messageText,
        BroadcastOptions? options = null,
        Func<BroadcastProgress, Task>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (chatIds == null || chatIds.Count == 0)
        {
            _logger.LogWarning("Broadcast dry run called with empty chat IDs list");
            return BroadcastResult.Success(0, Array.Empty<long>(), "No chats to broadcast to");
        }

        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new ArgumentException("Message text cannot be empty", nameof(messageText));
        }

        var distinctChatIds = RemoveDuplicateChatIds(chatIds);
        var config = options ?? new BroadcastOptions();
        var startTime = DateTime.UtcNow;
        var successfulChatIds = new List<long>(distinctChatIds.Count);
        var failures = Array.Empty<FailedChat>();

        foreach (var chatId in distinctChatIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            successfulChatIds.Add(chatId);

            if (progressCallback != null)
            {
                var progress = CreateProgress(
                    distinctChatIds.Count,
                    successfulChatIds.Count,
                    successfulChatIds.Count,
                    0,
                    failures,
                    startTime,
                    config);

                try
                {
                    await progressCallback(progress).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Progress callback failed");
                }
            }
        }

        return BroadcastResult.Success(
            distinctChatIds.Count,
            successfulChatIds,
            $"Dry run successfully processed {successfulChatIds.Count} chats");
    }

    public async Task<BroadcastResult> BroadcastToUsersAsync(
        IReadOnlyList<Models.BotUser> users,
        string messageText,
        BroadcastOptions? options = null,
        Func<BroadcastProgress, Task>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (users == null || users.Count == 0)
        {
            _logger.LogWarning("BroadcastToUsers called with empty user list");
            return BroadcastResult.Success(0, Array.Empty<long>(), "No users to broadcast to");
        }

        var chatIds = users.Select(u => u.TelegramId).ToList();
        return await BroadcastAsync(chatIds, messageText, options, progressCallback, cancellationToken).ConfigureAwait(false);
    }

    public RateLimitStats GetRateLimitStats()
    {
        lock (_statsLock)
        {
            var now = DateTime.UtcNow;
            var timeSinceReset = now - _lastStatsReset;

            // Simple average calculation
            var avgMessagesPerSecond = timeSinceReset.TotalSeconds > 0
                ? _totalMessagesSent / timeSinceReset.TotalSeconds
                : 0;

            return new RateLimitStats(
                messagesPerSecond: _rateLimiter.CurrentCount,
                maxConcurrency: _concurrencyLimiter.CurrentCount,
                totalMessagesSent: _totalMessagesSent,
                totalMessagesFailed: _totalMessagesFailed,
                averageMessagesPerSecond: avgMessagesPerSecond,
                currentConcurrency: _currentConcurrency);
        }
    }

    private TimeSpan CalculateBatchDelay(int messagesPerSecond)
    {
        if (messagesPerSecond <= 0)
        {
            return TimeSpan.Zero;
        }

        // Calculate delay to maintain rate: 1000ms / messagesPerSecond
        var delayMs = 1000.0 / messagesPerSecond;
        return TimeSpan.FromMilliseconds(delayMs);
    }

    private IReadOnlyList<long> RemoveDuplicateChatIds(IReadOnlyList<long> chatIds)
    {
        var seenChatIds = new HashSet<long>();
        var distinctChatIds = new List<long>(chatIds.Count);

        foreach (var chatId in chatIds)
        {
            if (seenChatIds.Add(chatId))
            {
                distinctChatIds.Add(chatId);
            }
        }

        var duplicateCount = chatIds.Count - distinctChatIds.Count;
        if (duplicateCount > 0)
        {
            _logger.LogInformation("Skipped {DuplicateCount} duplicate chat IDs", duplicateCount);
        }

        return distinctChatIds;
    }

    private BroadcastProgress CreateProgress(
        int totalChats,
        int processedCount,
        int successCount,
        int failedCount,
        IReadOnlyList<FailedChat> failures,
        DateTime startTime,
        BroadcastOptions options)
    {
        var elapsed = DateTime.UtcNow - startTime;
        var remainingChats = totalChats - processedCount;
        var estimatedRemaining = remainingChats > 0 && successCount > 0
            ? TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds * remainingChats / successCount)
            : TimeSpan.Zero;

        var currentRate = options.MessagesPerSecond > 0
            ? options.MessagesPerSecond
            : Math.Min(25, processedCount / (int)elapsed.TotalSeconds + 1);

        return new BroadcastProgress(
            totalChats,
            processedCount,
            successCount,
            failedCount,
            failures,
            elapsed,
            estimatedRemaining > TimeSpan.Zero ? estimatedRemaining : null,
            currentRate);
    }

    public void Dispose()
    {
        _rateLimiter.Dispose();
        _concurrencyLimiter.Dispose();
    }
}

/// <summary>
/// Exception thrown when broadcast operations fail.
/// </summary>
public sealed class BroadcastException : Exception
{
    public BroadcastException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

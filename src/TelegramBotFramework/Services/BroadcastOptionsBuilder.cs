#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace TelegramBotFramework.Services;

/// <summary>
/// Fluent builder for <see cref="BroadcastOptions"/> that validates on build.
/// </summary>
public sealed class BroadcastOptionsBuilder
{
    private readonly BroadcastOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BroadcastOptionsBuilder"/> class.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public BroadcastOptionsBuilder(BroadcastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Pre-fills the builder from an existing <see cref="BroadcastOptions"/> instance.
    /// </summary>
    /// <param name="template">The options to copy from.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static BroadcastOptionsBuilder From(BroadcastOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new BroadcastOptionsBuilder(template);
    }

    /// <summary>
    /// Sets the maximum messages per second.
    /// </summary>
    /// <param name="messagesPerSecond">The maximum messages per second.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithMessagesPerSecond(int messagesPerSecond)
    {
        _options.MessagesPerSecond = messagesPerSecond;
        return this;
    }

    /// <summary>
    /// Sets the maximum concurrent operations.
    /// </summary>
    /// <param name="maxConcurrency">The maximum concurrent operations.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithMaxConcurrency(int maxConcurrency)
    {
        _options.MaxConcurrency = maxConcurrency;
        return this;
    }

    /// <summary>
    /// Sets the maximum retry attempts for failed messages.
    /// </summary>
    /// <param name="maxRetryAttempts">The maximum retry attempts.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithMaxRetryAttempts(int maxRetryAttempts)
    {
        _options.MaxRetryAttempts = maxRetryAttempts;
        return this;
    }

    /// <summary>
    /// Sets the delay between retry attempts.
    /// </summary>
    /// <param name="retryDelay">The delay between retry attempts.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithRetryDelay(TimeSpan retryDelay)
    {
        _options.RetryDelay = retryDelay;
        return this;
    }

    /// <summary>
    /// Sets whether to continue on error.
    /// </summary>
    /// <param name="continueOnError">Whether to continue on error.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithContinueOnError(bool continueOnError)
    {
        _options.ContinueOnError = continueOnError;
        return this;
    }

    /// <summary>
    /// Sets the optional custom message formatter.
    /// </summary>
    /// <param name="messageFormatter">The optional custom message formatter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithMessageFormatter(Func<string, long, string>? messageFormatter)
    {
        _options.MessageFormatter = messageFormatter;
        return this;
    }

    /// <summary>
    /// Sets the optional delay between batches when rate limiting is active.
    /// </summary>
    /// <param name="batchDelay">The optional delay between batches.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BroadcastOptionsBuilder WithBatchDelay(TimeSpan? batchDelay)
    {
        _options.BatchDelay = batchDelay;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="BroadcastOptions"/> instance.
    /// </summary>
    /// <returns>The configured <see cref="BroadcastOptions"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public BroadcastOptions Build()
    {
        // Validate required properties
        if (_options.MessagesPerSecond < 0)
        {
            throw new ArgumentException("MessagesPerSecond must be non-negative.", nameof(_options.MessagesPerSecond));
        }

        if (_options.MaxConcurrency <= 0)
        {
            throw new ArgumentException("MaxConcurrency must be positive.", nameof(_options.MaxConcurrency));
        }

        if (_options.MaxRetryAttempts < 0)
        {
            throw new ArgumentException("MaxRetryAttempts must be non-negative.", nameof(_options.MaxRetryAttempts));
        }

        if (_options.RetryDelay < TimeSpan.Zero)
        {
            throw new ArgumentException("RetryDelay must be non-negative.", nameof(_options.RetryDelay));
        }

        // Note: ContinueOnError, MessageFormatter, and BatchDelay are optional and don't require validation

        return _options;
    }
}
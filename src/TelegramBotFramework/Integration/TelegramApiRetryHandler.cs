#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Utilities;

/// <summary>
/// Provides retry functionality for Telegram API calls with support for Retry-After header honoring.
/// </summary>
internal sealed class TelegramApiRetryHandler
{
    private readonly TelegramApiRetryOptions _options;
    private readonly ILogger<TelegramApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramApiRetryHandler"/> class.
    /// </summary>
    /// <param name="options">Retry configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public TelegramApiRetryHandler(TelegramApiRetryOptions options, ILogger<TelegramApiClient> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options.Validate();
    }

    /// <summary>
    /// Executes a Telegram API request with retry logic.
    /// </summary>
    /// <param name="httpClient">HTTP client to use.</param>
    /// <param name="url">Request URL.</param>
    /// <param name="content">Request content.</param>
    /// <param name="methodName">Name of the method being called (for logging).</param>
    /// <param name="isIdempotent">Whether the method is idempotent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HttpResponseMessage from the final attempt.</returns>
    public async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        HttpClient httpClient,
        string url,
        HttpContent? content,
        string methodName,
        bool isIdempotent,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var delay = _options.BaseDelayMilliseconds;
        Exception? lastException = null;

        while (true)
        {
            try
            {
                attempt++;
                _logger.LogDebug(
                    "Telegram API attempt {Attempt}/{MaxAttempts} for method: {MethodName}",
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    methodName);

                // Redact token from URL for logging
                var redactedUrl = TokenRedaction.RedactTokenFromUrl(url);

                var response = await httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);

                // Check for rate limiting (429) or server errors (5xx)
                if (response.StatusCode == HttpStatusCode.TooManyRequests && _options.RetryOnRateLimited)
                {
                    var retryAfter = await GetRetryAfterFromResponseAsync(response).ConfigureAwait(false);
                    _logger.LogWarning(
                        "Telegram API rate limited (429) for method {MethodName}. URL: {RedactedUrl}. Retry after {RetryAfter} seconds",
                        methodName,
                        redactedUrl,
                        retryAfter);

                    if (attempt <= _options.MaxRetryAttempts)
                    {
                        await DelayWithRetryAfterAsync(retryAfter, delay, cancellationToken).ConfigureAwait(false);
                        delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                        continue;
                    }

                    throw new TelegramRateLimitedException(
                        retryAfter,
                        $"Telegram API rate limited after {attempt} attempts for {methodName}");
                }

                if (IsRetryableServerError(response.StatusCode) && _options.RetryOnServerErrors)
                {
                    _logger.LogWarning(
                        "Telegram API server error {StatusCode} for method {MethodName}. URL: {RedactedUrl}",
                        response.StatusCode,
                        methodName,
                        redactedUrl);

                    if (attempt <= _options.MaxRetryAttempts)
                    {
                        await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                        delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                        continue;
                    }

                    throw new TelegramServerException(
                        response.StatusCode,
                        $"Telegram API server error after {attempt} attempts for {methodName}: {(int)response.StatusCode} {response.StatusCode}");
                }

                // For non-idempotent methods, only retry on network/timeout errors before bytes are sent
                if (!isIdempotent && response.StatusCode != HttpStatusCode.RequestTimeout &&
                    response.StatusCode != HttpStatusCode.GatewayTimeout &&
                    response.StatusCode != HttpStatusCode.BadGateway &&
                    response.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    return response;
                }

                // Check for timeout errors (408)
                if (response.StatusCode == HttpStatusCode.RequestTimeout && _options.RetryOnTimeout)
                {
                    _logger.LogWarning(
                        "Telegram API timeout (408) for method {MethodName}. URL: {RedactedUrl}",
                        methodName,
                        redactedUrl);

                    if (attempt <= _options.MaxRetryAttempts)
                    {
                        await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                        delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                        continue;
                    }
                }

                return response;
            }
            catch (HttpRequestException ex) when (_options.RetryOnNetworkErrors)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "Network error calling Telegram API method {MethodName} (attempt {Attempt}/{MaxAttempts}). URL: {RedactedUrl}",
                    methodName,
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    TokenRedaction.RedactTokenFromUrl(url));

                if (attempt <= _options.MaxRetryAttempts)
                {
                    await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                    delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                    continue;
                }

                throw;
            }
            catch (TaskCanceledException ex) when (_options.RetryOnNetworkErrors)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "Task cancelled/timeout calling Telegram API method {MethodName} (attempt {Attempt}/{MaxAttempts}). URL: {RedactedUrl}",
                    methodName,
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    TokenRedaction.RedactTokenFromUrl(url));

                if (attempt <= _options.MaxRetryAttempts)
                {
                    await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                    delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                    continue;
                }

                throw;
            }
            catch (Exception ex) when (attempt <= _options.MaxRetryAttempts)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "Error calling Telegram API method {MethodName} (attempt {Attempt}/{MaxAttempts}). URL: {RedactedUrl}",
                    methodName,
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    TokenRedaction.RedactTokenFromUrl(url));

                await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                continue;
            }
        }
    }

    /// <summary>
    /// Executes a Telegram API GET request with retry logic.
    /// </summary>
    /// <param name="httpClient">HTTP client to use.</param>
    /// <param name="url">Request URL.</param>
    /// <param name="methodName">Name of the method being called (for logging).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HttpResponseMessage from the final attempt.</returns>
    public async Task<HttpResponseMessage> ExecuteGetWithRetryAsync(
        HttpClient httpClient,
        string url,
        string methodName,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var delay = _options.BaseDelayMilliseconds;

        while (true)
        {
            try
            {
                attempt++;
                _logger.LogDebug(
                    "Telegram API GET attempt {Attempt}/{MaxAttempts} for method: {MethodName}",
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    methodName);

                // Redact token from URL for logging
                var redactedUrl = TokenRedaction.RedactTokenFromUrl(url);

                var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

                // Check for rate limiting (429) or server errors (5xx)
                if (response.StatusCode == HttpStatusCode.TooManyRequests && _options.RetryOnRateLimited)
                {
                    var retryAfter = await GetRetryAfterFromResponseAsync(response).ConfigureAwait(false);
                    _logger.LogWarning(
                        "Telegram API rate limited (429) for method {MethodName}. URL: {RedactedUrl}. Retry after {RetryAfter} seconds",
                        methodName,
                        redactedUrl,
                        retryAfter);

                    if (attempt <= _options.MaxRetryAttempts)
                    {
                        await DelayWithRetryAfterAsync(retryAfter, delay, cancellationToken).ConfigureAwait(false);
                        delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                        continue;
                    }

                    throw new TelegramRateLimitedException(
                        retryAfter > 0 ? retryAfter : 5,
                        $"Telegram API rate limited after {attempt} attempts for {methodName}");
                }

                if (IsRetryableServerError(response.StatusCode) && _options.RetryOnServerErrors)
                {
                    _logger.LogWarning(
                        "Telegram API server error {StatusCode} for method {MethodName}. URL: {RedactedUrl}",
                        response.StatusCode,
                        methodName,
                        redactedUrl);

                    if (attempt <= _options.MaxRetryAttempts)
                    {
                        await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                        delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                        continue;
                    }

                    throw new TelegramServerException(
                        response.StatusCode,
                        $"Telegram API server error after {attempt} attempts for {methodName}: {(int)response.StatusCode} {response.StatusCode}");
                }

                return response;
            }
            catch (HttpRequestException ex) when (_options.RetryOnNetworkErrors)
            {
                _logger.LogWarning(
                    ex,
                    "Network error calling Telegram API GET method {MethodName} (attempt {Attempt}/{MaxAttempts}). URL: {RedactedUrl}",
                    methodName,
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    TokenRedaction.RedactTokenFromUrl(url));

                if (attempt <= _options.MaxRetryAttempts)
                {
                    await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                    delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                    continue;
                }

                throw;
            }
            catch (TaskCanceledException ex) when (_options.RetryOnNetworkErrors)
            {
                _logger.LogWarning(
                    ex,
                    "Task cancelled/timeout calling Telegram API GET method {MethodName} (attempt {Attempt}/{MaxAttempts}). URL: {RedactedUrl}",
                    methodName,
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    TokenRedaction.RedactTokenFromUrl(url));

                if (attempt <= _options.MaxRetryAttempts)
                {
                    await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                    delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                    continue;
                }

                throw;
            }
            catch (Exception ex) when (attempt <= _options.MaxRetryAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Error calling Telegram API GET method {MethodName} (attempt {Attempt}/{MaxAttempts}). URL: {RedactedUrl}",
                    methodName,
                    attempt,
                    _options.MaxRetryAttempts + 1,
                    TokenRedaction.RedactTokenFromUrl(url));

                await DelayWithRetryAfterAsync(null, delay, cancellationToken).ConfigureAwait(false);
                delay = Math.Min(delay * 2, _options.MaxDelayMilliseconds);
                continue;
            }
        }
    }

    private async Task<int> GetRetryAfterFromResponseAsync(HttpResponseMessage response)
    {
        if (!_options.RespectRetryAfter || !_options.RetryOnRateLimited)
        {
            return 5; // Default retry after if not respecting Retry-After
        }

        // Try to get Retry-After header from response
        if (response.Headers.RetryAfter != null)
        {
            if (response.Headers.RetryAfter.Delta.HasValue)
            {
                return (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds;
            }

            if (response.Headers.RetryAfter.Date.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var retryAfterDate = response.Headers.RetryAfter.Date.Value;
                if (retryAfterDate > now)
                {
                    return (int)(retryAfterDate - now).TotalSeconds;
                }
            }
        }

        // Try to parse from response body
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = TelegramApiError.Parse(responseContent);
        if (error?.RetryAfter.HasValue == true)
        {
            return error.RetryAfter.Value;
        }

        // Default to 5 seconds if we can't determine the retry after time
        return 5;
    }

    private async Task DelayWithRetryAfterAsync(int? retryAfterSeconds, int baseDelayMilliseconds, CancellationToken cancellationToken)
    {
        var delayMilliseconds = retryAfterSeconds.HasValue && retryAfterSeconds.Value > 0
            ? retryAfterSeconds.Value * 1000
            : baseDelayMilliseconds;

        // Apply jitter: random delay between 0.5x and 1.5x the base delay
        var random = new Random();
        var jitterFactor = 0.5 + random.NextDouble();
        var totalDelay = (int)(delayMilliseconds * jitterFactor);

        _logger.LogDebug("Waiting {DelayMs}ms before retry (jitter factor: {JitterFactor:F2})", totalDelay, jitterFactor);

        await Task.Delay(totalDelay, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsRetryableServerError(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.InternalServerError ||
               statusCode == HttpStatusCode.BadGateway ||
               statusCode == HttpStatusCode.ServiceUnavailable ||
               statusCode == HttpStatusCode.GatewayTimeout ||
               statusCode == HttpStatusCode.HttpVersionNotSupported;
    }
}
#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;

/// <summary>
/// Configuration options for Telegram API retry behavior.
/// </summary>
public sealed class TelegramApiRetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for exponential backoff. Default is 1000ms (1 second).
    /// </summary>
    public int BaseDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Maximum delay in milliseconds between retries. Default is 30000ms (30 seconds).
    /// </summary>
    public int MaxDelayMilliseconds { get; set; } = 30000;

    /// <summary>
    /// Whether to respect Telegram's Retry-After header for 429 responses. Default is true.
    /// </summary>
    public bool RespectRetryAfter { get; set; } = true;

    /// <summary>
    /// Whether to retry on 5xx server errors. Default is true.
    /// </summary>
    public bool RetryOnServerErrors { get; set; } = true;

    /// <summary>
    /// Whether to retry on network/timeout errors. Default is true.
    /// </summary>
    public bool RetryOnNetworkErrors { get; set; } = true;

    /// <summary>
    /// Whether to retry on 429 Too Many Requests. Default is true.
    /// </summary>
    public bool RetryOnRateLimited { get; set; } = true;

    /// <summary>
    /// Whether to retry on 408 Request Timeout. Default is true.
    /// </summary>
    public bool RetryOnTimeout { get; set; } = true;

    /// <summary>
    /// Whether to retry on 503 Service Unavailable. Default is true.
    /// </summary>
    public bool RetryOnServiceUnavailable { get; set; } = true;

    /// <summary>
    /// Whether to retry on 502 Bad Gateway. Default is true.
    /// </summary>
    public bool RetryOnBadGateway { get; set; } = true;

    /// <summary>
    /// Whether to retry on 504 Gateway Timeout. Default is true.
    /// </summary>
    public bool RetryOnGatewayTimeout { get; set; } = true;

    /// <summary>
    /// Whether to retry on 500 Internal Server Error. Default is true.
    /// </summary>
    public bool RetryOnInternalServerError { get; set; } = true;

    /// <summary>
    /// Validates the configuration and throws if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (MaxRetryAttempts < 0)
        {
            throw new ArgumentException(
                "MaxRetryAttempts must be non-negative",
                nameof(MaxRetryAttempts));
        }

        if (BaseDelayMilliseconds <= 0)
        {
            throw new ArgumentException(
                "BaseDelayMilliseconds must be positive",
                nameof(BaseDelayMilliseconds));
        }

        if (MaxDelayMilliseconds <= 0)
        {
            throw new ArgumentException(
                "MaxDelayMilliseconds must be positive",
                nameof(MaxDelayMilliseconds));
        }

        if (BaseDelayMilliseconds > MaxDelayMilliseconds)
        {
            throw new ArgumentException(
                "BaseDelayMilliseconds must be less than or equal to MaxDelayMilliseconds",
                nameof(BaseDelayMilliseconds));
        }
    }
}

/// <summary>
/// Represents a Telegram API error response containing retry information.
/// </summary>
internal sealed class TelegramApiError
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public int ErrorCode { get; init; }

    /// <summary>
    /// Gets the error description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the retry after value in seconds (from Telegram's Retry-After header).
    /// </summary>
    public int? RetryAfter { get; init; }

    /// <summary>
    /// Parses a Telegram API error response.
    /// </summary>
    /// <param name="json">The JSON response from Telegram.</param>
    /// <returns>TelegramApiError instance or null if parsing failed.</returns>
    public static TelegramApiError? Parse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ok", out var okElement) || okElement.GetBoolean())
            {
                // Response is not an error
                return null;
            }

            // Check if there's an error in the response
            if (root.TryGetProperty("error_code", out var errorCodeElement))
            {
                var errorCode = errorCodeElement.GetInt32();
                var description = root.TryGetProperty("description", out var descElement)
                    ? descElement.GetString()
                    : null;

                var retryAfter = root.TryGetProperty("parameters", out var paramsElement) &&
                                paramsElement.TryGetProperty("retry_after", out var retryAfterElement)
                    ? retryAfterElement.GetInt32()
                    : (int?)null;

                return new TelegramApiError
                {
                    ErrorCode = errorCode,
                    Description = description,
                    RetryAfter = retryAfter
                };
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Exception thrown when a Telegram API call is rate limited.
/// </summary>
public sealed class TelegramRateLimitedException : Exception
{
    /// <summary>
    /// Gets the retry after value in seconds.
    /// </summary>
    public int RetryAfterSeconds { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramRateLimitedException"/> class.
    /// </summary>
    /// <param name="retryAfterSeconds">Retry after value in seconds.</param>
    /// <param name="message">Error message.</param>
    public TelegramRateLimitedException(int retryAfterSeconds, string? message = null)
        : base(message ?? $"Telegram API rate limited. Retry after {retryAfterSeconds} seconds.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// Exception thrown when a Telegram API call fails with a server error.
/// </summary>
public sealed class TelegramServerException : Exception
{
    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramServerException"/> class.
    /// </summary>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="message">Error message.</param>
    public TelegramServerException(HttpStatusCode statusCode, string? message = null)
        : base(message ?? $"Telegram API server error: {(int)statusCode} {statusCode}")
    {
        StatusCode = statusCode;
    }
}
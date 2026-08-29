#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Utilities;

/// <summary>
/// Client for interacting with Telegram Bot API.
/// Provides methods for sending messages, managing updates, and querying bot state.
/// </summary>
public sealed class TelegramApiClient : ITelegramApiClient
{
    private readonly HttpClientFactory _httpClientFactory;
    private readonly string _botToken;
    private readonly ILogger<TelegramApiClient> _logger;
    private readonly TelegramApiRetryHandler _retryHandler;
    private readonly TelegramApiRetryOptions _retryOptions;

    public TelegramApiClient(
        string botToken,
        HttpClientFactory? httpClientFactory = null,
        ILogger<TelegramApiClient>? logger = null,
        TelegramApiRetryOptions? retryOptions = null)
    {
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("Bot token cannot be empty", nameof(botToken));

        if (!ValidationUtility.IsValidTelegramToken(botToken))
            throw new ArgumentException("Invalid Telegram bot token format", nameof(botToken));

        _botToken = botToken;
        _httpClientFactory = httpClientFactory ?? new HttpClientFactory();
        _logger = logger ?? new ConsoleLogger<TelegramApiClient>();

        _retryOptions = retryOptions ?? new TelegramApiRetryOptions();
        _retryOptions.Validate();
        _retryHandler = new TelegramApiRetryHandler(_retryOptions, _logger);
    }

    /// <summary>
    /// Sends a simple text message to a chat.
    /// </summary>
    public async Task<bool> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text cannot be empty", nameof(text));

        cancellationToken.ThrowIfCancellationRequested();

        var payload = new { chat_id = chatId, text = text };
        return await SendApiRequestAsync(TelegramApiClientConstants.SendMessageMethod, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a message with inline keyboard buttons.
    /// </summary>
    public async Task<bool> SendMessageWithButtonsAsync(long chatId, string text, string[][] buttonLabels, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        cancellationToken.ThrowIfCancellationRequested();

        var buttons = buttonLabels.Select(row =>
            row.Select(label => new { text = label, callback_data = label }).ToArray()
        ).ToArray();

        var payload = new
        {
            chat_id = chatId,
            text = text,
            reply_markup = new { inline_keyboard = buttons }
        };

        return await SendApiRequestAsync(TelegramApiClientConstants.SendMessageMethod, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Edits a previously sent message.
    /// </summary>
    public async Task<bool> EditMessageAsync(long chatId, int messageId, string newText, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (messageId <= 0)
            throw new ArgumentException("Message ID must be positive", nameof(messageId));

        cancellationToken.ThrowIfCancellationRequested();

        var payload = new { chat_id = chatId, message_id = messageId, text = newText };
        return await SendApiRequestAsync(TelegramApiClientConstants.EditMessageTextMethod, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a message from a chat.
    /// </summary>
    public async Task<bool> DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        cancellationToken.ThrowIfCancellationRequested();

        var payload = new { chat_id = chatId, message_id = messageId };
        return await SendApiRequestAsync(TelegramApiClientConstants.DeleteMessageMethod, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a poll to a chat.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="question">Poll question (1-256 characters)</param>
    /// <param name="options">List of answer options (2-10 options, each 1-100 characters)</param>
    /// <param name="allowsMultipleAnswers">Whether users can select multiple answers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message ID of the sent poll if successful, null otherwise</returns>
    public async Task<int?> SendPollAsync(long chatId, string question, string[] options, bool allowsMultipleAnswers = false, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (string.IsNullOrWhiteSpace(question) || question.Length > TelegramApiClientConstants.MaxPollQuestionLength)
            throw new ArgumentException("Question must be 1-256 characters", nameof(question));

        if (options == null
            || options.Length < TelegramApiClientConstants.MinPollOptions
            || options.Length > TelegramApiClientConstants.MaxPollOptions)
            throw new ArgumentException("Must provide 2-10 options", nameof(options));

        if (options.Any(o => string.IsNullOrWhiteSpace(o) || o.Length > TelegramApiClientConstants.MaxPollOptionLength))
            throw new ArgumentException("Each option must be 1-100 characters", nameof(options));

        cancellationToken.ThrowIfCancellationRequested();

        var payload = new
        {
            chat_id = chatId,
            question = question,
            options = options,
            allows_multiple_answers = allowsMultipleAnswers
        };

        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var url = string.Format(
                CultureInfo.InvariantCulture,
                TelegramApiClientConstants.BotApiUrlFormat,
                _botToken,
                TelegramApiClientConstants.SendPollMethod);

            var json = JsonUtility.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, TelegramApiClientConstants.JsonContentType);

            var response = await client.PostAsync(url, content, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean() &&
                    root.TryGetProperty("result", out var resultElement) &&
                    resultElement.TryGetProperty("message_id", out var messageIdElement))
                {
                    return messageIdElement.GetInt32();
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Poll send failed: Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending poll to Telegram API: {RedactedMessage}", TokenRedaction.RedactToken(ex));
            return null;
        }
    }

    /// <summary>
    /// Sends a media group (album) to a chat.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="items">List of media items (2-10 items)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of message IDs for the sent media items if successful, empty list otherwise</returns>
    public async Task<IList<int>> SendMediaGroupAsync(long chatId, IList<MediaGroupItem> items, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (items == null
            || items.Count < TelegramApiClientConstants.MinMediaGroupItems
            || items.Count > TelegramApiClientConstants.MaxMediaGroupItems)
            throw new ArgumentException("Must provide 2-10 media items", nameof(items));

        if (items.Any(item => item == null || string.IsNullOrWhiteSpace(item.FileIdOrUrl)))
            throw new ArgumentException("Each item must have a valid FileIdOrUrl", nameof(items));

        try
        {
            var client = _httpClientFactory.GetTelegramClient();

            // A media group must go through sendMediaGroup in a single request;
            // sending items one-by-one produces separate messages instead of an album.
            var media = items.Select(item =>
            {
                var entry = new Dictionary<string, object>
                {
                    { "type", GetMediaFieldName(item.Type) },
                    { "media", item.FileIdOrUrl }
                };

                if (item.Caption != null)
                {
                    entry["caption"] = item.Caption;
                }

                return entry;
            }).ToArray();

            var payload = new Dictionary<string, object>
            {
                { "chat_id", chatId },
                { "media", media }
            };

            var json = JsonUtility.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, TelegramApiClientConstants.JsonContentType);

            var url = string.Format(
                CultureInfo.InvariantCulture,
                TelegramApiClientConstants.BotApiUrlFormat,
                _botToken,
                TelegramApiClientConstants.SendMediaGroupMethod);
            var response = await client.PostAsync(url, content, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Media group send failed: Status={StatusCode}, Error={Error}",
                    response.StatusCode, errorContent);
                return new List<int>();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var messageIds = new List<int>();

            if (root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean() &&
                root.TryGetProperty("result", out var resultElement) &&
                resultElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var message in resultElement.EnumerateArray())
                {
                    if (message.TryGetProperty("message_id", out var messageIdElement))
                    {
                        messageIds.Add(messageIdElement.GetInt32());
                    }
                }
            }

            _logger.LogInformation("Sent media group with {Count} items to chat {ChatId}", messageIds.Count, chatId);
            return messageIds;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Media group send operation was cancelled");
            return new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending media group to Telegram API: {RedactedMessage}", TokenRedaction.RedactToken(ex));
            return new List<int>();
        }
    }

    /// <summary>
    /// Gets information about the bot itself.
    /// </summary>
    public async Task<string?> GetMeAsync()
    {
        return await GetApiRequestAsync(TelegramApiClientConstants.GetMeMethod).ConfigureAwait(false);
    }

    /// <summary>
    /// Answers a callback query from an inline button press.
    /// </summary>
    public async Task<bool> AnswerCallbackQueryAsync(string callbackQueryId, string? notificationText = null)
    {
        if (string.IsNullOrWhiteSpace(callbackQueryId))
            throw new ArgumentException("Callback query ID cannot be empty", nameof(callbackQueryId));

        var payload = new { callback_query_id = callbackQueryId, text = notificationText };
        return await SendApiRequestAsync(
            TelegramApiClientConstants.AnswerCallbackQueryMethod,
            payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the webhook URL for receiving updates.
    /// </summary>
    public async Task<bool> SetWebhookAsync(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl) || !ValidationUtility.IsValidUrl(webhookUrl))
            throw new ArgumentException("Invalid webhook URL", nameof(webhookUrl));

        var payload = new { url = webhookUrl };
        return await SendApiRequestAsync(TelegramApiClientConstants.SetWebhookMethod, payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the webhook (switches to polling mode).
    /// </summary>
    public async Task<bool> RemoveWebhookAsync()
    {
        return await SendApiRequestAsync(
            TelegramApiClientConstants.SetWebhookMethod,
            new { url = string.Empty }).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the list of bot commands shown in the Telegram UI.
    /// </summary>
    /// <param name="commands">Collection of command name / description pairs.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>True if the request succeeded, false otherwise.</returns>
    public async Task<bool> SetMyCommandsAsync(IReadOnlyList<BotCommand> commands, CancellationToken cancellationToken = default)
    {
        if (commands == null)
            throw new ArgumentNullException(nameof(commands));

        cancellationToken.ThrowIfCancellationRequested();

        var payload = new
        {
            commands = commands.Select(c => new { command = c.Command, description = c.Description })
        };

        return await SendApiRequestAsync(
            TelegramApiClientConstants.SetMyCommandsMethod,
            payload,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches pending updates from Telegram using long polling.
    /// </summary>
    /// <param name="offset">Identifier of the first update to return; pass the last processed update ID + 1 to avoid duplicates.</param>
    /// <param name="timeoutSeconds">Long-polling timeout in seconds.</param>
    /// <returns>The raw update objects returned by Telegram, or an empty list if the call failed.</returns>
    public async Task<IReadOnlyList<JsonElement>> GetUpdatesAsync(
        long offset = 0,
        int timeoutSeconds = TelegramApiClientConstants.DefaultGetUpdatesTimeoutSeconds)
    {
        var method = string.Format(
            CultureInfo.InvariantCulture,
            TelegramApiClientConstants.GetUpdatesQueryStringFormat,
            offset,
            timeoutSeconds);
        var json = await GetApiRequestAsync(method).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<JsonElement>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
                return Array.Empty<JsonElement>();

            if (!root.TryGetProperty("result", out var resultElement) || resultElement.ValueKind != JsonValueKind.Array)
                return Array.Empty<JsonElement>();

            return resultElement.EnumerateArray().Select(static element => element.Clone()).ToArray();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse getUpdates response");
            return Array.Empty<JsonElement>();
        }
    }

    /// <summary>
    /// Gets information about a file stored on Telegram servers.
    /// </summary>
    /// <param name="fileId">File identifier to get info for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File information including file path and size, or null if not found</returns>
    public async Task<FileInfoResult?> GetFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("File ID cannot be empty", nameof(fileId));

        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var url = string.Format(
                CultureInfo.InvariantCulture,
                TelegramApiClientConstants.GetFileUrlFormat,
                _botToken,
                Uri.EscapeDataString(fileId));

            var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean() &&
                    root.TryGetProperty("result", out var resultElement))
                {
                    var fileIdResult = resultElement.TryGetProperty("file_id", out var fileIdProp) ? fileIdProp.GetString() : null;
                    var fileUniqueId = resultElement.TryGetProperty("file_unique_id", out var fileUniqueIdProp) ? fileUniqueIdProp.GetString() : null;
                    var filePath = resultElement.TryGetProperty("file_path", out var filePathProp) ? filePathProp.GetString() : null;
                    var fileSize = resultElement.TryGetProperty("file_size", out var fileSizeProp) ? fileSizeProp.GetInt64() : 0L;
                    var createdAt = resultElement.TryGetProperty("created_at", out var createdAtProp) ?
                        DateTimeOffset.FromUnixTimeSeconds(createdAtProp.GetInt64()) : DateTimeOffset.UtcNow;

                    if (filePath != null)
                    {
                        return new FileInfoResult(
                            fileIdResult ?? fileId,
                            fileUniqueId ?? string.Empty,
                            filePath,
                            fileSize,
                            createdAt
                        );
                    }
                }
            }

            _logger.LogWarning("Failed to get file info for file_id: {FileId}, Status: {StatusCode}", fileId, response.StatusCode);
            return null;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetFile operation was cancelled for file_id: {FileId}", fileId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for file_id: {FileId}", fileId);
            return null;
        }
    }


    private static string GetMediaFieldName(MediaType type)
    {
        return type switch
        {
            MediaType.Photo => "photo",
            MediaType.Video => "video",
            MediaType.Audio => "audio",
            MediaType.Document => "document",
            _ => "photo"
        };
    }

    private async Task<bool> SendApiRequestAsync<T>(string method, T payload, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var url = string.Format(
                CultureInfo.InvariantCulture,
                TelegramApiClientConstants.BotApiUrlFormat,
                _botToken,
                method);

            var json = JsonUtility.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, TelegramApiClientConstants.JsonContentType);

            // Determine if method is idempotent (safe to retry)
            var isIdempotent = IsIdempotentMethod(method);

            var response = await _retryHandler.ExecuteWithRetryAsync(
                client,
                url,
                content,
                method,
                isIdempotent,
                cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Telegram API call succeeded: {Method}", method);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Telegram API call failed: {Method}, Status: {StatusCode}, Error: {Error}", method, response.StatusCode, errorContent);

            return false;
        }
        catch (TelegramRateLimitedException ex)
        {
            _logger.LogWarning(ex, "Rate limited calling Telegram API method: {Method}", method);
            return false;
        }
        catch (TelegramServerException ex)
        {
            _logger.LogWarning(ex, "Server error calling Telegram API method: {Method}", method);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Telegram API method: {Method}. {RedactedMessage}", method, TokenRedaction.RedactToken(ex));
            return false;
        }
    }

    private async Task<string?> GetApiRequestAsync(string method)
    {
        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var url = string.Format(
                CultureInfo.InvariantCulture,
                TelegramApiClientConstants.BotApiUrlFormat,
                _botToken,
                method);

            // GET requests are generally idempotent
            var response = await _retryHandler.ExecuteGetWithRetryAsync(
                client,
                url,
                method).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            _logger.LogWarning("Telegram API GET call failed: {Method}, Status: {StatusCode}", method, response.StatusCode);

            return null;
        }
        catch (TelegramRateLimitedException ex)
        {
            _logger.LogWarning(ex, "Rate limited calling Telegram API GET method: {Method}", method);
            return null;
        }
        catch (TelegramServerException ex)
        {
            _logger.LogWarning(ex, "Server error calling Telegram API GET method: {Method}", method);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Telegram API GET method: {Method}. {RedactedMessage}", method, TokenRedaction.RedactToken(ex));
            return null;
        }
    }

    /// <summary>
    /// Determines if a Telegram API method is idempotent (safe to retry).
    /// </summary>
    /// <param name="method">The Telegram API method name.</param>
    /// <returns>True if the method is idempotent, false otherwise.</returns>
    private bool IsIdempotentMethod(string method)
    {
        // Methods that modify state and should not be retried blindly
        if (TelegramApiClientConstants.NonIdempotentMethods.Contains(method))
            return false;

        // Methods that are generally safe to retry (idempotent)
        if (TelegramApiClientConstants.IdempotentMethods.Contains(method))
            return true;

        // Default to non-idempotent for safety (don't retry methods that modify state)
        return false;
    }
}

// Dummy logger for demonstration when DI logger not available
internal sealed class ConsoleLogger<T> : ILogger<T>
{
    IDisposable? ILogger.BeginScope<TState>(TState state) => new NullDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}

internal sealed class NullDisposable : IDisposable { public void Dispose() { } }

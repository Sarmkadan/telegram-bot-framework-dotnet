#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System.Globalization;
using System.Text;
using System.Text.Json;
using Utilities;

/// <summary>
/// Client for interacting with Telegram Bot API.
/// Provides methods for sending messages, managing updates, and querying bot state.
/// </summary>
public sealed class TelegramApiClient : ITelegramApiClient
{
    private readonly HttpClientFactory _httpClientFactory;
    private readonly string _botToken;
    private readonly ILogger<TelegramApiClient> _logger;

    public TelegramApiClient(string botToken, HttpClientFactory? httpClientFactory = null, ILogger<TelegramApiClient>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("Bot token cannot be empty", nameof(botToken));

        if (!ValidationUtility.IsValidTelegramToken(botToken))
            throw new ArgumentException("Invalid Telegram bot token format", nameof(botToken));

        _botToken = botToken;
        _httpClientFactory = httpClientFactory ?? new HttpClientFactory();
        _logger = logger ?? new ConsoleLogger<TelegramApiClient>();
    }

    /// <summary>
    /// Sends a simple text message to a chat.
    /// </summary>
    public async Task<bool> SendMessageAsync(long chatId, string text)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text cannot be empty", nameof(text));

        var payload = new { chat_id = chatId, text = text };
        return await SendApiRequestAsync("sendMessage", payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a message with inline keyboard buttons.
    /// </summary>
    public async Task<bool> SendMessageWithButtonsAsync(long chatId, string text, string[][] buttonLabels)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        var buttons = buttonLabels.Select(row =>
            row.Select(label => new { text = label, callback_data = label }).ToArray()
        ).ToArray();

        var payload = new
        {
            chat_id = chatId,
            text = text,
            reply_markup = new { inline_keyboard = buttons }
        };

        return await SendApiRequestAsync("sendMessage", payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Edits a previously sent message.
    /// </summary>
    public async Task<bool> EditMessageAsync(long chatId, int messageId, string newText)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (messageId <= 0)
            throw new ArgumentException("Message ID must be positive", nameof(messageId));

        var payload = new { chat_id = chatId, message_id = messageId, text = newText };
        return await SendApiRequestAsync("editMessageText", payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a message from a chat.
    /// </summary>
    public async Task<bool> DeleteMessageAsync(long chatId, int messageId)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        var payload = new { chat_id = chatId, message_id = messageId };
        return await SendApiRequestAsync("deleteMessage", payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a poll to a chat.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="question">Poll question (1-256 characters)</param>
    /// <param name="options">List of answer options (2-10 options, each 1-100 characters)</param>
    /// <param name="allowsMultipleAnswers">Whether users can select multiple answers</param>
    /// <returns>Message ID of the sent poll if successful, null otherwise</returns>
    public async Task<int?> SendPollAsync(long chatId, string question, string[] options, bool allowsMultipleAnswers = false)
    {
        if (!ValidationUtility.IsValidTelegramChatId(chatId))
            throw new ArgumentException("Invalid chat ID", nameof(chatId));

        if (string.IsNullOrWhiteSpace(question) || question.Length > 256)
            throw new ArgumentException("Question must be 1-256 characters", nameof(question));

        if (options == null || options.Length < 2 || options.Length > 10)
            throw new ArgumentException("Must provide 2-10 options", nameof(options));

        if (options.Any(o => string.IsNullOrWhiteSpace(o) || o.Length > 100))
            throw new ArgumentException("Each option must be 1-100 characters", nameof(options));

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
            var url = $"bot{_botToken}/sendPoll";

            var json = JsonUtility.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean() &&
                    root.TryGetProperty("result", out var resultElement) &&
                    resultElement.TryGetProperty("message_id", out var messageIdElement))
                {
                    return messageIdElement.GetInt32();
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogWarning("Poll send failed: Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending poll to Telegram API");
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

        if (items == null || items.Count < 2 || items.Count > 10)
            throw new ArgumentException("Must provide 2-10 media items", nameof(items));

        if (items.Any(item => item == null || string.IsNullOrWhiteSpace(item.FileIdOrUrl)))
            throw new ArgumentException("Each item must have a valid FileIdOrUrl", nameof(items));

        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var messageIds = new List<int>();

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var method = GetMediaMethod(item.Type);
                var payload = CreateMediaPayload(chatId, item);

                var json = JsonUtility.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"bot{_botToken}/{method}";
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
                        messageIds.Add(messageIdElement.GetInt32());
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogWarning("Media item send failed: Type={Type}, Status={StatusCode}, Error={Error}",
                        item.Type, response.StatusCode, errorContent);
                }
            }

            _logger.LogInformation("Sent media group with {Count} items to chat {ChatId}", items.Count, chatId);
            return messageIds;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Media group send operation was cancelled");
            return new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending media group to Telegram API");
            return new List<int>();
        }
    }

    /// <summary>
    /// Gets information about the bot itself.
    /// </summary>
    public async Task<string?> GetMeAsync()
    {
        return await GetApiRequestAsync("getMe").ConfigureAwait(false);
    }

    /// <summary>
    /// Answers a callback query from an inline button press.
    /// </summary>
    public async Task<bool> AnswerCallbackQueryAsync(string callbackQueryId, string? notificationText = null)
    {
        if (string.IsNullOrWhiteSpace(callbackQueryId))
            throw new ArgumentException("Callback query ID cannot be empty", nameof(callbackQueryId));

        var payload = new { callback_query_id = callbackQueryId, text = notificationText };
        return await SendApiRequestAsync("answerCallbackQuery", payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the webhook URL for receiving updates.
    /// </summary>
    public async Task<bool> SetWebhookAsync(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl) || !ValidationUtility.IsValidUrl(webhookUrl))
            throw new ArgumentException("Invalid webhook URL", nameof(webhookUrl));

        var payload = new { url = webhookUrl };
        return await SendApiRequestAsync("setWebhook", payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the webhook (switches to polling mode).
    /// </summary>
    public async Task<bool> RemoveWebhookAsync()
    {
        return await SendApiRequestAsync("setWebhook", new { url = string.Empty }).ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches pending updates from Telegram using long polling.
    /// </summary>
    /// <param name="offset">Identifier of the first update to return; pass the last processed update ID + 1 to avoid duplicates.</param>
    /// <param name="timeoutSeconds">Long-polling timeout in seconds.</param>
    /// <returns>The raw update objects returned by Telegram, or an empty list if the call failed.</returns>
    public async Task<IReadOnlyList<JsonElement>> GetUpdatesAsync(long offset = 0, int timeoutSeconds = 30)
    {
        var method = $"getUpdates?offset={offset.ToString(CultureInfo.InvariantCulture)}&timeout={timeoutSeconds.ToString(CultureInfo.InvariantCulture)}";
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

    private string GetMediaMethod(MediaType type)
    {
        return type switch
        {
            MediaType.Photo => "sendPhoto",
            MediaType.Video => "sendVideo",
            MediaType.Audio => "sendAudio",
            MediaType.Document => "sendDocument",
            _ => "sendPhoto"
        };
    }

    private object CreateMediaPayload(long chatId, MediaGroupItem item)
    {
        var payload = new Dictionary<string, object> { { "chat_id", chatId } };

        if (item.Caption != null)
        {
            payload["caption"] = item.Caption;
        }

        // Telegram API expects media to be sent as file_id or URL
        payload[GetMediaFieldName(item.Type)] = item.FileIdOrUrl;

        return payload;
    }

    private string GetMediaFieldName(MediaType type)
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

    private async Task<bool> SendApiRequestAsync<T>(string method, T payload) where T : class
    {
        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var url = $"bot{_botToken}/{method}";

            var json = JsonUtility.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Telegram API call succeeded: {Method}", method);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogWarning("Telegram API call failed: {Method}, Status: {StatusCode}, Error: {Error}",
                method, response.StatusCode, errorContent);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Telegram API method: {Method}", method);
            return false;
        }
    }

    private async Task<string?> GetApiRequestAsync(string method)
    {
        try
        {
            var client = _httpClientFactory.GetTelegramClient();
            var url = $"bot{_botToken}/{method}";

            var response = await client.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            _logger.LogWarning("Telegram API GET call failed: {Method}, Status: {StatusCode}",
                method, response.StatusCode);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Telegram API GET method: {Method}", method);
            return null;
        }
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
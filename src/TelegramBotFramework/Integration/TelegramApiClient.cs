#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

using System.Text;
using System.Text.Json;
using Utilities;

/// <summary>
/// Client for interacting with Telegram Bot API.
/// Provides methods for sending messages, managing updates, and querying bot state.
/// </summary>
public sealed class TelegramApiClient
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
    public IDisposable BeginScope<TState>(TState state) => new NullDisposable();
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}

internal sealed class NullDisposable : IDisposable { public void Dispose() { } }
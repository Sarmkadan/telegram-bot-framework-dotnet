// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

using System.Text.Json;

/// <summary>
/// Handles incoming webhook updates from Telegram and processes them.
/// Validates update authenticity and dispatches to appropriate handlers.
/// </summary>
public class WebhookHandler
{
    private readonly ILogger<WebhookHandler> _logger;

    public WebhookHandler(ILogger<WebhookHandler>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<WebhookHandler>();
    }

    /// <summary>
    /// Processes incoming webhook JSON data from Telegram.
    /// </summary>
    public async Task<TelegramUpdate?> ProcessUpdateAsync(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogWarning("Received empty webhook data");
            return null;
        }

        try
        {
            var doc = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;

            var update = new TelegramUpdate
            {
                UpdateId = root.GetProperty("update_id").GetInt64(),
                Timestamp = DateTime.UtcNow
            };

            // Check for message update
            if (root.TryGetProperty("message", out var messageElement))
            {
                update.MessageType = UpdateType.Message;
                update.Message = ParseTelegramMessage(messageElement);
            }
            // Check for callback query (button click)
            else if (root.TryGetProperty("callback_query", out var callbackElement))
            {
                update.MessageType = UpdateType.CallbackQuery;
                update.CallbackData = callbackElement.GetProperty("data").GetString();
                update.CallbackQueryId = callbackElement.GetProperty("id").GetString();

                if (callbackElement.TryGetProperty("message", out var cbMessage))
                {
                    update.Message = ParseTelegramMessage(cbMessage);
                }
            }
            // Check for edited message
            else if (root.TryGetProperty("edited_message", out var editedMsgElement))
            {
                update.MessageType = UpdateType.EditedMessage;
                update.Message = ParseTelegramMessage(editedMsgElement);
            }
            // Check for inline query
            else if (root.TryGetProperty("inline_query", out var inlineElement))
            {
                update.MessageType = UpdateType.InlineQuery;
                update.InlineQuery = inlineElement.GetProperty("query").GetString();
            }

            _logger.LogInformation("Successfully parsed webhook update {UpdateId} of type {Type}",
                update.UpdateId, update.MessageType);

            return update;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse webhook JSON data");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook update");
            return null;
        }
    }

    /// <summary>
    /// Validates the webhook request authenticity (if configured).
    /// </summary>
    public bool ValidateWebhookRequest(string jsonData, string? signature, string? secretKey)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secretKey))
        {
            // Skip validation if no secret key configured
            return true;
        }

        try
        {
            // Compute HMAC-SHA256 of the payload
            var computedSignature = Utilities.CryptoUtility.ComputeHmacSHA256(jsonData, secretKey);

            // Compare signatures (should be timing-safe in production)
            return computedSignature.Equals(signature, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook request");
            return false;
        }
    }

    private TelegramMessage? ParseTelegramMessage(JsonElement messageElement)
    {
        if (!messageElement.TryGetProperty("message_id", out var messageIdElement))
            return null;

        var message = new TelegramMessage
        {
            MessageId = messageIdElement.GetInt64(),
            ChatId = messageElement.GetProperty("chat").GetProperty("id").GetInt64(),
            UserId = messageElement.GetProperty("from").GetProperty("id").GetInt64(),
            Timestamp = UnixTimeStampToDateTime(messageElement.GetProperty("date").GetInt64()),
            Text = messageElement.TryGetProperty("text", out var textElement) ? textElement.GetString() : null
        };

        if (messageElement.TryGetProperty("edit_date", out var editDateElement))
        {
            message.EditedTimestamp = UnixTimeStampToDateTime(editDateElement.GetInt64());
        }

        return message;
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }
}

/// <summary>
/// Represents a Telegram bot update received via webhook.
/// </summary>
public class TelegramUpdate
{
    public long UpdateId { get; set; }
    public UpdateType MessageType { get; set; }
    public DateTime Timestamp { get; set; }
    public TelegramMessage? Message { get; set; }
    public string? CallbackData { get; set; }
    public string? CallbackQueryId { get; set; }
    public string? InlineQuery { get; set; }
}

/// <summary>
/// Represents a Telegram message.
/// </summary>
public class TelegramMessage
{
    public long MessageId { get; set; }
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string? Text { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime? EditedTimestamp { get; set; }
}

/// <summary>
/// Types of Telegram updates.
/// </summary>
public enum UpdateType
{
    Message,
    CallbackQuery,
    EditedMessage,
    InlineQuery,
    Unknown
}

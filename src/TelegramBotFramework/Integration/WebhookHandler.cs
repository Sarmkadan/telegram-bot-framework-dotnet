#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Handles incoming webhook updates from Telegram and processes them.
/// Validates update authenticity and dispatches to appropriate handlers.
/// </summary>
public sealed class WebhookHandler : IWebhookHandler
{
    // Maximum allowed lengths for various Telegram message components to prevent DoS attacks
    private const int MaxMessageTextLength = 10_000; // 10KB - Telegram's typical limit is 4096 chars
    private const int MaxCaptionLength = 10_000; // 10KB - same as message text
    private const int MaxEntityCount = 100; // Maximum number of message entities (mentions, links, etc.)
    private const int MaxInlineKeyboardRows = 100; // Maximum number of rows in inline keyboard
    private const int MaxInlineKeyboardColumns = 10; // Maximum number of buttons per row
    private const int MaxMessageLength = 20_000; // 20KB - overall message size limit
    private const int MaxCallbackDataLength = 1_000; // 1KB - callback data limit

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

                // Parse and validate callback query ID
                var callbackQueryId = callbackElement.GetProperty("id").GetString();
                if (callbackQueryId != null && callbackQueryId.Length > MaxCallbackDataLength)
                {
                    _logger.LogWarning("Callback query ID too long ({Length} chars, max {MaxLength}). Truncating.", callbackQueryId.Length, MaxCallbackDataLength);
                    update.CallbackQueryId = callbackQueryId[..MaxCallbackDataLength];
                }
                else
                {
                    update.CallbackQueryId = callbackQueryId;
                }

                // Parse and validate callback data with length limit
                if (callbackElement.TryGetProperty("data", out var callbackDataProperty))
                {
                    var callbackData = callbackDataProperty.GetString();
                    if (callbackData != null)
                    {
                        if (callbackData.Length > MaxCallbackDataLength)
                        {
                            _logger.LogWarning("Callback data too long ({Length} chars, max {MaxLength}). Truncating.", callbackData.Length, MaxCallbackDataLength);
                            update.CallbackData = callbackData[..MaxCallbackDataLength];
                        }
                        else
                        {
                            update.CallbackData = callbackData;
                        }
                    }
                }

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
                var inlineQuery = inlineElement.GetProperty("query").GetString();
                if (inlineQuery != null && inlineQuery.Length > MaxMessageTextLength)
                {
                    _logger.LogWarning("Inline query too long ({Length} chars, max {MaxLength}). Truncating.", inlineQuery.Length, MaxMessageTextLength);
                    update.InlineQuery = inlineQuery[..MaxMessageTextLength];
                }
                else
                {
                    update.InlineQuery = inlineQuery;
                }
            }

            _logger.LogInformation("Successfully parsed webhook update {UpdateId} of type {Type}", update.UpdateId, update.MessageType);

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
    /// Validates the webhook request authenticity by comparing the X-Telegram-Bot-Api-Secret-Token header
    /// against the configured secret using constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="secretTokenHeader">The value of the X-Telegram-Bot-Api-Secret-Token header from the request.</param>
    /// <param name="configuredSecret">The configured secret token from WebhookOptions.</param>
    /// <returns>True if the tokens match or no secret is configured; false otherwise.</returns>
    public bool ValidateSecretToken(string? secretTokenHeader, string? configuredSecret)
    {
        // If no secret is configured, skip validation (for backward compatibility)
        if (string.IsNullOrEmpty(configuredSecret))
        {
            return true;
        }

        // If header is missing but secret is configured, reject
        if (string.IsNullOrEmpty(secretTokenHeader))
        {
            _logger.LogWarning("Webhook request rejected: X-Telegram-Bot-Api-Secret-Token header is missing");
            return false;
        }

        // Use constant-time comparison to prevent timing attacks
        var isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(secretTokenHeader),
            Encoding.UTF8.GetBytes(configuredSecret));

        if (!isValid)
        {
            _logger.LogWarning("Webhook request rejected: secret token mismatch");
        }

        return isValid;
    }

    private TelegramMessage? ParseTelegramMessage(JsonElement messageElement)
    {
        if (!messageElement.TryGetProperty("message_id", out var messageIdElement))
            return null;

        // Extract basic message properties with bounds checking
        var messageId = messageIdElement.GetInt64();
        var chatId = messageElement.GetProperty("chat").GetProperty("id").GetInt64();
        var userId = messageElement.GetProperty("from").GetProperty("id").GetInt64();
        var timestamp = UnixTimeStampToDateTime(messageElement.GetProperty("date").GetInt64());

        // Initialize message with basic properties
        var message = new TelegramMessage
        {
            MessageId = messageId,
            ChatId = chatId,
            UserId = userId,
            Timestamp = timestamp,
            Text = null
        };

        // Parse and validate message text with length limit
        if (messageElement.TryGetProperty("text", out var textElement))
        {
            var text = textElement.GetString();
            if (text != null)
            {
                if (text.Length > MaxMessageTextLength)
                {
                    _logger.LogWarning("Message text too long ({Length} chars, max {MaxLength}). Truncating.", text.Length, MaxMessageTextLength);
                    message.Text = text[..MaxMessageTextLength]; // Truncate to max length
                }
                else
                {
                    message.Text = text;
                }
            }
        }

        // Parse and validate caption for media messages
        if (messageElement.TryGetProperty("caption", out var captionElement))
        {
            var caption = captionElement.GetString();
            if (caption != null && caption.Length > MaxCaptionLength)
            {
                _logger.LogWarning("Message caption too long ({Length} chars, max {MaxLength}). Truncating.", caption.Length, MaxCaptionLength);
                // Note: We can't set caption here as TelegramMessage doesn't have a Caption property
                // The caption will be truncated when the message is processed by the API client
            }
        }

        // Parse and validate message entities (mentions, links, etc.)
        if (messageElement.TryGetProperty("entities", out var entitiesElement) && entitiesElement.ValueKind == JsonValueKind.Array)
        {
            var entities = new List<JsonElement>();
            foreach (var entity in entitiesElement.EnumerateArray())
            {
                entities.Add(entity.Clone());
                if (entities.Count > MaxEntityCount)
                {
                    _logger.LogWarning("Message contains too many entities ({Count}, max {MaxCount}). Truncating entities list.", entities.Count, MaxEntityCount);
                    break;
                }
            }

            // Store entities for potential processing, but limit the count
            message.Entities = entities.Count <= MaxEntityCount ? entities : entities.Take(MaxEntityCount).ToList();
        }

        // Parse and validate inline keyboard (reply_markup)
        if (messageElement.TryGetProperty("reply_markup", out var replyMarkupElement))
        {
            if (TryParseInlineKeyboard(replyMarkupElement, out var keyboard))
            {
                message.InlineKeyboard = keyboard;
            }
        }

        // Parse edit date if present
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

    /// <summary>
    /// Attempts to parse an inline keyboard from reply_markup JSON element.
    /// Validates keyboard dimensions and rejects excessively large keyboards.
    /// </summary>
    private bool TryParseInlineKeyboard(JsonElement replyMarkupElement, out List<List<InlineKeyboardButton>> keyboard)
    {
        keyboard = new List<List<InlineKeyboardButton>>();

        // Check if it's an inline keyboard
        if (!replyMarkupElement.TryGetProperty("inline_keyboard", out var inlineKeyboardElement) ||
            inlineKeyboardElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var rowCount = 0;
        foreach (var rowElement in inlineKeyboardElement.EnumerateArray())
        {
            if (rowCount >= MaxInlineKeyboardRows)
            {
                _logger.LogWarning("Inline keyboard has too many rows ({Count}, max {MaxCount}). Truncating.", rowCount, MaxInlineKeyboardRows);
                break;
            }

            var row = new List<InlineKeyboardButton>();
            var buttonCount = 0;

            foreach (var buttonElement in rowElement.EnumerateArray())
            {
                if (buttonCount >= MaxInlineKeyboardColumns)
                {
                    _logger.LogWarning("Inline keyboard row has too many buttons ({Count}, max {MaxCount}). Truncating row.", buttonCount, MaxInlineKeyboardColumns);
                    break;
                }

                try
                {
                    var button = ParseInlineKeyboardButton(buttonElement);
                    if (button != null)
                    {
                        row.Add(button);
                        buttonCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse inline keyboard button. Skipping.");
                }

                if (row.Count > 0)
                {
                    keyboard.Add(row);
                    rowCount++;
                }
            }
        }

        return keyboard.Count > 0;
    }

    /// <summary>
    /// Parses a single inline keyboard button from JSON.
    /// </summary>
    private InlineKeyboardButton? ParseInlineKeyboardButton(JsonElement buttonElement)
    {
        if (!buttonElement.TryGetProperty("text", out var textElement))
        {
            return null;
        }

        var text = textElement.GetString();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        // Validate button text length
        if (text.Length > MaxMessageTextLength)
        {
            _logger.LogWarning("Inline keyboard button text too long ({Length} chars, max {MaxLength}). Truncating.", text.Length, MaxMessageTextLength);
            text = text[..MaxMessageTextLength];
        }

        // Check for callback_data
        string? callbackData = null;
        if (buttonElement.TryGetProperty("callback_data", out var callbackDataElement))
        {
            callbackData = callbackDataElement.GetString();
            if (!string.IsNullOrEmpty(callbackData) && callbackData.Length > MaxCallbackDataLength)
            {
                _logger.LogWarning("Inline keyboard button callback_data too long ({Length} chars, max {MaxLength}). Truncating.", callbackData.Length, MaxCallbackDataLength);
                callbackData = callbackData[..MaxCallbackDataLength];
            }
        }

        return new InlineKeyboardButton { Text = text, CallbackData = callbackData };
    }
}

/// <summary>
/// Represents a Telegram bot update received via webhook.
/// </summary>
public sealed class TelegramUpdate
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
public sealed class TelegramMessage
{
    public long MessageId { get; set; }
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string? Text { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime? EditedTimestamp { get; set; }
    public List<JsonElement>? Entities { get; set; }
    public List<List<InlineKeyboardButton>>? InlineKeyboard { get; set; }
}

/// <summary>
/// Represents a button in an inline keyboard.
/// </summary>
public sealed class InlineKeyboardButton
{
    public string Text { get; set; } = string.Empty;
    public string? CallbackData { get; set; }
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
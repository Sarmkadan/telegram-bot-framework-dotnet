#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Handles incoming webhook updates from Telegram and processes them.
/// Validates update authenticity and dispatches to appropriate handlers.
/// </summary>
public sealed class WebhookHandler
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

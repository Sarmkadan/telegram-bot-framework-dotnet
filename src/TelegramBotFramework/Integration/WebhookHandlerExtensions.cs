#nullable enable

namespace TelegramBotFramework.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookHandler"/> to provide additional functionality
/// for processing and analyzing Telegram webhook updates.
/// </summary>
public static class WebhookHandlerExtensions
{
    /// <summary>
    /// Extracts the message text from the webhook update if available.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="update">The Telegram update to extract text from.</param>
    /// <returns>The message text if available, otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="update"/> is <see langword="null"/></exception>
    public static string? GetMessageText(this WebhookHandler handler, TelegramUpdate update)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(update);

        return update.Message?.Text;
    }

    /// <summary>
    /// Checks if the update contains a callback query with the specified data.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="update">The Telegram update to check.</param>
    /// <param name="callbackData">The callback data to match against.</param>
    /// <returns>True if the callback data matches, otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="update"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="callbackData"/> is <see langword="null"/> or whitespace.</exception>
    public static bool HasCallbackData(this WebhookHandler handler, TelegramUpdate update, string callbackData)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(callbackData);

        return update.CallbackData?.Equals(callbackData, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Gets the chat identifier from the webhook update if available.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="update">The Telegram update to extract chat ID from.</param>
    /// <returns>The chat identifier if available, otherwise 0.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="update"/> is <see langword="null"/></exception>
    public static long GetChatId(this WebhookHandler handler, TelegramUpdate update)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(update);

        return update.Message?.ChatId ?? 0;
    }

    /// <summary>
    /// Gets the user identifier from the webhook update if available.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="update">The Telegram update to extract user ID from.</param>
    /// <returns>The user identifier if available, otherwise 0.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="update"/> is <see langword="null"/></exception>
    public static long GetUserId(this WebhookHandler handler, TelegramUpdate update)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(update);

        return update.Message?.UserId ?? 0;
    }
}
#nullable enable

namespace TelegramBotFramework.Integration;

/// <summary>
/// Configuration options for webhook mode.
/// </summary>
public interface IWebhookOptions
{
    /// <summary>
    /// Gets or sets the HTTPS URL Telegram will send updates to.
    /// Must be reachable from the public internet on port 443, 80, 88, or 8443.
    /// </summary>
    string Url { get; set; }

    /// <summary>
    /// Gets or sets an optional secret token (1–256 ASCII characters) that Telegram
    /// includes in the <c>X-Telegram-Bot-Api-Secret-Token</c> header of every request.
    /// Use this to verify that requests genuinely originate from Telegram.
    /// </summary>
    string? SecretToken { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of simultaneous HTTPS connections Telegram
    /// may use for delivering updates. Accepted values: 1–100. Defaults to <c>40</c>.
    /// </summary>
    int MaxConnections { get; set; }

    /// <summary>
    /// Gets or sets the list of update types the bot should receive.
    /// Passing <c>null</c> or an empty array instructs Telegram to send all types.
    /// </summary>
    string[]? AllowedUpdates { get; set; }

    /// <summary>
    /// Gets or sets the relative path at which the webhook controller listens.
    /// Defaults to <c>/api/webhook/telegram</c>.
    /// </summary>
    string ListenPath { get; set; }

    /// <summary>
    /// Gets or sets whether pending updates that accumulated while the webhook was
    /// not configured should be discarded on registration. Defaults to <c>false</c>.
    /// </summary>
    bool DropPendingUpdates { get; set; }

    /// <summary>Validates required fields.</summary>
    void Validate();
}
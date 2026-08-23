#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Linq;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Configuration options for webhook mode.
/// Pass an instance to <c>services.AddWebhookMode(options)</c> or configure
/// via <see cref="BotConfiguration.WebhookUrl"/> and <see cref="BotConfiguration.WebhookSecret"/>.
/// </summary>
public sealed class WebhookOptions : IEquatable<WebhookOptions>
{
    /// <summary>
    /// Gets or sets the HTTPS URL Telegram will send updates to.
    /// Must be reachable from the public internet on port 443, 80, 88, or 8443.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional secret token (1–256 ASCII characters) that Telegram
    /// includes in the <c>X-Telegram-Bot-Api-Secret-Token</c> header of every request.
    /// Use this to verify that requests genuinely originate from Telegram.
    /// </summary>
    public string? SecretToken { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of simultaneous HTTPS connections Telegram
    /// may use for delivering updates. Accepted values: 1–100. Defaults to <c>40</c>.
    /// </summary>
    public int MaxConnections { get; set; } = 40;

    /// <summary>
    /// Gets or sets the list of update types the bot should receive.
    /// Passing <c>null</c> or an empty array instructs Telegram to send all types.
    /// </summary>
    public string[]? AllowedUpdates { get; set; }

    /// <summary>
    /// Gets or sets the relative path at which the webhook controller listens.
    /// Defaults to <c>/api/webhook/telegram</c>.
    /// </summary>
    public string ListenPath { get; set; } = "/api/webhook/telegram";

    /// <summary>
    /// Gets or sets whether pending updates that accumulated while the webhook was
    /// not configured should be discarded on registration. Defaults to <c>false</c>.
    /// </summary>
    public bool DropPendingUpdates { get; set; }

    /// <summary>
    /// Gets or sets the deduplication window for update_id tracking.
    /// Updates with the same update_id within this window will be deduplicated.
    /// Defaults to <c>TimeSpan.FromMinutes(5)</c>.
    /// </summary>
    public TimeSpan UpdateDeduplicationWindow { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum allowed size in bytes for incoming webhook request bodies.
    /// Prevents denial-of-service attacks via oversized payloads.
    /// Defaults to <c>1 MB (1_048_576 bytes)</c>.
    /// </summary>
    public long MaxRequestBodySize { get; set; } = 1_048_576; // 1 MB

    public bool Equals(WebhookOptions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Url == other.Url &&
               SecretToken == other.SecretToken &&
               MaxConnections == other.MaxConnections &&
               (AllowedUpdates == null ? other.AllowedUpdates == null : other.AllowedUpdates != null && AllowedUpdates.SequenceEqual(other.AllowedUpdates)) &&
               ListenPath == other.ListenPath &&
               DropPendingUpdates == other.DropPendingUpdates;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as WebhookOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Url, SecretToken, MaxConnections, AllowedUpdates != null ? AllowedUpdates.GetHashCode() : 0, ListenPath, DropPendingUpdates);
    }

    public static bool operator ==(WebhookOptions? left, WebhookOptions? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(WebhookOptions? left, WebhookOptions? right)
    {
        return !Equals(left, right);
    }

    /// <summary>Validates required fields.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new InvalidOperationException("WebhookOptions.Url must be set.");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new InvalidOperationException($"WebhookOptions.Url '{Url}' is not a valid absolute URL.");

        if (MaxConnections is < 1 or > 100)
            throw new InvalidOperationException("MaxConnections must be between 1 and 100.");

        if (!string.IsNullOrEmpty(ListenPath) && !ListenPath.StartsWith('/'))
            throw new InvalidOperationException("ListenPath must start with '/'");

        if (UpdateDeduplicationWindow <= TimeSpan.Zero)
            throw new InvalidOperationException("UpdateDeduplicationWindow must be positive.");

        if (MaxRequestBodySize <= 0)
            throw new InvalidOperationException("MaxRequestBodySize must be positive.");
    }
}
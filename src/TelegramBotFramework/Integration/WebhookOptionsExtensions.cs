namespace TelegramBotFramework.Integration;

/// <summary>
/// Provides extension methods for <see cref="WebhookOptions"/> to simplify common webhook configuration checks.
/// </summary>
public static class WebhookOptionsExtensions
{
    /// <summary>
    /// Gets a value indicating whether the webhook options allow updates.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <returns><see langword="true"/> if the webhook options allow updates; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool AreUpdatesAllowed(this WebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.AllowedUpdates is null or { Length: > 0 };
    }

    /// <summary>
    /// Gets the maximum allowed connections as a <see cref="long"/> value.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <returns>The maximum allowed connections.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public static long GetMaxConnectionsAsLong(this WebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.MaxConnections;
    }

    /// <summary>
    /// Determines whether the webhook options have a secret token configured.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <returns><see langword="true"/> if the webhook options have a secret token configured; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool HasSecretToken(this WebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return !string.IsNullOrEmpty(options.SecretToken);
    }
}

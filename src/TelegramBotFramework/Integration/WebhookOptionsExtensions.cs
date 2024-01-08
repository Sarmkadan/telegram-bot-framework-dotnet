namespace TelegramBotFramework.Integration;

public static class WebhookOptionsExtensions
{
    /// <summary>
    /// Gets a value indicating whether the webhook options allow updates.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <returns>true if the webhook options allow updates; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool AreUpdatesAllowed(this WebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.AllowedUpdates == null || options.AllowedUpdates.Length > 0;
    }

    /// <summary>
    /// Gets the maximum allowed connections as an <see cref="long"/> value.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <returns>The maximum allowed connections.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static long GetMaxConnectionsAsLong(this WebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.MaxConnections;
    }

    /// <summary>
    /// Determines whether the webhook options have a secret token configured.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <returns>true if the webhook options have a secret token configured; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool HasSecretToken(this WebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return !string.IsNullOrEmpty(options.SecretToken);
    }
}

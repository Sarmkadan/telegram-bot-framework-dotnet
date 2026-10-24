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

    /// <summary>
    /// Ensures the webhook options carry a secret token. When none is configured, a cryptographically random
    /// token is generated and stored in <see cref="WebhookOptions.SecretToken"/>; a configured token is kept.
    /// </summary>
    /// <param name="options">The webhook options.</param>
    /// <param name="length">The length of a generated token, between 1 and <see cref="WebhookSecretToken.MaxLength"/>.</param>
    /// <returns>The configured or newly generated secret token.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is outside the valid token length range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the configured secret token contains characters or a length Telegram does not accept.</exception>
    public static string EnsureSecretToken(this WebhookOptions options, int length = WebhookSecretTokenConstants.DefaultLength)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(options.SecretToken))
        {
            options.SecretToken = WebhookSecretToken.Generate(length);
        }
        else if (!WebhookSecretToken.IsValid(options.SecretToken))
        {
            throw new InvalidOperationException(WebhookSecretTokenConstants.InvalidSecretTokenMessage);
        }

        return options.SecretToken;
    }
}

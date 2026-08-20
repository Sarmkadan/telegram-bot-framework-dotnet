#nullable enable

namespace TelegramBotFramework.Integration;

/// <summary>
/// Builder for <see cref="WebhookOptions"/> objects.
/// </summary>
public sealed class WebhookOptionsBuilder
{
    private WebhookOptions _options = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookOptionsBuilder"/> class.
    /// </summary>
    private WebhookOptionsBuilder()
    {
    }

    /// <summary>
    /// Creates a new builder pre-filled with values from an existing <see cref="WebhookOptions"/> instance.
    /// </summary>
    /// <param name="template">The template to copy values from.</param>
    /// <returns>A new builder instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <see langword="null"/>.</exception>
    public static WebhookOptionsBuilder From(WebhookOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new WebhookOptionsBuilder
        {
            _options = new WebhookOptions
            {
                Url = template.Url,
                SecretToken = template.SecretToken,
                MaxConnections = template.MaxConnections,
                AllowedUpdates = template.AllowedUpdates is not null ? (string[])template.AllowedUpdates.Clone() : null,
                ListenPath = template.ListenPath,
                DropPendingUpdates = template.DropPendingUpdates,
                UpdateDeduplicationWindow = template.UpdateDeduplicationWindow,
                MaxRequestBodySize = template.MaxRequestBodySize
            }
        };
    }

    /// <summary>
    /// Creates a new builder with default values.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static WebhookOptionsBuilder Create() => new WebhookOptionsBuilder();

    /// <summary>
    /// Sets the HTTPS URL Telegram will send updates to.
    /// </summary>
    /// <param name="url">The URL. Must not be null or empty.</param>
    /// <returns>The same builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">If <paramref name="url"/> is null or empty.</exception>
    public WebhookOptionsBuilder WithUrl(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        _options.Url = url;
        return this;
    }

    /// <summary>
    /// Sets the optional secret token used to verify requests originate from Telegram.
    /// </summary>
    /// <param name="secretToken">The secret token (1-256 ASCII characters), or <see langword="null"/> to disable.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public WebhookOptionsBuilder WithSecretToken(string? secretToken)
    {
        _options.SecretToken = secretToken;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of simultaneous HTTPS connections Telegram may use.
    /// </summary>
    /// <param name="maxConnections">The maximum connections. Must be between 1 and 100.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public WebhookOptionsBuilder WithMaxConnections(int maxConnections)
    {
        _options.MaxConnections = maxConnections;
        return this;
    }

    /// <summary>
    /// Sets the list of update types the bot should receive.
    /// </summary>
    /// <param name="allowedUpdates">The update types, or <see langword="null"/> to receive all types.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public WebhookOptionsBuilder WithAllowedUpdates(string[]? allowedUpdates)
    {
        _options.AllowedUpdates = allowedUpdates is not null ? (string[])allowedUpdates.Clone() : null;
        return this;
    }

    /// <summary>
    /// Sets the relative path at which the webhook controller listens.
    /// </summary>
    /// <param name="listenPath">The path. Must start with '/' if not empty.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public WebhookOptionsBuilder WithListenPath(string listenPath)
    {
        _options.ListenPath = listenPath;
        return this;
    }

    /// <summary>
    /// Sets whether pending updates should be discarded on webhook registration.
    /// </summary>
    /// <param name="dropPendingUpdates">Whether to drop pending updates.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public WebhookOptionsBuilder WithDropPendingUpdates(bool dropPendingUpdates)
    {
        _options.DropPendingUpdates = dropPendingUpdates;
        return this;
    }

    /// <summary>
    /// Builds and validates the <see cref="WebhookOptions"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="WebhookOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">If required properties are missing or invalid.</exception>
    public WebhookOptions Build()
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(_options.Url))
            throw new ArgumentException("WebhookOptions.Url must be set.");

        if (!Uri.TryCreate(_options.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new ArgumentException($"WebhookOptions.Url '{_options.Url}' is not a valid absolute URL.");

        if (_options.MaxConnections is < 1 or > 100)
            throw new ArgumentException("MaxConnections must be between 1 and 100.");

        if (!string.IsNullOrEmpty(_options.ListenPath) && !_options.ListenPath.StartsWith('/'))
            throw new ArgumentException("ListenPath must start with '/'");

        if (_options.UpdateDeduplicationWindow <= TimeSpan.Zero)
            throw new ArgumentException("UpdateDeduplicationWindow must be positive.");

        if (_options.MaxRequestBodySize <= 0)
            throw new ArgumentException("MaxRequestBodySize must be positive.");

        return _options;
    }
}
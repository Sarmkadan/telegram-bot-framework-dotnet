#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Manages the full lifecycle of webhook mode: registering the webhook URL with
/// Telegram on startup, dispatching received updates to subscribed handlers, and
/// removing the webhook on shutdown.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Raised whenever a validated update arrives at the webhook endpoint.
    /// Subscribe to this event to receive and process updates.
    /// </summary>
    event Func<TelegramUpdate, Task>? OnUpdateReceived;

    /// <summary>
    /// Registers the configured webhook URL with Telegram and makes the bot ready to
    /// receive updates over HTTPS. Called automatically on application startup when
    /// registered as a hosted service.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task RegisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the webhook registration from Telegram, causing the bot to stop receiving
    /// updates via push. Called automatically on application shutdown.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task UnregisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches an already-parsed update to all <see cref="OnUpdateReceived"/> subscribers.
    /// Typically called by the webhook controller upon receiving a valid request from Telegram.
    /// </summary>
    /// <param name="update">The parsed update to dispatch.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task DispatchUpdateAsync(TelegramUpdate update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses and validates a raw JSON payload received at the webhook endpoint.
    /// Returns the parsed update, or <c>null</c> if the payload is invalid or the
    /// secret-token check fails.
    /// </summary>
    /// <param name="jsonBody">The raw request body.</param>
    /// <param name="secretTokenHeader">
    /// Value of the <see cref="IWebhookServiceConstants.TelegramSecretTokenHeaderName"/> header, if present.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<TelegramUpdate?> ParseAndValidateAsync(
        string jsonBody,
        string? secretTokenHeader,
        CancellationToken cancellationToken = default);

    /// <summary>Gets current runtime info about the webhook.</summary>
    WebhookInfo GetInfo();
}

/// <summary>Runtime information about the current webhook state.</summary>
public sealed class WebhookInfo
{
    /// <summary>Gets a value indicating whether the webhook is currently registered.</summary>
    public bool IsRegistered { get; init; }

    /// <summary>Gets the registered webhook URL, or <c>null</c> when not registered.</summary>
    public string? Url { get; init; }

    /// <summary>Gets the UTC timestamp of the last successful registration, or <c>null</c>.</summary>
    public DateTime? RegisteredAt { get; init; }

    /// <summary>Gets the number of updates dispatched since the last registration.</summary>
    public long UpdatesDispatched { get; init; }
}

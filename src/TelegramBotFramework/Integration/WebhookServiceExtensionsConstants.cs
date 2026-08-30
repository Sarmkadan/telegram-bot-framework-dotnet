#nullable enable

namespace TelegramBotFramework.Integration;

/// <summary>
/// Constants for <see cref="WebhookServiceExtensions"/>.
/// </summary>
internal static class WebhookServiceExtensionsConstants
{
    /// <summary>
    /// Default maximum number of retry attempts for webhook registration.
    /// </summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>
    /// Default delay between retry attempts in milliseconds.
    /// </summary>
    public const int DefaultRetryDelayMs = 1000;

    /// <summary>
    /// Name of the logger field in <see cref="WebhookService"/>.
    /// </summary>
    public const string LoggerFieldName = "_logger";

    /// <summary>
    /// Name of the API client field in <see cref="WebhookService"/>.
    /// </summary>
    public const string ApiClientFieldName = "_apiClient";

    /// <summary>
    /// Name of the options field in <see cref="WebhookService"/>.
    /// </summary>
    public const string OptionsFieldName = "_options";

    /// <summary>
    /// Name of the updates dispatched field in <see cref="WebhookService"/>.
    /// </summary>
    public const string UpdatesDispatchedFieldName = "_updatesDispatched";

    /// <summary>
    /// Name of the registration timestamp field in <see cref="WebhookService"/>.
    /// </summary>
    public const string RegisteredAtFieldName = "_registeredAt";

    /// <summary>
    /// Log message for failed webhook registration attempt.
    /// </summary>
    public const string LogWebhookRegistrationAttemptFailed = "Webhook registration attempt {Attempt} of {MaxRetries} failed, retrying...";

    /// <summary>
    /// Log message for failed webhook unregistration.
    /// </summary>
    public const string LogWebhookUnregistrationFailed = "Failed to unregister webhook";

    /// <summary>
    /// Exception message when logger field is not found or invalid.
    /// </summary>
    public const string ExceptionLoggerNotFound = "Logger field not found or invalid.";

    /// <summary>
    /// Exception message when API client field is not found or invalid.
    /// </summary>
    public const string ExceptionApiClientNotFound = "API client field not found or invalid.";

    /// <summary>
    /// Exception message when options field is not found or invalid.
    /// </summary>
    public const string ExceptionOptionsNotFound = "Options field not found or invalid.";

    /// <summary>
    /// Exception message when webhook service is not found as hosted service.
    /// </summary>
    public const string ExceptionWebhookServiceNotFound = "WebhookService not found";
}
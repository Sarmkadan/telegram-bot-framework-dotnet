namespace TelegramBotFramework.Controllers;

/// <summary>
/// Constants for WebhookController.
/// </summary>
internal static class WebhookControllerConstants
{
    /// <summary>
    /// Base route for webhook endpoints.
    /// </summary>
    public const string Route = "api/webhook";

    /// <summary>
    /// Route for receiving Telegram webhook updates.
    /// </summary>
    public const string TelegramRoute = "telegram";

    /// <summary>
    /// Route for retrieving webhook information.
    /// </summary>
    public const string InfoRoute = "info";

    /// <summary>
    /// Media type accepted by the Telegram webhook endpoint.
    /// </summary>
    public const string JsonMediaType = "application/json";

    /// <summary>
    /// The header name for the Telegram secret token.
    /// </summary>
    public const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    /// <summary>
    /// Error message for request body too large.
    /// </summary>
    public const string RequestBodyTooLargeMessage = "Request body too large.";

    /// <summary>
    /// Error message for empty request body.
    /// </summary>
    public const string EmptyRequestBodyMessage = "Request body is required.";

    /// <summary>
    /// HTTP status code returned when the request body is too large.
    /// </summary>
    public const int PayloadTooLargeStatusCode = 413;

    /// <summary>
    /// HTTP status code returned when the update queue is full and the update is rejected.
    /// </summary>
    public const int TooManyRequestsStatusCode = 429;

    /// <summary>
    /// HTTP status code returned when the service is stopping and no longer accepts updates.
    /// </summary>
    public const int ServiceUnavailableStatusCode = 503;

    /// <summary>
    /// Name of the <c>Retry-After</c> response header.
    /// </summary>
    public const string RetryAfterHeaderName = "Retry-After";

    /// <summary>
    /// Value of the <c>Retry-After</c> header sent with a rejected update, in seconds.
    /// </summary>
    public const string QueueFullRetryAfterSeconds = "1";

    /// <summary>
    /// Message returned when the update queue is full.
    /// </summary>
    public const string QueueFullMessage = "Update queue is full, retry later.";

    /// <summary>
    /// Message returned while the service is stopping.
    /// </summary>
    public const string ServiceUnavailableMessage = "Bot is shutting down.";

    /// <summary>
    /// Log message for an update received while the service is stopping.
    /// </summary>
    public const string ServiceStoppingLogMessage = "Webhook update received while the service is stopping";

    public const string EndpointCalledLogMessage =
        "Webhook endpoint called - Path: {Path}, Method: {Method}";

    public const string RequestBodyTooLargeLogMessage =
        "Rejected webhook request: request body size {ContentLength} bytes exceeds maximum allowed {MaxSize} bytes";

    public const string EmptyRequestBodyLogMessage = "Received empty webhook request body";

    public const string RequestBodyReceivedLogMessage =
        "Webhook request body received - Length: {BodyLength} bytes";

    public const string ValidatingSecretTokenLogMessage =
        "Validating webhook request with secret token";

    public const string ValidationFailedLogMessage =
        "Webhook request validation failed - Invalid signature or parse error";

    public const string ValidationSucceededLogMessage =
        "Webhook request validated successfully - UpdateId: {UpdateId}, Type: {UpdateType}";

}

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

    public const string UpdateDispatchedLogMessage =
        "Webhook update dispatched successfully - UpdateId: {UpdateId}";
}

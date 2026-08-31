namespace TelegramBotFramework.Controllers;

/// <summary>
/// Constants for WebhookController.
/// </summary>
internal static class WebhookControllerConstants
{
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
}
#nullable enable
namespace TelegramBotFramework.Integration;

internal static class IWebhookServiceConstants
{
    /// <summary>
    /// The name of the header used for Telegram webhook secret token validation.
    /// </summary>
    public const string TelegramSecretTokenHeaderName = "X-Telegram-Bot-Api-Secret-Token";
}

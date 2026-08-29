namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// Interface for unit tests for the <see cref="WebhookHandler"/> extension methods.
/// </summary>
public interface IWebhookHandlerExtensionsTests
{
    void GetMessageText_MessageIsNull_ReturnsNull();
    void GetMessageText_UpdateIsNull_ThrowsArgumentNullException();
    void HasCallbackData_CallbackDataMatches_ReturnsTrue();
    void HasCallbackData_CallbackDataDoesNotMatch_ReturnsFalse();
    void GetChatId_MessageIsNull_Returns0();
    void GetUserId_MessageIsNull_Returns0();
}
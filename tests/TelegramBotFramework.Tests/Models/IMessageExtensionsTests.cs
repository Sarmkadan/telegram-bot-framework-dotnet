namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Interface for message extension method tests.
/// </summary>
public interface IMessageExtensionsTests
{
    void IsCommand_MessageIsCommand_ReturnsTrue();
    void IsCommand_MessageIsNotCommand_ReturnsFalse();
    void HasAttachments_MessageHasAttachments_ReturnsTrue();
    void GetTypeString_MessageHasType_ReturnsTypeString();
    void IsReply_MessageIsReply_ReturnsTrue();
}
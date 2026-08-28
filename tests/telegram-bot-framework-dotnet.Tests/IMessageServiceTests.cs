#nullable enable
using System.Threading.Tasks;

namespace TelegramBotFramework.Tests;

public interface IMessageServiceTests
{
    Task ProcessIncomingMessageAsync_WithValidMessage_ReturnsCreatedMessage();
    Task ProcessIncomingMessageAsync_WithInvalidMessage_ThrowsInvalidOperationException();
    Task GetMessageAsync_WithExistingMessageId_ReturnsMessage();
    Task GetMessageAsync_WithNonExistingMessageId_ReturnsNull();
    Task GetUserMessagesAsync_WithValidUserId_ReturnsMessages();
    Task GetFailedMessagesAsync_WithFailedMessages_ReturnsFailedMessages();
    Task MarkAsProcessedAsync_WithExistingMessageId_ReturnsTrue();
    Task MarkAsProcessedAsync_WithNonExistingMessageId_ReturnsFalse();
    Task MarkAsFailedAsync_WithExistingMessageId_ReturnsTrue();
    Task MarkAsFailedAsync_WithNonExistingMessageId_ReturnsFalse();
    Task GetUnprocessedMessageCountAsync_WithProcessingAndReceivedMessages_ReturnsCount();
    Task ArchiveOldMessagesAsync_WithOldMessages_ArchivesThem();
    Task SendPollAsync_WithValidInput_ReturnsCreatedMessage();
    Task SendPollAsync_WithInvalidChatId_ThrowsArgumentException();
    Task SendPollAsync_WithEmptyQuestion_ThrowsArgumentException();
    Task SendPollAsync_WithTooManyOptions_ThrowsArgumentException();
    Task SendPollAsync_WhenTelegramApiFails_ReturnsNull();
    Task SendPollAsync_WhenTelegramApiThrowsException_ReturnsNull();
    Task SendMediaGroupAsync_WithValidInput_ReturnsCreatedMessages();
}
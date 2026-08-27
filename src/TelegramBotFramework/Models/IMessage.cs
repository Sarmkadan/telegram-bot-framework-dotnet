namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a message in the bot system.
/// </summary>
public interface IMessage
{
    long MessageId { get; set; }
    long UserId { get; set; }
    long ChatId { get; set; }
    string Content { get; set; }
    MessageType Type { get; set; }
    MessageStatus Status { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? ProcessedAt { get; set; }
    string? CommandName { get; set; }
    Dictionary<string, object>? Metadata { get; set; }
    List<string>? AttachmentUrls { get; set; }
    bool IsEdited { get; set; }
    long? ReplyToMessageId { get; set; }
    int? ForwardedFromUserId { get; set; }

    void MarkAsProcessed();
    void MarkAsFailed(string errorMessage);
    long GetProcessingDurationMs();
    void SetMetadata(string key, object value);
    object? GetMetadata(string key);
    void AddAttachment(string url);
}
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a message in the bot system.
/// </summary>
public class Message
{
    public long MessageId { get; set; }

    public long UserId { get; set; }

    public long ChatId { get; set; }

    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public MessageStatus Status { get; set; } = MessageStatus.Received;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public string? CommandName { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public List<string>? AttachmentUrls { get; set; }

    public bool IsEdited { get; set; }

    public long? ReplyToMessageId { get; set; }

    public int? ForwardedFromUserId { get; set; }

    /// <summary>
    /// Marks the message as processed with timestamp.
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Status = MessageStatus.Processed;
    }

    /// <summary>
    /// Marks the message as failed.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = MessageStatus.Failed;
        SetMetadata("error", errorMessage);
    }

    /// <summary>
    /// Gets processing duration in milliseconds.
    /// </summary>
    public long GetProcessingDurationMs() =>
        ProcessedAt.HasValue
            ? (long)(ProcessedAt.Value - CreatedAt).TotalMilliseconds
            : -1;

    /// <summary>
    /// Sets metadata value.
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }

    /// <summary>
    /// Gets metadata value.
    /// </summary>
    public object? GetMetadata(string key) =>
        Metadata?.TryGetValue(key, out var value) == true ? value : null;

    /// <summary>
    /// Adds attachment URL.
    /// </summary>
    public void AddAttachment(string url)
    {
        AttachmentUrls ??= new List<string>();
        AttachmentUrls.Add(url);
    }

    /// <summary>
    /// Validates message data.
    /// </summary>
    public bool Validate()
    {
        if (UserId <= 0)
            throw new InvalidOperationException("UserId must be positive");

        if (ChatId <= 0)
            throw new InvalidOperationException("ChatId must be positive");

        if (string.IsNullOrWhiteSpace(Content))
            throw new InvalidOperationException("Message content cannot be empty");

        return true;
    }
}

public enum MessageType
{
    Text = 0,
    Photo = 1,
    Video = 2,
    Audio = 3,
    Document = 4,
    Sticker = 5,
    Location = 6,
    Contact = 7
}

public enum MessageStatus
{
    Received = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3,
    Archived = 4
}

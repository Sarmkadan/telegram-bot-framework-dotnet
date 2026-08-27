#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Builder for <see cref="Message"/> objects.
/// </summary>
public sealed class MessageBuilder
{
    private long _messageId;
    private long _userId;
    private long _chatId;
    private string _content = string.Empty;
    private MessageType _type = MessageType.Text;
    private MessageStatus _status = MessageStatus.Received;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _processedAt;
    private string? _commandName;
    private Dictionary<string, object>? _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBuilder"/> class.
    /// </summary>
    public MessageBuilder()
    {
    }

    /// <summary>
    /// Sets the message ID.
    /// </summary>
    /// <param name="messageId">The message ID.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithMessageId(long messageId)
    {
        _messageId = messageId;
        return this;
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is less than or equal to zero.</exception>
    public MessageBuilder WithUserId(long userId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be positive.", nameof(userId));

        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the chat ID.
    /// </summary>
    /// <param name="chatId">The chat ID.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chatId"/> is less than or equal to zero.</exception>
    public MessageBuilder WithChatId(long chatId)
    {
        if (chatId <= 0)
            throw new ArgumentException("ChatId must be positive.", nameof(chatId));

        _chatId = chatId;
        return this;
    }

    /// <summary>
    /// Sets the message content.
    /// </summary>
    /// <param name="content">The message content.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="content"/> is null, empty, or whitespace.</exception>
    public MessageBuilder WithContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));

        _content = content;
        return this;
    }

    /// <summary>
    /// Sets the message type.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithType(MessageType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the message status.
    /// </summary>
    /// <param name="status">The message status.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithStatus(MessageStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the processing timestamp.
    /// </summary>
    /// <param name="processedAt">The processing timestamp.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithProcessedAt(DateTime? processedAt)
    {
        _processedAt = processedAt;
        return this;
    }

    /// <summary>
    /// Sets the command name.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithCommandName(string? commandName)
    {
        _commandName = commandName;
        return this;
    }

    /// <summary>
    /// Sets the metadata.
    /// </summary>
    /// <param name="metadata">The metadata dictionary.</param>
    /// <returns>This builder instance.</returns>
    public MessageBuilder WithMetadata(Dictionary<string, object>? metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="MessageBuilder"/> pre-filled with values from an existing <see cref="Message"/>.
    /// </summary>
    /// <param name="template">The message to copy values from.</param>
    /// <returns>A new builder instance with values from the template.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static MessageBuilder From(Message template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new MessageBuilder()
            .WithMessageId(template.MessageId)
            .WithUserId(template.UserId)
            .WithChatId(template.ChatId)
            .WithContent(template.Content)
            .WithType(template.Type)
            .WithStatus(template.Status)
            .WithCreatedAt(template.CreatedAt)
            .WithProcessedAt(template.ProcessedAt)
            .WithCommandName(template.CommandName)
            .WithMetadata(template.Metadata);
    }

    /// <summary>
    /// Builds the <see cref="Message"/> instance with the current values.
    /// </summary>
    /// <returns>A configured <see cref="Message"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing or invalid.</exception>
    public Message Build()
    {
        // Validate required properties as per Message.Validate()
        if (_userId <= 0)
            throw new ArgumentException("UserId must be positive.", nameof(_userId));

        if (_chatId <= 0)
            throw new ArgumentException("ChatId must be positive.", nameof(_chatId));

        if (string.IsNullOrWhiteSpace(_content))
            throw new ArgumentException("Message content cannot be empty.", nameof(_content));

        return new Message
        {
            MessageId = _messageId,
            UserId = _userId,
            ChatId = _chatId,
            Content = _content,
            Type = _type,
            Status = _status,
            CreatedAt = _createdAt,
            ProcessedAt = _processedAt,
            CommandName = _commandName,
            Metadata = _metadata
        };
    }
}
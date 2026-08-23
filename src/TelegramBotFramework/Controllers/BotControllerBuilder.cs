#nullable enable
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Builder for creating <see cref="ProcessMessageRequest"/> instances with fluent syntax.
/// </summary>
public sealed class BotControllerBuilder
{
    private long _userId;
    private long _chatId;
    private string _firstName = string.Empty;
    private string? _lastName;
    private string _content = string.Empty;
    private MessageType _messageType = MessageType.Text;

    /// <summary>
    /// Sets the UserId.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is less than or equal to zero.</exception>
    public BotControllerBuilder WithUserId(long userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
        }

        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the ChatId.
    /// </summary>
    /// <param name="chatId">The chat identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chatId"/> is less than or equal to zero.</exception>
    public BotControllerBuilder WithChatId(long chatId)
    {
        if (chatId <= 0)
        {
            throw new ArgumentException("ChatId must be greater than zero.", nameof(chatId));
        }

        _chatId = chatId;
        return this;
    }

    /// <summary>
    /// Sets the FirstName.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BotControllerBuilder WithFirstName(string? firstName)
    {
        _firstName = firstName ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Sets the LastName.
    /// </summary>
    /// <param name="lastName">The last name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BotControllerBuilder WithLastName(string? lastName)
    {
        _lastName = lastName;
        return this;
    }

    /// <summary>
    /// Sets the Content.
    /// </summary>
    /// <param name="content">The message content.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="content"/> is null or empty.</exception>
    public BotControllerBuilder WithContent(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);
        _content = content;
        return this;
    }

    /// <summary>
    /// Sets the MessageType.
    /// </summary>
    /// <param name="messageType">The message type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BotControllerBuilder WithMessageType(MessageType messageType)
    {
        _messageType = messageType;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="BotControllerBuilder"/> pre-filled with values from an existing <see cref="ProcessMessageRequest"/>.
    /// </summary>
    /// <param name="template">The template request to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static BotControllerBuilder From(ProcessMessageRequest template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new BotControllerBuilder
        {
            _userId = template.UserId,
            _chatId = template.ChatId,
            _firstName = template.FirstName,
            _lastName = template.LastName,
            _content = template.Content,
            _messageType = template.MessageType
        };
    }

    /// <summary>
    /// Builds and validates the <see cref="ProcessMessageRequest"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="ProcessMessageRequest"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing or invalid.</exception>
    public ProcessMessageRequest Build()
    {
        // Validate required properties
        if (_userId <= 0)
        {
            throw new ArgumentException("UserId must be set and greater than zero.", nameof(_userId));
        }

        if (_chatId <= 0)
        {
            throw new ArgumentException("ChatId must be set and greater than zero.", nameof(_chatId));
        }

        if (string.IsNullOrEmpty(_content))
        {
            throw new ArgumentException("Content must not be null or empty.", nameof(_content));
        }

        return new ProcessMessageRequest
        {
            UserId = _userId,
            ChatId = _chatId,
            FirstName = _firstName,
            LastName = _lastName,
            Content = _content,
            MessageType = _messageType
        };
    }
}
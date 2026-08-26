#nullable enable
using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Models;

/// <summary>
/// A builder class for creating <see cref="ExecutionContext"/> instances.
/// </summary>
public sealed class ExecutionContextBuilder
{
    private string? _contextId;
    private long _userId;
    private long _chatId;
    private BotUser? _user;
    private UserSession? _session;
    private Command? _command;
    private Message? _message;
    private Dictionary<string, object>? _parameters;
    private DateTime _createdAt;
    private Dictionary<string, object> _states = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionContextBuilder"/> class.
    /// </summary>
    public ExecutionContextBuilder()
    {
        _contextId = Guid.NewGuid().ToString();
        _createdAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the context ID.
    /// </summary>
    /// <param name="value">The context ID.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithContextId(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        _contextId = value;
        return this;
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="value">The user ID.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithUserId(long value)
    {
        _userId = value;
        return this;
    }

    /// <summary>
    /// Sets the chat ID.
    /// </summary>
    /// <param name="value">The chat ID.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithChatId(long value)
    {
        _chatId = value;
        return this;
    }

    /// <summary>
    /// Sets the user.
    /// </summary>
    /// <param name="value">The user.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithUser(BotUser? value)
    {
        _user = value;
        return this;
    }

    /// <summary>
    /// Sets the session.
    /// </summary>
    /// <param name="value">The session.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithSession(UserSession? value)
    {
        _session = value;
        return this;
    }

    /// <summary>
    /// Sets the command.
    /// </summary>
    /// <param name="value">The command.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithCommand(Command? value)
    {
        _command = value;
        return this;
    }

    /// <summary>
    /// Sets the message.
    /// </summary>
    /// <param name="value">The message.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithMessage(Message? value)
    {
        _message = value;
        return this;
    }

    /// <summary>
    /// Sets the parameters.
    /// </summary>
    /// <param name="value">The parameters.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithParameters(Dictionary<string, object>? value)
    {
        _parameters = value;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    /// <param name="value">The creation timestamp.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithCreatedAt(DateTime value)
    {
        _createdAt = value;
        return this;
    }

    /// <summary>
    /// Sets the states.
    /// </summary>
    /// <param name="value">The states.</param>
    /// <returns>The builder instance.</returns>
    public ExecutionContextBuilder WithStates(Dictionary<string, object> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _states = value;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="ExecutionContextBuilder"/> instance pre-filled from a template.
    /// </summary>
    /// <param name="template">The template <see cref="ExecutionContext"/>.</param>
    /// <returns>A new <see cref="ExecutionContextBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if template is null.</exception>
    public static ExecutionContextBuilder From(ExecutionContext template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ExecutionContextBuilder()
            .WithContextId(template.ContextId)
            .WithUserId(template.UserId)
            .WithChatId(template.ChatId)
            .WithUser(template.User)
            .WithSession(template.Session)
            .WithCommand(template.Command)
            .WithMessage(template.Message)
            .WithParameters(template.Parameters)
            .WithCreatedAt(template.CreatedAt)
            .WithStates(template.States);
    }

    /// <summary>
    /// Builds the <see cref="ExecutionContext"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="ExecutionContext"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ExecutionContext Build()
    {
        if (_userId <= 0)
        {
            throw new ArgumentException("UserId must be positive", nameof(_userId));
        }

        if (_chatId <= 0)
        {
            throw new ArgumentException("ChatId must be positive", nameof(_chatId));
        }

        return new ExecutionContext
        {
            ContextId = _contextId ?? Guid.NewGuid().ToString(),
            UserId = _userId,
            ChatId = _chatId,
            User = _user,
            Session = _session,
            Command = _command,
            Message = _message,
            Parameters = _parameters,
            CreatedAt = _createdAt,
            States = _states
        };
    }
}
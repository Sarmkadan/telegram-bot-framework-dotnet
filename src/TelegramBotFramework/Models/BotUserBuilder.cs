using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Models;

/// <summary>
/// A builder class for creating <see cref="BotUser"/> instances.
/// </summary>
public sealed class BotUserBuilder
{
    private long _telegramId;
    private string? _firstName;
    private string? _lastName;
    private string? _username;
    private string? _phoneNumber;
    private UserStatus _status = UserStatus.Active;
    private UserRole _role = UserRole.User;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private DateTime? _lastActivityAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotUserBuilder"/> class.
    /// </summary>
    public BotUserBuilder() { }

    /// <summary>
    /// Sets the Telegram ID.
    /// </summary>
    /// <param name="value">The Telegram ID.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithTelegramId(long value)
    {
        _telegramId = value;
        return this;
    }

    /// <summary>
    /// Sets the first name.
    /// </summary>
    /// <param name="value">The first name.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace.</exception>
    public BotUserBuilder WithFirstName(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        _firstName = value;
        return this;
    }

    /// <summary>
    /// Sets the last name.
    /// </summary>
    /// <param name="value">The last name.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithLastName(string? value)
    {
        _lastName = value;
        return this;
    }

    /// <summary>
    /// Sets the username.
    /// </summary>
    /// <param name="value">The username.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithUsername(string? value)
    {
        _username = value;
        return this;
    }

    /// <summary>
    /// Sets the phone number.
    /// </summary>
    /// <param name="value">The phone number.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithPhoneNumber(string? value)
    {
        _phoneNumber = value;
        return this;
    }

    /// <summary>
    /// Sets the user status.
    /// </summary>
    /// <param name="value">The user status.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithStatus(UserStatus value)
    {
        _status = value;
        return this;
    }

    /// <summary>
    /// Sets the user role.
    /// </summary>
    /// <param name="value">The user role.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithRole(UserRole value)
    {
        _role = value;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    /// <param name="value">The creation timestamp.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithCreatedAt(DateTime value)
    {
        _createdAt = value;
        return this;
    }

    /// <summary>
    /// Sets the update timestamp.
    /// </summary>
    /// <param name="value">The update timestamp.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithUpdatedAt(DateTime value)
    {
        _updatedAt = value;
        return this;
    }

    /// <summary>
    /// Sets the last activity timestamp.
    /// </summary>
    /// <param name="value">The last activity timestamp.</param>
    /// <returns>The builder instance.</returns>
    public BotUserBuilder WithLastActivityAt(DateTime? value)
    {
        _lastActivityAt = value;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="BotUserBuilder"/> instance pre-filled from a template.
    /// </summary>
    /// <param name="template">The template <see cref="BotUser"/>.</param>
    /// <returns>A new <see cref="BotUserBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if template is null.</exception>
    public static BotUserBuilder From(BotUser template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new BotUserBuilder()
            .WithTelegramId(template.TelegramId)
            .WithFirstName(template.FirstName ?? string.Empty)
            .WithLastName(template.LastName)
            .WithUsername(template.Username)
            .WithPhoneNumber(template.PhoneNumber)
            .WithStatus(template.Status)
            .WithRole(template.Role)
            .WithCreatedAt(template.CreatedAt)
            .WithUpdatedAt(template.UpdatedAt)
            .WithLastActivityAt(template.LastActivityAt);
    }

    /// <summary>
    /// Builds the <see cref="BotUser"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="BotUser"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public BotUser Build()
    {
        if (_telegramId <= 0)
        {
            throw new ArgumentException("TelegramId must be positive", nameof(_telegramId));
        }

        if (string.IsNullOrWhiteSpace(_firstName))
        {
            throw new ArgumentException("FirstName cannot be empty", nameof(_firstName));
        }

        return new BotUser
        {
            TelegramId = _telegramId,
            FirstName = _firstName,
            LastName = _lastName,
            Username = _username,
            PhoneNumber = _phoneNumber,
            Status = _status,
            Role = _role,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            LastActivityAt = _lastActivityAt
        };
    }
}

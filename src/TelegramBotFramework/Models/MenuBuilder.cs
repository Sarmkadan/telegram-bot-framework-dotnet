#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides a fluent interface for creating <see cref="Menu"/> instances.
/// </summary>
public sealed class MenuBuilder
{
    private string _id = string.Empty;
    private string _title = string.Empty;
    private string? _description;
    private MenuType _type = MenuType.Inline;
    private List<MenuButton> _buttons = new();
    private bool _isActive = true;
    private int _displayOrder;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private string? _backMenuId;

    /// <summary>
    /// Sets the menu identifier.
    /// </summary>
    /// <param name="id">The menu identifier.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null, empty, or whitespace.</exception>
    public MenuBuilder WithId(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the menu title.
    /// </summary>
    /// <param name="title">The menu title.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> is null, empty, or whitespace.</exception>
    public MenuBuilder WithTitle(string title)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the menu description.
    /// </summary>
    /// <param name="description">The menu description.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the menu type.
    /// </summary>
    /// <param name="type">The menu type.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithType(MenuType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the menu buttons.
    /// </summary>
    /// <param name="buttons">The menu buttons.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="buttons"/> is null.</exception>
    public MenuBuilder WithButtons(IEnumerable<MenuButton> buttons)
    {
        ArgumentNullException.ThrowIfNull(buttons);
        _buttons = buttons.ToList();
        return this;
    }

    /// <summary>
    /// Sets whether the menu is active.
    /// </summary>
    /// <param name="isActive">Whether the menu is active.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Sets the menu display order.
    /// </summary>
    /// <param name="displayOrder">The menu display order.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    /// <summary>
    /// Sets the menu creation timestamp.
    /// </summary>
    /// <param name="createdAt">The menu creation timestamp.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the menu last update timestamp.
    /// </summary>
    /// <param name="updatedAt">The menu last update timestamp.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    /// <summary>
    /// Sets the identifier of the back menu.
    /// </summary>
    /// <param name="backMenuId">The identifier of the back menu.</param>
    /// <returns>This builder instance.</returns>
    public MenuBuilder WithBackMenuId(string? backMenuId)
    {
        _backMenuId = backMenuId;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="MenuBuilder"/> pre-filled with values from an existing <see cref="Menu"/> instance.
    /// </summary>
    /// <param name="template">The menu instance to copy values from.</param>
    /// <returns>A new <see cref="MenuBuilder"/> initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static MenuBuilder From(Menu template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new MenuBuilder()
            .WithId(template.Id)
            .WithTitle(template.Title)
            .WithDescription(template.Description)
            .WithType(template.Type)
            .WithButtons(template.Buttons)
            .WithIsActive(template.IsActive)
            .WithDisplayOrder(template.DisplayOrder)
            .WithCreatedAt(template.CreatedAt)
            .WithUpdatedAt(template.UpdatedAt)
            .WithBackMenuId(template.BackMenuId);
    }

    /// <summary>
    /// Builds and returns a <see cref="Menu"/> instance with the configured values.
    /// </summary>
    /// <returns>A configured <see cref="Menu"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public Menu Build()
    {
        if (string.IsNullOrWhiteSpace(_id))
            throw new ArgumentException("Menu Id is required", nameof(_id));

        if (string.IsNullOrWhiteSpace(_title))
            throw new ArgumentException("Menu Title is required", nameof(_title));

        if (_buttons == null || _buttons.Count == 0)
            throw new ArgumentException("Menu must have at least one button", nameof(_buttons));

        return new Menu
        {
            Id = _id,
            Title = _title,
            Description = _description,
            Type = _type,
            Buttons = _buttons,
            IsActive = _isActive,
            DisplayOrder = _displayOrder,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            BackMenuId = _backMenuId
        };
    }
}
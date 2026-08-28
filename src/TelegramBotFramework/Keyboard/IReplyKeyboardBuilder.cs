#nullable enable

namespace TelegramBotFramework.Keyboard;

using Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// Fluent builder interface for constructing Telegram reply keyboard markups.
/// </summary>
public interface IReplyKeyboardBuilder
{
    /// <summary>
    /// Adds a standard text button to the current row.
    /// </summary>
    /// <param name="text">Button label shown to the user.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or whitespace.
    /// </exception>
    IReplyKeyboardBuilder AddButton(string text);

    /// <summary>
    /// Adds a button to the current row with custom configuration.
    /// </summary>
    /// <param name="text">Button label shown to the user.</param>
    /// <param name="configure">Action to configure the button.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or whitespace.
    /// </exception>
    IReplyKeyboardBuilder AddButton(string text, Action<ReplyKeyboardButton> configure);

    /// <summary>
    /// Forces the next button(s) onto a new row, even if the current row is not full.
    /// </summary>
    IReplyKeyboardBuilder NewRow();

    /// <summary>
    /// Sets the keyboard to be one-time, meaning it will automatically be removed
    /// from the chat after the user selects a button.
    /// </summary>
    IReplyKeyboardBuilder OneTime();

    /// <summary>
    /// Sets the keyboard to be persistent, meaning it will remain visible after
    /// the user selects a button.
    /// </summary>
    IReplyKeyboardBuilder Persistent();

    /// <summary>
    /// Sets the keyboard to be resized to fit its content, instead of always
    /// using the maximum size.
    /// </summary>
    IReplyKeyboardBuilder Resize();

    /// <summary>
    /// Sets the keyboard to use the full width of the screen.
    /// </summary>
    IReplyKeyboardBuilder NoResize();

    /// <summary>
    /// Builds and returns an immutable <see cref="ReplyKeyboardMarkup"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no buttons have been added.</exception>
    ReplyKeyboardMarkup Build();

    /// <summary>
    /// Converts the current builder state to a <see cref="Models.Menu"/> for storage in the menu repository.
    /// </summary>
    /// <param name="menuId">Unique identifier for the menu.</param>
    /// <param name="title">Human-readable menu title.</param>
    Models.Menu ToMenu(string menuId, string title);
}
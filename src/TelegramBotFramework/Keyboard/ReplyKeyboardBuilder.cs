#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Fluent builder for constructing Telegram reply keyboard markups.
/// Supports standard text buttons, request contact, request location, and request poll buttons,
/// organised into rows automatically or explicitly via <see cref="NewRow"/>.
/// </summary>
/// <example>
/// <code>
/// var keyboard = ReplyKeyboardBuilder.Create()
///     .AddButton("📍 Share Location", button => button.RequestLocation = true)
///     .AddButton("📞 Share Contact", button => button.RequestContact = true)
///     .NewRow()
///     .AddButton("Help")
///     .AddButton("Settings")
///     .OneTime()
///     .Resize()
///     .Build();
///
/// // Use with TelegramApiClient:
/// await apiClient.SendMessageWithReplyKeyboardAsync(chatId, "Choose an option:", keyboard);
/// </code>
/// </example>
public sealed class ReplyKeyboardBuilder
{
    private readonly List<List<ReplyKeyboardButton>> _rows = new();
    private List<ReplyKeyboardButton> _currentRow = new();
    private readonly int _maxButtonsPerRow;
    private bool _isPersistent;
    private bool _resize;

    /// <summary>
    /// Initialises a new <see cref="ReplyKeyboardBuilder"/>.
    /// </summary>
    /// <param name="maxButtonsPerRow">
    /// Maximum buttons placed on a row before automatically starting a new one.
    /// Defaults to <c>2</c>. Use <see cref="NewRow"/> to force an earlier row break.
    /// </param>
    public ReplyKeyboardBuilder(int maxButtonsPerRow = 2)
    {
        if (maxButtonsPerRow < 1)
            throw new ArgumentOutOfRangeException(nameof(maxButtonsPerRow), "Must be at least 1.");

        _maxButtonsPerRow = maxButtonsPerRow;
    }

    /// <summary>Creates a new builder instance with the default row width.</summary>
    public static ReplyKeyboardBuilder Create(int maxButtonsPerRow = 2) => new(maxButtonsPerRow);

    // -------------------------------------------------------------------------
    // Button adders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a standard text button to the current row.
    /// </summary>
    /// <param name="text">Button label shown to the user.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or whitespace.
    /// </exception>
    public ReplyKeyboardBuilder AddButton(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Button text cannot be empty.", nameof(text));

        return AppendButton(new ReplyKeyboardButton { Text = text });
    }

    /// <summary>
    /// Adds a button to the current row with custom configuration.
    /// </summary>
    /// <param name="configure">Action to configure the button.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public ReplyKeyboardBuilder AddButton(string text, Action<ReplyKeyboardButton> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Button text cannot be empty.", nameof(text));

        var button = new ReplyKeyboardButton { Text = text };
        configure(button);
        return AppendButton(button);
    }

    /// <summary>
    /// Forces the next button(s) onto a new row, even if the current row is not full.
    /// </summary>
    public ReplyKeyboardBuilder NewRow()
    {
        FlushCurrentRow();
        return this;
    }

    // -------------------------------------------------------------------------
    // Keyboard configuration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets the keyboard to be one-time, meaning it will automatically be removed
    /// from the chat after the user selects a button.
    /// </summary>
    public ReplyKeyboardBuilder OneTime()
    {
        _isPersistent = false;
        return this;
    }

    /// <summary>
    /// Sets the keyboard to be persistent, meaning it will remain visible after
    /// the user selects a button.
    /// </summary>
    public ReplyKeyboardBuilder Persistent()
    {
        _isPersistent = true;
        return this;
    }

    /// <summary>
    /// Sets the keyboard to be resized to fit its content, instead of always
    /// using the maximum size.
    /// </summary>
    public ReplyKeyboardBuilder Resize()
    {
        _resize = true;
        return this;
    }

    /// <summary>
    /// Sets the keyboard to use the full width of the screen.
    /// </summary>
    public ReplyKeyboardBuilder NoResize()
    {
        _resize = false;
        return this;
    }

    // -------------------------------------------------------------------------
    // Build
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds and returns an immutable <see cref="ReplyKeyboardMarkup"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no buttons have been added.</exception>
    public ReplyKeyboardMarkup Build()
    {
        FlushCurrentRow();

        if (_rows.Count == 0)
            throw new InvalidOperationException("Cannot build an empty keyboard — add at least one button.");

        // Convert our ReplyKeyboardButton to Telegram.Bot's KeyboardButton
        var keyboardButtons = _rows
            .Select(row => row.Select(btn => new Telegram.Bot.Types.ReplyMarkups.KeyboardButton(btn.Text)
            {
                RequestContact = btn.RequestContact,
                RequestLocation = btn.RequestLocation
            }).ToList())
            .ToList();

        return new ReplyKeyboardMarkup(keyboardButtons)
        {
            ResizeKeyboard = _resize,
            OneTimeKeyboard = !_isPersistent,
            InputFieldPlaceholder = null,
            Selective = false
        };
    }

    /// <summary>
    /// Converts the current builder state to a <see cref="Models.Menu"/> for storage in the menu repository.
    /// </summary>
    /// <param name="menuId">Unique identifier for the menu.</param>
    /// <param name="title">Human-readable menu title.</param>
    public Models.Menu ToMenu(string menuId, string title)
    {
        if (string.IsNullOrWhiteSpace(menuId))
            throw new ArgumentException("Menu ID cannot be empty.", nameof(menuId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var markup = Build();
        var menu = new Models.Menu
        {
            Id = menuId,
            Title = title,
            Type = Models.MenuType.ReplyKeyboard,
            IsActive = true,
            MaxButtonsPerRow = _maxButtonsPerRow
        };

        int order = 0;
        foreach (var row in markup.Keyboard)
        {
            foreach (var btn in row)
            {
                menu.Buttons.Add(new Models.MenuButton
                {
                    Label = btn.Text,
                    CallbackData = btn.Text, // Use text as callback data for reply keyboards
                    Action = Models.ButtonAction.Callback,
                    DisplayOrder = order++,
                    IsVisible = true,
                    // Note: RequestContact, RequestLocation, RequestPoll are not stored in Menu model
                    // They are only relevant for the ReplyKeyboardMarkup itself
                });
            }
        }

        return menu;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private ReplyKeyboardBuilder AppendButton(ReplyKeyboardButton button)
    {
        if (_currentRow.Count >= _maxButtonsPerRow)
            FlushCurrentRow();

        _currentRow.Add(button);
        return this;
    }

    private void FlushCurrentRow()
    {
        if (_currentRow.Count > 0)
        {
            _rows.Add(_currentRow);
            _currentRow = new List<ReplyKeyboardButton>();
        }
    }
}
